using UnityEngine;



/// <summary>

/// 사과게임 스테이지 데이터. 에셋 하나당 이름·그리드 크기·시간 등 한 세트.

/// Create → MiniGame → AppleGameStageData 로 생성한다.

/// </summary>

[CreateAssetMenu(fileName = "NewAppleGameStageData", menuName = "MiniGame/AppleGameStageData")]

public class AppleGameStageData : ScriptableObject

{
    public string stageName = "Stage";



    [Header("시간·등급 산정")]

    public float timeLimit = 60f;


    [Header("그리드")]

    public int gridColumns = 12;

    public int gridRows = 8;

    /// <summary>합 10 패턴을 자주 넣는 Easy 규칙 사용 여부. 스테이지 SO마다 설정.</summary>
    public bool useEasyFillRules = true;

    [Header("GridLayoutGroup")]

    public Vector2 gridCellSize = new Vector2(52f, 52f);

    public Vector2 gridCellSpacing = new Vector2(2f, 2f);



    [Header("셀 프리팹")]

    public AppleCell cellPrefab;

}


