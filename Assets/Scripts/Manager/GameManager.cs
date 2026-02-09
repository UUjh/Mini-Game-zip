using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체를 관리하는 싱글턴 매니저.
/// 씬 전환, 현재 선택된 미니게임 정보 등을 관리한다.
/// TitleScene에 배치하며, DontDestroyOnLoad로 씬 전환 시에도 유지된다.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("설정")]
    [SerializeField] private string titleSceneName = "TitleScene";

    /// <summary>현재 선택된 미니게임 데이터</summary>
    public MiniGameData CurrentGameData { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ──────────────────────────────────────
    // 씬 전환
    // ──────────────────────────────────────

    /// <summary>
    /// 미니게임 데이터를 받아 해당 씬으로 전환한다.
    /// TitleManager의 버튼 클릭 시 호출된다.
    /// </summary>
    public void LoadMiniGame(MiniGameData gameData)
    {
        if (gameData == null)
        {
            Debug.LogWarning("[GameManager] MiniGameData가 null입니다.");
            return;
        }

        if (string.IsNullOrEmpty(gameData.sceneName))
        {
            Debug.LogWarning("[GameManager] sceneName이 비어 있습니다. MiniGameData SO의 Scene Name 필드를 확인하세요. (게임: " + gameData.gameName + ")");
            return;
        }

        CurrentGameData = gameData;
        SceneManager.LoadScene(gameData.sceneName);
    }

    /// <summary>
    /// 타이틀 씬으로 돌아간다.
    /// </summary>
    public void LoadTitle()
    {
        CurrentGameData = null;
        SceneManager.LoadScene(titleSceneName);
    }

    /// <summary>
    /// 현재 미니게임 씬을 다시 로드한다. (재시작)
    /// </summary>
    public void ReloadCurrentGame()
    {
        if (CurrentGameData != null)
        {
            SceneManager.LoadScene(CurrentGameData.sceneName);
        }
    }
}
