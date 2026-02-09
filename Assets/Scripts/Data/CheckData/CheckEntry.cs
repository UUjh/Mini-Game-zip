using UnityEngine;

/// <summary>
/// 서류 검증 게임의 한 건의 데이터.
/// </summary>
[System.Serializable]
public class CheckEntry
{
    [Header("정답")]
    public PersonInfo monitorInfo;

    [Header("검증 대상")]
    public PersonInfo documentInfo;
}

