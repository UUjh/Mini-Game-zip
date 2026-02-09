/// <summary>
/// 미니게임의 공통 상태를 정의하는 Enum.
/// 모든 미니게임은 이 상태 흐름을 따른다:
/// Init → Countdown → Play → Result → (BackToTitle 또는 Retry)
/// </summary>
public enum GameState
{
    None,       // 초기 상태 (아직 시작 전)
    Init,       // 게임 초기화 (리소스 로딩, UI 세팅 등)
    Countdown,  // 카운트다운 (3, 2, 1, GO!)
    Play,       // 실제 플레이 중
    Result      // 결과 표시 (점수, 등급, 최고기록 등)
}
