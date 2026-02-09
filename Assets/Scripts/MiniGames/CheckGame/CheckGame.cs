using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using DG.Tweening;

/// <summary>
/// 서류 검증 미니게임 (틀린 정보 찾기).
///
/// [게임 규칙]
/// - 위쪽 모니터에 정답 정보, 아래쪽 서류에 검증 대상 정보가 표시된다.
/// - 서류가 먼저 표시되고, 모니터는 로딩바(1초) 후 표시된다.
/// - 플레이어는 서류와 모니터를 비교하여 O(일치) 또는 X(불일치)를 판단한다.
/// - 60초 제한시간 내에 최대한 많이 맞추면 된다.
/// - 매 판마다 랜덤 조합으로 문제가 생성되어 다양성 확보.
///
/// [흐름]
/// Init → Countdown → Play → (O/X 판정 반복) → Result
/// </summary>
public class CheckGame : MiniGameBase
{
    [Header("스테이지 데이터")]
    [SerializeField] private CheckStageData stageData;

    [Header("모니터 UI (위쪽)")]
    [SerializeField] private GameObject monitorInfoGroup;
    [SerializeField] private GameObject loadingObj;
    [SerializeField] private Slider loadingBar;
    [SerializeField] private TextMeshProUGUI monitorNameText;
    [SerializeField] private TextMeshProUGUI monitorGenderText;
    [SerializeField] private TextMeshProUGUI monitorAgeText;
    [SerializeField] private TextMeshProUGUI monitorDiseaseText;
    [SerializeField] private TextMeshProUGUI monitorSpecialText;

    [Header("서류 UI (아래쪽)")]
    [SerializeField] private TextMeshProUGUI docNameText;
    [SerializeField] private TextMeshProUGUI docGenderText;
    [SerializeField] private TextMeshProUGUI docAgeText;
    [SerializeField] private TextMeshProUGUI docDiseaseText;
    [SerializeField] private TextMeshProUGUI docSpecialText;

    [Header("판정 UI")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("피드백 이미지")]
    [SerializeField] private Image feedbackImage;
    [SerializeField] private Color correctFeedbackColor = new Color(0.6f, 1f, 0.6f, 0.4f);
    [SerializeField] private Color wrongFeedbackColor = new Color(1f, 0.4f, 0.4f, 0.4f);
    [SerializeField] private float feedbackDuration = 0.15f;

    [Header("카운트다운 UI")]
    [SerializeField] private GameObject countdownObj;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("결과 UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultCorrectText;
    [SerializeField] private TextMeshProUGUI resultWrongText;
    [SerializeField] private TextMeshProUGUI resultRankText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;

    [Header("설정")]
    [SerializeField] private float monitorLoadTime = 1f;

    private float remainingTime;
    private bool isMonitorLoaded;
    private bool isJudging;
    private int wrongCount;
    private Tween feedbackTween;

    // 현재 판의 정답(모니터)과 서류 정보
    private PersonInfo curMonitorInfo;
    private PersonInfo curDocumentInfo;

    // ──────────────────────────────────────
    // MiniGameBase override
    // ──────────────────────────────────────

    public override void OnGameInit()
    {
        base.OnGameInit();

        // 스테이지 데이터 기반 설정
        maxScore = 100; // 시간제한 내 최대한 많이 처리 (고정 기준점)
        remainingTime = stageData.timeLimit;
        isMonitorLoaded = false;
        isJudging = false;
        wrongCount = 0;

        // UI 초기화
        if (resultPanel != null) resultPanel.SetActive(false);
        if (feedbackImage != null)
        {
            feedbackImage.gameObject.SetActive(false);
            var imgColor = feedbackImage.color;
            imgColor.a = 0f;
            feedbackImage.color = imgColor;
        }
        if (monitorInfoGroup != null) monitorInfoGroup.SetActive(false);
        if (loadingObj != null) loadingObj.SetActive(false);
        if (scoreText != null) scoreText.text = "0";
        if (timerText != null) timerText.text = remainingTime.ToString("F0");

        SetButtonsInteractable(false);

        // 버튼 이벤트 등록
        if (acceptButton != null) acceptButton.onClick.AddListener(OnAcceptClicked);
        if (rejectButton != null) rejectButton.onClick.AddListener(OnRejectClicked);
        if (retryButton != null) retryButton.onClick.AddListener(Retry);
        if (titleButton != null) titleButton.onClick.AddListener(BackToTitle);
    }

    protected override void OnCountdownTick(int secondsLeft)
    {
        if (countdownObj != null) countdownObj.SetActive(true);
        if (countdownText != null) countdownText.text = secondsLeft.ToString();
    }

    protected override void OnCountdownFinished()
    {
        if (countdownText != null) countdownText.text = "START!";
    }

    public override void OnPlayStart()
    {
        base.OnPlayStart();

        if (countdownObj != null) countdownObj.SetActive(false);

        // 첫 번째 랜덤 문제 생성
        GenerateRandomEntry();
    }

    public override void OnPlayUpdate()
    {
        // 남은 시간 감소
        remainingTime -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = Mathf.Max(0f, remainingTime).ToString("F0");
        }

        // 시간 종료 → 게임 종료
        if (remainingTime <= 0f)
        {
            Result.isCleared = false;
            EndGame();
        }
    }

    public override void OnResult(GameResult result)
    {
        SetButtonsInteractable(false);

        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultCorrectText != null) resultCorrectText.text = "맞은 개수: " + result.score;
        if (resultWrongText != null) resultWrongText.text = "틀린 개수: " + wrongCount;
        if (resultRankText != null) resultRankText.text = "등급: " + result.rank;
    }

    public override void OnGameCleanUp()
    {
        base.OnGameCleanUp();

        if (acceptButton != null) acceptButton.onClick.RemoveAllListeners();
        if (rejectButton != null) rejectButton.onClick.RemoveAllListeners();
        if (retryButton != null) retryButton.onClick.RemoveAllListeners();
        if (titleButton != null) titleButton.onClick.RemoveAllListeners();
    }

    // ──────────────────────────────────────
    // 랜덤 문제 생성
    // ──────────────────────────────────────

    /// <summary>
    /// 랜덤 조합으로 모니터 정보와 서류 정보를 생성한다.
    /// 서류는 errorProbability 확률로 일부 필드가 틀리게 생성된다.
    /// </summary>
    private void GenerateRandomEntry()
    {
        // 풀에서 랜덤 인덱스 선택
        string randomName = stageData.namePool[Random.Range(0, stageData.namePool.Length)];
        int randomAge = stageData.agePool[Random.Range(0, stageData.agePool.Length)];
        string randomGender = stageData.genderPool[Random.Range(0, stageData.genderPool.Length)];
        string randomDisease = stageData.diseasePool[Random.Range(0, stageData.diseasePool.Length)];
        string randomSpecial = stageData.specialPool[Random.Range(0, stageData.specialPool.Length)];

        // 모니터 정보 (정답)
        curMonitorInfo = new PersonInfo
        {
            personName = randomName,
            age = randomAge,
            gender = randomGender,
            disease = randomDisease,
            special = randomSpecial
        };

        // 서류 정보 (모니터를 복사 후, 일부를 틀리게 만들 수도 있음)
        curDocumentInfo = new PersonInfo
        {
            personName = curMonitorInfo.personName,
            age = curMonitorInfo.age,
            gender = curMonitorInfo.gender,
            disease = curMonitorInfo.disease,
            special = curMonitorInfo.special
        };

        // errorProbability 확률로 서류를 틀리게 만듦
        if (Random.value < stageData.errorProbability)
        {
            MakeDocumentWrong();
        }

        // 서류 즉시 표시
        DisplayDocumentInfo(curDocumentInfo);

        // 모니터 로딩 시작 (로딩바 → 정보 표시)
        StartCoroutine(LoadMonitorRoutine(curMonitorInfo));
    }

    /// <summary>
    /// 서류의 일부 필드를 틀리게 만든다.
    /// 랜덤으로 1~2개 필드를 다른 값으로 교체한다.
    /// </summary>
    private void MakeDocumentWrong()
    {
        int errorCount = Random.Range(1, 3); // 1~2개 필드를 틀리게

        for (int i = 0; i < errorCount; i++)
        {
            int fieldIndex = Random.Range(0, 5); // 0~4: 이름, 나이, 성별, 병명, 특이사항

            switch (fieldIndex)
            {
                case 0: // 이름
                    string wrongName = stageData.namePool[Random.Range(0, stageData.namePool.Length)];
                    if (wrongName != curDocumentInfo.personName)
                        curDocumentInfo.personName = wrongName;
                    break;
                case 1: // 나이
                    int wrongAge = stageData.agePool[Random.Range(0, stageData.agePool.Length)];
                    if (wrongAge != curDocumentInfo.age)
                        curDocumentInfo.age = wrongAge;
                    break;
                case 2: // 성별
                    string wrongGender = stageData.genderPool[Random.Range(0, stageData.genderPool.Length)];
                    if (wrongGender != curDocumentInfo.gender)
                        curDocumentInfo.gender = wrongGender;
                    break;
                case 3: // 병명
                    string wrongDisease = stageData.diseasePool[Random.Range(0, stageData.diseasePool.Length)];
                    if (wrongDisease != curDocumentInfo.disease)
                        curDocumentInfo.disease = wrongDisease;
                    break;
                case 4: // 특이사항
                    string wrongSpecial = stageData.specialPool[Random.Range(0, stageData.specialPool.Length)];
                    if (wrongSpecial != curDocumentInfo.special)
                        curDocumentInfo.special = wrongSpecial;
                    break;
            }
        }
    }

    /// <summary>
    /// 모니터 로딩바를 채운 뒤 정보를 표시한다.
    /// 로딩 중에는 O/X 버튼이 비활성화된다.
    /// </summary>
    private IEnumerator LoadMonitorRoutine(PersonInfo info)
    {
        isMonitorLoaded = false;
        SetButtonsInteractable(false);

        // 정보 숨기고 로딩바 표시
        if (monitorInfoGroup != null) monitorInfoGroup.SetActive(false);
        if (loadingObj != null) loadingObj.SetActive(true);
        if (loadingBar != null)
        {
            loadingBar.value = 0f;
        }

        // 로딩 진행
        float elapsed = 0f;
        while (elapsed < monitorLoadTime)
        {
            elapsed += Time.deltaTime;
            if (loadingBar != null) loadingBar.value = elapsed / monitorLoadTime;
            yield return null;
        }

        // 로딩 완료 → 정보 표시
        if (loadingObj != null) loadingObj.SetActive(false);
        if (monitorInfoGroup != null) monitorInfoGroup.SetActive(true);
        DisplayMonitorInfo(info);

        isMonitorLoaded = true;
        SetButtonsInteractable(true);
    }

    /// <summary>
    /// 서류 UI에 인물 정보를 표시한다.
    /// </summary>
    private void DisplayDocumentInfo(PersonInfo info)
    {
        if (docNameText != null) docNameText.text = info.personName;
        if (docAgeText != null) docAgeText.text = info.age.ToString();
        if (docGenderText != null) docGenderText.text = info.gender;
        if (docDiseaseText != null) docDiseaseText.text = info.disease;
        if (docSpecialText != null) docSpecialText.text = info.special;
    }

    /// <summary>
    /// 모니터 UI에 인물 정보를 표시한다.
    /// </summary>
    private void DisplayMonitorInfo(PersonInfo info)
    {
        if (monitorNameText != null) monitorNameText.text = info.personName;
        if (monitorAgeText != null) monitorAgeText.text = info.age.ToString();
        if (monitorGenderText != null) monitorGenderText.text = info.gender;
        if (monitorDiseaseText != null) monitorDiseaseText.text = info.disease;
        if (monitorSpecialText != null) monitorSpecialText.text = info.special;
    }

    // ──────────────────────────────────────
    // 판정 처리
    // ──────────────────────────────────────

    /// <summary>
    /// O(일치) 버튼 클릭 시 호출.
    /// </summary>
    private void OnAcceptClicked()
    {
        if (!isMonitorLoaded || isJudging) return;
        JudgeAnswer(true);
    }

    /// <summary>
    /// X(불일치) 버튼 클릭 시 호출.
    /// </summary>
    private void OnRejectClicked()
    {
        if (!isMonitorLoaded || isJudging) return;
        JudgeAnswer(false);
    }

    /// <summary>
    /// 플레이어의 판단(O/X)이 맞는지 확인하고 피드백을 표시한다.
    /// playerSaidCorrect: true면 "accept" 선택, false면 "reject" 선택.
    /// </summary>
    private void JudgeAnswer(bool playerSaidCorrect)
    {
        isJudging = true;

        bool isActuallyCorrect = IsDocumentCorrect();
        bool isPlayerRight = (playerSaidCorrect == isActuallyCorrect);

        if (isPlayerRight)
        {
            Result.score++;
            if (scoreText != null) scoreText.text = Result.score.ToString();
        }
        else
        {
            wrongCount++;
        }

        StartCoroutine(ShowFeedbackAndNext(isPlayerRight));
    }

    /// <summary>
    /// 서류의 모든 필드가 모니터와 일치하는지 확인한다.
    /// 하나라도 다르면 false를 반환한다.
    /// </summary>
    private bool IsDocumentCorrect()
    {
        return curMonitorInfo.personName == curDocumentInfo.personName
            && curMonitorInfo.age == curDocumentInfo.age
            && curMonitorInfo.gender == curDocumentInfo.gender
            && curMonitorInfo.disease == curDocumentInfo.disease
            && curMonitorInfo.special == curDocumentInfo.special;
    }

    /// <summary>
    /// 판정 결과(정답/오답)를 표시하고, 잠시 후 다음 문제를 생성한다.
    /// </summary>
    private IEnumerator ShowFeedbackAndNext(bool isPlayerRight)
    {
        SetButtonsInteractable(false);

        PlayImageFeedback(isPlayerRight);

        // 이미지 깜빡임 연출 시간만큼 대기
        yield return new WaitForSeconds(feedbackDuration * 2f);

        isJudging = false;

        // 시간이 남아 있으면 다음 문제 생성
        if (remainingTime > 0f)
        {
            GenerateRandomEntry();
        }
    }

    /// <summary>
    /// DOTween을 사용해 피드백 이미지를 깜빡이게 한다.
    /// </summary>
    private void PlayImageFeedback(bool isPlayerRight)
    {
        if (feedbackImage == null) return;

        if (feedbackTween != null && feedbackTween.IsActive())
        {
            feedbackTween.Kill();
        }

        feedbackImage.gameObject.SetActive(true);

        Color baseColor = isPlayerRight ? correctFeedbackColor : wrongFeedbackColor;
        baseColor.a = 0f;
        feedbackImage.color = baseColor;

        float targetAlpha = isPlayerRight ? correctFeedbackColor.a : wrongFeedbackColor.a;

        feedbackTween = feedbackImage.DOFade(targetAlpha, feedbackDuration)
            .SetLoops(2, LoopType.Yoyo)
            .OnComplete(() =>
            {
                if (feedbackImage != null)
                {
                    var c = feedbackImage.color;
                    c.a = 0f;
                    feedbackImage.color = c;
                    feedbackImage.gameObject.SetActive(false);
                }
            });
    }

    // ──────────────────────────────────────
    // 유틸리티
    // ──────────────────────────────────────

    /// <summary>
    /// O/X 버튼의 상호작용 가능 여부를 설정한다.
    /// </summary>
    private void SetButtonsInteractable(bool interactable)
    {
        if (acceptButton != null) acceptButton.interactable = interactable;
        if (rejectButton != null) rejectButton.interactable = interactable;
    }
}
