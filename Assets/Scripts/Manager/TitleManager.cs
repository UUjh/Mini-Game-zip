using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// TitleScene에서 미니게임 목록 UI를 관리한다.
/// MiniGameData 배열을 기반으로 버튼을 동적으로 생성하고,
/// 버튼 클릭 시 GameManager를 통해 해당 미니게임 씬으로 전환한다.
/// </summary>
public class TitleManager : MonoBehaviour
{
    [Header("미니게임 목록")]
    [SerializeField] private MiniGameData[] gameDataList;

    [Header("UI 참조")]
    [SerializeField] private Transform buttonContainer;
    [SerializeField] private GameObject buttonPrefab;

    private void Start()
    {
        CreateGameButtons();
    }

    /// <summary>
    /// gameDataList를 순회하며 버튼을 동적으로 생성한다.
    /// 각 버튼에 미니게임 이름을 표시하고, 클릭 시 해당 게임을 로드한다.
    /// </summary>
    private void CreateGameButtons()
    {
        if (buttonPrefab == null || buttonContainer == null)
        {
            Debug.LogWarning("[TitleManager] buttonPrefab 또는 buttonContainer가 할당되지 않았습니다.");
            return;
        }

        foreach (MiniGameData gameData in gameDataList)
        {
            if (gameData == null) continue;

            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
            SetupButton(buttonObj, gameData);
        }
    }

    /// <summary>
    /// 생성된 버튼에 미니게임 정보를 세팅한다.
    /// </summary>
    private void SetupButton(GameObject buttonObj, MiniGameData gameData)
    {
        // 버튼 텍스트 설정
        Text buttonText = buttonObj.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = gameData.gameName;
        }

        // 버튼 클릭 이벤트 등록
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            MiniGameData capturedData = gameData;
            button.onClick.AddListener(() => OnGameButtonClicked(capturedData));
        }
    }

    /// <summary>
    /// 미니게임 버튼 클릭 시 호출된다.
    /// GameManager를 통해 해당 미니게임 씬을 로드한다.
    /// </summary>
    private void OnGameButtonClicked(MiniGameData gameData)
    {
        GameManager.Instance.LoadMiniGame(gameData);
    }
}
