/// <summary>
/// 미니게임 결과 데이터를 담는 클래스.
/// Result 화면에서 점수, 시간, 등급, 최고기록 등을 표시하는 데 사용된다.
/// </summary>
[System.Serializable]
public class GameResult
{
    public string gameName;     // 미니게임 이름
    public int score;           // 획득 점수
    public float playTime;      // 플레이 소요 시간 (초)
    public bool isCleared;      // 클리어 여부
    public string rank;         // 등급 (S, A, B, C, F 등)
    public int highScore;       // 최고 기록

    public GameResult(string gameName)
    {
        this.gameName = gameName;
        this.score = 0;
        this.playTime = 0f;
        this.isCleared = false;
        this.rank = "F";
        this.highScore = 0;
    }

    /// <summary>
    /// 점수를 기반으로 등급을 계산한다.
    /// 각 미니게임에서 기준을 오버라이드하여 사용할 수 있도록
    /// maxScore를 매개변수로 받는다.
    /// </summary>
    public void CalcRank(int maxScore)
    {
        if (maxScore <= 0) return;

        float ratio = (float)score / maxScore;

        if (ratio >= 0.95f) rank = "S";
        else if (ratio >= 0.80f) rank = "A";
        else if (ratio >= 0.60f) rank = "B";
        else if (ratio >= 0.40f) rank = "C";
        else rank = "F";
    }

    /// <summary>
    /// 현재 점수가 최고기록보다 높으면 갱신한다.
    /// </summary>
    public bool TryUpdateHighScore()
    {
        if (score > highScore)
        {
            highScore = score;
            return true;
        }
        return false;
    }
}
