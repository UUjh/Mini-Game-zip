using UnityEngine;

/// <summary>
/// 서류 검증 게임의 스테이지 데이터 (ScriptableObject).
/// Unity 에디터에서 [Create] → [MiniGame] → [CheckStageData]로 생성할 수 있다.
///
/// 매 판마다 랜덤 조합으로 문제를 생성하기 위해,
/// 이름/나이/성별/병명/특이사항의 풀(Pool)을 가지고 있다.
/// </summary>
[CreateAssetMenu(fileName = "NewCheckStageData", menuName = "MiniGame/CheckStageData")]
public class CheckStageData : ScriptableObject
{
    [Header("데이터 풀 (각 10개 권장)")]
    public string[] namePool;
    public int[] agePool;
    public string[] genderPool;
    public string[] diseasePool;
    public string[] specialPool;

    [Header("게임 설정")]
    public float timeLimit = 60f;

    [Tooltip("서류가 틀릴 확률 (0~1). 0.5 = 50% 확률로 틀린 서류")]
    [Range(0f, 1f)]
    public float errorProbability = 0.5f;
}
