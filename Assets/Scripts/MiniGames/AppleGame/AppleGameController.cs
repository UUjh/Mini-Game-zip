using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 사과게임: 드래그 직사각형으로 겹치는 칸의 합이 10이면 제거. 블록당 1점.
/// 숫자 생성 규칙은 선택된 AppleGameStageData.useEasyFillRules 로만 결정한다.
/// </summary>
public class AppleGameController : MiniGameBase
{
    [Header("AppleGame 설정")]
    [SerializeField] private AppleGameStageData[] stageDatas;
    [SerializeField] private int activeStageIndex;

    private AppleGameStageData activeStageData;
    private int activeColumns;
    private int activeRows;
    private float stageTimeLimit;

    [Header("씬 UI")]
    [SerializeField] private RectTransform gridPanelRect;
    [SerializeField] private RectTransform dragPanelRect;
    [SerializeField] private RectTransform selectionRectTransform;
    [SerializeField] private TextMeshProUGUI scoreHudText;
    [SerializeField] private TextMeshProUGUI timerHudText;
    [SerializeField] private Image timerBarFill;
    [SerializeField] private GameObject countdownOverlay;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultScoreText;
    [SerializeField] private TextMeshProUGUI resultRankText;
    [SerializeField] private TextMeshProUGUI resultHighText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;

    private int[,] grid;
    private RectTransform[,] cellRects;
    private Image[,] cellImages;
    private TextMeshProUGUI[,] cellTexts;

    private System.Random rng;
    private float remainingTime;

    private bool dragPanelDragActive;
    private Vector2 dragStartScreen;
    private Vector2 dragCurrentScreen;
    private Canvas rootCanvas;

    private static readonly Color CellEmptyColor = new Color(0f, 0f, 0f, 0f);

    protected override void Awake()
    {
        base.Awake();
        BindSceneUiReferences();
    }

    public override void OnGameInit()
    {
        base.OnGameInit();
        Result.gameName = "AppleGame";

        rng = new System.Random(System.Environment.TickCount ^ GetInstanceID());

        if (stageDatas == null || stageDatas.Length == 0)
        {
            Debug.LogError("[AppleGame] stageDatas에 AppleGameStageData를 1개 이상 넣어 주세요.");
            return;
        }

        activeStageIndex = Mathf.Clamp(activeStageIndex, 0, stageDatas.Length - 1);
        activeStageData = stageDatas[activeStageIndex];
        if (activeStageData == null)
        {
            Debug.LogError("[AppleGame] stageDatas[" + activeStageIndex + "]가 비어 있습니다.");
            return;
        }

        maxScore = 100;
        stageTimeLimit = activeStageData.timeLimit;
        remainingTime = stageTimeLimit;
        dragPanelDragActive = false;

        ApplyGridSizeForDifficulty();

        grid = new int[activeRows, activeColumns];
        cellRects = new RectTransform[activeRows, activeColumns];
        cellImages = new Image[activeRows, activeColumns];
        cellTexts = new TextMeshProUGUI[activeRows, activeColumns];

        AppleGridGenerator.FillGrid(grid, activeStageData, rng);

        if (!BuildGridCellsFromPrefab())
        {
            return;
        }

        if (!BindDragPanel())
        {
            return;
        }

        WireResultButtons();

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (countdownOverlay != null)
        {
            countdownOverlay.SetActive(false);
        }

        if (scoreHudText != null)
        {
            scoreHudText.text = "0";
        }

        if (timerHudText != null)
        {
            timerHudText.text = Mathf.CeilToInt(remainingTime).ToString();
        }

        if (timerBarFill != null)
        {
            timerBarFill.fillAmount = 1f;
        }

        HideSelectionVisual();
        RefreshAllCellVisuals();
    }

    private void BindSceneUiReferences()
    {
        if (gridPanelRect == null)
        {
            gridPanelRect = FindRectTransform("AppleGameRoot/GridPanel") ?? FindRectTransform("GridPanel");
        }

        if (dragPanelRect == null)
        {
            dragPanelRect = FindRectTransform("AppleGameRoot/DragPanel") ?? FindRectTransform("DragPanel");
        }

        if (selectionRectTransform == null && dragPanelRect != null)
        {
            Transform sel = dragPanelRect.parent != null ? dragPanelRect.parent.Find("Selection") : null;
            if (sel != null)
            {
                selectionRectTransform = sel.GetComponent<RectTransform>();
            }
        }

        if (scoreHudText == null)
        {
            scoreHudText = FindTmp("AppleGameRoot/HUD/ScoreText") ?? FindTmp("HUD/ScoreText");
        }

        if (timerHudText == null)
        {
            timerHudText = FindTmp("AppleGameRoot/HUD/TimerText") ?? FindTmp("HUD/TimerText");
        }

        if (dragPanelRect != null)
        {
            rootCanvas = dragPanelRect.GetComponentInParent<Canvas>();
        }
    }

    private RectTransform FindRectTransform(string path)
    {
        GameObject go = GameObject.Find(path);
        if (go == null)
        {
            return null;
        }

        return go.GetComponent<RectTransform>();
    }

    private TextMeshProUGUI FindTmp(string path)
    {
        GameObject go = GameObject.Find(path);
        if (go == null)
        {
            return null;
        }

        return go.GetComponent<TextMeshProUGUI>();
    }

    private bool BuildGridCellsFromPrefab()
    {
        if (gridPanelRect == null)
        {
            Debug.LogError("[AppleGame] GridPanel(RectTransform)이 없습니다.");
            return false;
        }

        if (activeStageData.cellPrefab == null)
        {
            Debug.LogError("[AppleGame] AppleGameStageData에 AppleCell 프리팹을 넣어 주세요.");
            return false;
        }

        ClearGridPanelChildren();

        GridLayoutGroup glg = gridPanelRect.GetComponent<GridLayoutGroup>();
        if (glg == null)
        {
            Debug.LogError("[AppleGame] GridLayoutGroup이 없습니다.");
            return false;
        }

        glg.cellSize = activeStageData.gridCellSize;

        int need = activeRows * activeColumns;
        for (int i = 0; i < need; i++)
        {
            AppleCell cell = Instantiate(activeStageData.cellPrefab, gridPanelRect);
            int r = i / activeColumns;
            int c = i % activeColumns;
            cell.gameObject.name = "Cell_" + r + "_" + c;

            Image img = cell.BackgroundImage;
            TextMeshProUGUI tmp = cell.NumberText;

            if (img != null)
            {
                cellRects[r, c] = img.rectTransform;
                cellImages[r, c] = img;
                img.raycastTarget = false;
            }
            else
            {
                cellRects[r, c] = cell.GetComponent<RectTransform>();
                cellImages[r, c] = null;
            }

            cellTexts[r, c] = tmp;
        }

        return true;
    }

    private void ClearGridPanelChildren()
    {
        if (gridPanelRect == null)
        {
            return;
        }

        for (int i = gridPanelRect.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(gridPanelRect.GetChild(i).gameObject);
        }
    }

    private bool BindDragPanel()
    {
        if (dragPanelRect == null)
        {
            Debug.LogError("[AppleGame] DragPanel(RectTransform)이 없습니다.");
            return false;
        }

        AppleGameDragPanel drag = dragPanelRect.GetComponent<AppleGameDragPanel>();
        if (drag == null)
        {
            drag = dragPanelRect.gameObject.AddComponent<AppleGameDragPanel>();
        }

        drag.appleGameController = this;
        return true;
    }

    private void WireResultButtons()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(Retry);
        }

        if (titleButton != null)
        {
            titleButton.onClick.RemoveAllListeners();
            titleButton.onClick.AddListener(BackToTitle);
        }
    }

    private void ApplyGridSizeForDifficulty()
    {
        activeColumns = activeStageData.gridColumns;
        activeRows = activeStageData.gridRows;
    }

    protected override void OnCountdownTick(int secondsLeft)
    {
        if (countdownOverlay != null)
        {
            countdownOverlay.SetActive(true);
        }

        if (countdownText != null)
        {
            countdownText.text = secondsLeft.ToString();
        }
    }

    protected override void OnCountdownFinished()
    {
        if (countdownOverlay != null)
        {
            countdownOverlay.SetActive(false);
        }
    }

    public override void OnPlayStart()
    {
        base.OnPlayStart();
        if (countdownOverlay != null)
        {
            countdownOverlay.SetActive(false);
        }
    }

    public override void OnPlayUpdate()
    {
        remainingTime -= Time.deltaTime;
        if (remainingTime < 0f)
        {
            remainingTime = 0f;
        }

        if (timerHudText != null)
        {
            timerHudText.text = Mathf.CeilToInt(remainingTime).ToString();
        }

        if (timerBarFill != null && stageTimeLimit > 0.001f)
        {
            timerBarFill.fillAmount = remainingTime / stageTimeLimit;
        }

        if (remainingTime <= 0f)
        {
            EndGame();
        }
    }

    public override void OnResult(GameResult result)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        if (resultScoreText != null)
        {
            resultScoreText.text = "점수: " + result.score;
        }

        if (resultRankText != null)
        {
            resultRankText.text = "등급: " + result.rank;
        }

        if (resultHighText != null)
        {
            resultHighText.text = "최고: " + result.highScore;
        }
    }

    public override void OnGameCleanUp()
    {
        base.OnGameCleanUp();
        dragPanelDragActive = false;
        HideSelectionVisual();
    }

    public void OnDragPanelBegin(Vector2 screenPosition)
    {
        dragPanelDragActive = true;
        dragStartScreen = screenPosition;
        dragCurrentScreen = screenPosition;
        UpdateSelectionVisual();
    }

    public void OnDragPanelDrag(Vector2 screenPosition)
    {
        dragCurrentScreen = screenPosition;
        UpdateSelectionVisual();
    }

    public void OnDragPanelEnd(Vector2 screenPosition)
    {
        dragCurrentScreen = screenPosition;
        dragPanelDragActive = false;
        TryCommitSelection();
        HideSelectionVisual();
    }

    private void TryCommitSelection()
    {
        float xMin = Mathf.Min(dragStartScreen.x, dragCurrentScreen.x);
        float xMax = Mathf.Max(dragStartScreen.x, dragCurrentScreen.x);
        float yMin = Mathf.Min(dragStartScreen.y, dragCurrentScreen.y);
        float yMax = Mathf.Max(dragStartScreen.y, dragCurrentScreen.y);

        if (xMax - xMin < 8f || yMax - yMin < 8f)
        {
            return;
        }

        Rect sel = Rect.MinMaxRect(xMin, yMin, xMax, yMax);

        System.Collections.Generic.List<int> rs = new System.Collections.Generic.List<int>();
        System.Collections.Generic.List<int> cs = new System.Collections.Generic.List<int>();
        int sum = 0;

        Camera cam = rootCanvas != null ? rootCanvas.worldCamera : null;

        for (int r = 0; r < activeRows; r++)
        {
            for (int c = 0; c < activeColumns; c++)
            {
                if (grid[r, c] == 0)
                {
                    continue;
                }

                RectTransform crt = cellRects[r, c];
                if (crt == null)
                {
                    continue;
                }

                Rect cellScreen = GetScreenRect(crt, cam);
                if (!sel.Overlaps(cellScreen, true))
                {
                    continue;
                }

                sum += grid[r, c];
                rs.Add(r);
                cs.Add(c);
            }
        }

        if (sum != 10 || rs.Count == 0)
        {
            return;
        }

        int removed = rs.Count;
        for (int i = 0; i < rs.Count; i++)
        {
            grid[rs[i], cs[i]] = 0;
        }

        Result.score += removed;
        if (scoreHudText != null)
        {
            scoreHudText.text = Result.score.ToString();
        }

        RefreshAllCellVisuals();
    }

    private static Rect GetScreenRect(RectTransform rt, Camera cam)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        for (int i = 0; i < 4; i++)
        {
            Vector2 sp = RectTransformUtility.WorldToScreenPoint(cam, corners[i]);
            if (sp.x < minX)
            {
                minX = sp.x;
            }

            if (sp.x > maxX)
            {
                maxX = sp.x;
            }

            if (sp.y < minY)
            {
                minY = sp.y;
            }

            if (sp.y > maxY)
            {
                maxY = sp.y;
            }
        }

        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }

    private void RefreshAllCellVisuals()
    {
        for (int r = 0; r < activeRows; r++)
        {
            for (int c = 0; c < activeColumns; c++)
            {
                int v = grid[r, c];
                TextMeshProUGUI tmp = cellTexts[r, c];
                Image img = cellImages[r, c];

                if (tmp != null)
                {
                    tmp.text = v == 0 ? string.Empty : v.ToString();
                }

                if (img != null && v == 0)
                {
                    img.color = CellEmptyColor;
                }
            }
        }
    }

    private void UpdateSelectionVisual()
    {
        if (selectionRectTransform == null)
        {
            return;
        }

        RectTransform parentRt = selectionRectTransform.parent as RectTransform;
        if (parentRt == null)
        {
            return;
        }

        Camera cam = rootCanvas != null ? rootCanvas.worldCamera : null;

        float xMin = Mathf.Min(dragStartScreen.x, dragCurrentScreen.x);
        float xMax = Mathf.Max(dragStartScreen.x, dragCurrentScreen.x);
        float yMin = Mathf.Min(dragStartScreen.y, dragCurrentScreen.y);
        float yMax = Mathf.Max(dragStartScreen.y, dragCurrentScreen.y);

        Vector2 a, b, c2, d;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, new Vector2(xMin, yMin), cam, out a);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, new Vector2(xMin, yMax), cam, out b);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, new Vector2(xMax, yMin), cam, out c2);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRt, new Vector2(xMax, yMax), cam, out d);

        float lx = Mathf.Min(Mathf.Min(a.x, b.x), Mathf.Min(c2.x, d.x));
        float ly = Mathf.Min(Mathf.Min(a.y, b.y), Mathf.Min(c2.y, d.y));
        float hx = Mathf.Max(Mathf.Max(a.x, b.x), Mathf.Max(c2.x, d.x));
        float hy = Mathf.Max(Mathf.Max(a.y, b.y), Mathf.Max(c2.y, d.y));

        Vector2 center = new Vector2((lx + hx) * 0.5f, (ly + hy) * 0.5f);
        Vector2 size = new Vector2(hx - lx, hy - ly);

        selectionRectTransform.anchoredPosition = center;
        selectionRectTransform.sizeDelta = size;

        Image selImg = selectionRectTransform.GetComponent<Image>();
        if (selImg != null)
        {
            Color col = selImg.color;
            col.a = 0.35f;
            selImg.color = col;
        }
    }

    private void HideSelectionVisual()
    {
        if (selectionRectTransform == null)
        {
            return;
        }

        selectionRectTransform.sizeDelta = Vector2.zero;

        Image selImg = selectionRectTransform.GetComponent<Image>();
        if (selImg != null)
        {
            Color col = selImg.color;
            col.a = 0f;
            selImg.color = col;
        }
    }
}

/// <summary>DragPanel에서 드래그 → 컨트롤러에 스크린 좌표 전달.</summary>
public class AppleGameDragPanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public AppleGameController appleGameController;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (appleGameController != null)
        {
            appleGameController.OnDragPanelBegin(eventData.position);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (appleGameController != null)
        {
            appleGameController.OnDragPanelDrag(eventData.position);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (appleGameController != null)
        {
            appleGameController.OnDragPanelEnd(eventData.position);
        }
    }
}
