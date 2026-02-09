using UnityEngine;

/// <summary>
/// 미니게임 메타데이터를 담는 ScriptableObject.
/// Unity 에디터에서 [Create] → [MiniGame] → [MiniGameData]로 생성할 수 있다.
///
/// 각 미니게임마다 하나의 에셋을 만들어
/// Assets/ScriptableObjects/MiniGameData/ 폴더에 저장한다.
/// </summary>
[CreateAssetMenu(fileName = "NewMiniGameData", menuName = "MiniGame/MiniGameData")]
public class MiniGameData : ScriptableObject
{
    [Header("기본 정보")]
    public string gameName;                  // 게임 이름 (UI 표시용)
    [TextArea(2, 4)]
    public string description;               // 게임 설명

    [Header("시각 정보")]
    public Sprite icon;                      // 아이콘 (타이틀 버튼에 표시)

    [Header("난이도")]
    [Range(1, 5)]
    public int difficulty = 1;               // 난이도 (1~5)

    [Header("씬 참조")]
    public string sceneName;                 // 미니게임 씬 이름 (Build Settings에 등록 필요)
}
