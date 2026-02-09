/// <summary>
/// 모든 미니게임이 구현해야 하는 공통 인터페이스.
/// 각 상태 진입 시 호출되는 메서드를 정의한다.
/// </summary>
public interface IMiniGame
{
    /// <summary>게임 초기화 (리소스 준비, UI 세팅 등)</summary>
    void OnGameInit();

    /// <summary>카운트다운 시작 (3, 2, 1, GO!)</summary>
    void OnCountdownStart();

    /// <summary>실제 플레이 시작</summary>
    void OnPlayStart();

    /// <summary>매 프레임 플레이 중 업데이트</summary>
    void OnPlayUpdate();

    /// <summary>게임 종료 → 결과 표시</summary>
    void OnResult(GameResult result);

    /// <summary>게임 정리 (리소스 해제 등)</summary>
    void OnGameCleanUp();
}
