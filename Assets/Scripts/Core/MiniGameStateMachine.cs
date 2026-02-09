using UnityEngine;

/// <summary>
/// 미니게임의 상태 전환을 관리하는 상태머신.
/// Init → Countdown → Play → Result 순서로 진행된다.
/// MiniGameBase 내부에서 사용된다.
/// </summary>
public class MiniGameStateMachine
{
    public GameState CurrentState { get; private set; } = GameState.None;

    private MiniGameBase owner;

    public MiniGameStateMachine(MiniGameBase owner)
    {
        this.owner = owner;
    }

    /// <summary>
    /// 다음 상태로 전환한다.
    /// 각 상태 진입 시 owner(MiniGameBase)의 해당 메서드를 호출한다.
    /// </summary>
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        switch (newState)
        {
            case GameState.Init:
                owner.OnGameInit();
                break;
            case GameState.Countdown:
                owner.OnCountdownStart();
                break;
            case GameState.Play:
                owner.OnPlayStart();
                break;
            case GameState.Result:
                owner.OnResult(owner.Result);
                break;
        }
    }
}
