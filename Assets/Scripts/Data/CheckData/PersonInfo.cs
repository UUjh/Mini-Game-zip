/// <summary>
/// 서류 검증 게임에서 사용하는 인물 정보.
/// 모니터(정답)와 서류(검증 대상) 양쪽에 동일한 구조로 사용된다.
/// </summary>
[System.Serializable]
public class PersonInfo
{
    public string personName;   // 이름
    public int age;             // 나이
    public string gender;       // 성별
    public string disease;      // 병명
    public string special;      // 특이사항
}
