using UnityEngine;
using System.Collections;

/// <summary>
/// 모든 미니게임의 공통 베이스 클래스.
/// IMiniGame 인터페이스를 구현하며, 상태머신을 통해
/// Init → Countdown → Play → Result 흐름을 자동으로 실행한다.
///
/// 새 미니게임을 만들 때 이 클래스를 상속하고
/// 각 On~ 메서드를 override하여 게임 로직을 구현한다.
/// </summary>
public abstract class MiniGameBase : MonoBehaviour, IMiniGame
{
    [Header("게임 설정")]
    [SerializeField] protected float countdownDuration = 3f;
    [SerializeField] protected int maxScore = 100;

    /// <summary>현재 게임 결과 데이터</summary>
    public GameResult Result { get; protected set; }

    /// <summary>상태머신 인스턴스</summary>
    protected MiniGameStateMachine StateMachine { get; private set; }

    /// <summary>플레이 경과 시간</summary>
    protected float ElapsedTime { get; private set; }

    /// <summary>게임이 플레이 중인지 여부</summary>
    protected bool IsPlaying { get; private set; }

    // ──────────────────────────────────────
    // Unity 생명주기
    // ──────────────────────────────────────

    protected virtual void Awake()
    {
        StateMachine = new MiniGameStateMachine(this);
    }

    /// <summary>
    /// 씬 로드 직후 자동으로 게임 흐름을 시작한다.
    /// </summary>
    protected virtual void Start()
    {
        StartGame();
    }

    protected virtual void Update()
    {
        if (StateMachine.CurrentState == GameState.Play)
        {
            ElapsedTime += Time.deltaTime;
            OnPlayUpdate();
        }
    }

    // ──────────────────────────────────────
    // 공통 흐름 제어
    // ──────────────────────────────────────

    /// <summary>
    /// 전체 게임 흐름을 시작한다. (Init → Countdown → Play)
    /// Init 완료 후 Countdown으로 명시적 전환한다.
    /// </summary>
    public void StartGame()
    {
        StateMachine.ChangeState(GameState.Init);
        StateMachine.ChangeState(GameState.Countdown);
    }

    /// <summary>
    /// 게임을 종료하고 결과 화면으로 전환한다.
    /// 플레이 중에 호출해야 한다.
    /// </summary>
    public void EndGame()
    {
        IsPlaying = false;
        Result.playTime = ElapsedTime;
        Result.CalcRank(maxScore);

        // 최고기록 갱신 시 PlayerPrefs에 저장
        if (Result.TryUpdateHighScore())
        {
            PlayerPrefs.SetInt("HighScore_" + Result.gameName, Result.highScore);
            PlayerPrefs.Save();
        }

        StateMachine.ChangeState(GameState.Result);
    }

    /// <summary>
    /// 게임을 재시작한다.
    /// </summary>
    public void Retry()
    {
        OnGameCleanUp();
        ElapsedTime = 0f;
        StartGame();
    }

    /// <summary>
    /// 타이틀 씬으로 돌아간다.
    /// </summary>
    public void BackToTitle()
    {
        OnGameCleanUp();
        GameManager.Instance.LoadTitle();
    }

    // ──────────────────────────────────────
    // IMiniGame 기본 구현 (override 가능)
    // ──────────────────────────────────────

    /// <summary>
    /// 게임 초기화. 기본 구현: Result 생성 및 최고기록 로드.
    /// 상태 전환은 StartGame()에서 관리하므로 여기서는 하지 않는다.
    /// </summary>
    public virtual void OnGameInit()
    {
        Result = new GameResult(gameObject.name);
        ElapsedTime = 0f;
        IsPlaying = false;

        // 최고기록 불러오기
        Result.highScore = PlayerPrefs.GetInt("HighScore_" + Result.gameName, 0);
    }

    /// <summary>
    /// 카운트다운 시작. 기본 구현: countdownDuration초 후 Play로 전환.
    /// </summary>
    public virtual void OnCountdownStart()
    {
        StartCoroutine(CountdownRoutine());
    }

    /// <summary>
    /// 플레이 시작. 기본 구현: IsPlaying을 true로 설정.
    /// </summary>
    public virtual void OnPlayStart()
    {
        IsPlaying = true;
    }

    /// <summary>
    /// 매 프레임 플레이 업데이트. 하위 클래스에서 override하여 게임 로직을 구현한다.
    /// </summary>
    public virtual void OnPlayUpdate()
    {
        // 하위 클래스에서 구현
    }

    /// <summary>
    /// 결과 표시. 하위 클래스에서 override하여 결과 UI를 갱신한다.
    /// </summary>
    public virtual void OnResult(GameResult result)
    {
        // 하위 클래스에서 결과 UI 표시 구현
    }

    /// <summary>
    /// 게임 정리. 하위 클래스에서 override하여 리소스를 해제한다.
    /// </summary>
    public virtual void OnGameCleanUp()
    {
        IsPlaying = false;
        StopAllCoroutines();
    }

    // ──────────────────────────────────────
    // 카운트다운 코루틴
    // ──────────────────────────────────────

    /// <summary>
    /// 카운트다운을 실행하고, 완료 시 Play 상태로 전환한다.
    /// </summary>
    private IEnumerator CountdownRoutine()
    {
        float remaining = countdownDuration;

        while (remaining > 0f)
        {
            OnCountdownTick(Mathf.CeilToInt(remaining));
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        OnCountdownFinished();
        StateMachine.ChangeState(GameState.Play);
    }

    /// <summary>
    /// 카운트다운 매 초마다 호출된다. (3, 2, 1 ...)
    /// 하위 클래스에서 override하여 UI 텍스트를 갱신한다.
    /// </summary>
    protected virtual void OnCountdownTick(int secondsLeft)
    {
        // 하위 클래스에서 UI 갱신 구현 (예: 텍스트 "3", "2", "1")
    }

    /// <summary>
    /// 카운트다운 종료 시 호출된다. (GO!)
    /// 하위 클래스에서 override하여 "GO!" 표시 등을 구현한다.
    /// </summary>
    protected virtual void OnCountdownFinished()
    {
        // 하위 클래스에서 "GO!" 표시 등 구현
    }
}
