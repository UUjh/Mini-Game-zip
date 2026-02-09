using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 예시 미니게임 스켈레톤.
/// MiniGameBase를 상속하여 공통 흐름(Init → Countdown → Play → Result)을 따른다.
///
/// [구현 예시]
/// - 제한 시간 내에 화면을 터치/클릭하여 점수를 올리는 간단한 탭 게임.
/// - 실제 게임 로직 구현 시 이 파일을 참고하여 새 미니게임을 만든다.
/// </summary>
public class SampleMiniGame : MiniGameBase
{
    [Header("SampleGame 설정")]
    [SerializeField] private float timeLimit = 10f;

    [Header("UI 참조 (씬에서 할당)")]
    [SerializeField] private Text countdownText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text timerText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Text resultScoreText;
    [SerializeField] private Text resultRankText;
    [SerializeField] private Text resultHighScoreText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;

    private float remainingTime;

    // ──────────────────────────────────────
    // 공통 흐름 override
    // ──────────────────────────────────────

    public override void OnGameInit()
    {
        // 기본 초기화 (Result 생성 등)
        base.OnGameInit();

        // SampleGame 고유 초기화
        remainingTime = timeLimit;

        if (resultPanel != null) resultPanel.SetActive(false);
        if (scoreText != null) scoreText.text = "0";
        if (timerText != null) timerText.text = timeLimit.ToString("F1");

        // 버튼 이벤트 등록
        if (retryButton != null) retryButton.onClick.AddListener(Retry);
        if (titleButton != null) titleButton.onClick.AddListener(BackToTitle);
    }

    protected override void OnCountdownTick(int secondsLeft)
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = secondsLeft.ToString();
        }
    }

    protected override void OnCountdownFinished()
    {
        if (countdownText != null)
        {
            countdownText.text = "GO!";
            // 잠시 후 숨기기는 코루틴으로 처리 가능
        }
    }

    public override void OnPlayStart()
    {
        base.OnPlayStart();

        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    public override void OnPlayUpdate()
    {
        // 남은 시간 감소
        remainingTime -= Time.deltaTime;

        if (timerText != null)
        {
            timerText.text = Mathf.Max(0f, remainingTime).ToString("F1");
        }

        // 클릭/터치 시 점수 증가
        if (Input.GetMouseButtonDown(0))
        {
            Result.score++;
            if (scoreText != null) scoreText.text = Result.score.ToString();
        }

        // 시간 종료 → 게임 종료
        if (remainingTime <= 0f)
        {
            Result.isCleared = true;
            EndGame();
        }
    }

    public override void OnResult(GameResult result)
    {
        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultScoreText != null) resultScoreText.text = "점수: " + result.score;
        if (resultRankText != null) resultRankText.text = "등급: " + result.rank;
        if (resultHighScoreText != null) resultHighScoreText.text = "최고기록: " + result.highScore;
    }

    public override void OnGameCleanUp()
    {
        base.OnGameCleanUp();

        // 버튼 이벤트 해제
        if (retryButton != null) retryButton.onClick.RemoveAllListeners();
        if (titleButton != null) titleButton.onClick.RemoveAllListeners();
    }
}
