using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 사과 격자 한 칸. 프리팹 루트에 Image, 자식에 TMP를 두고 인스펙터에 연결한다.
/// </summary>
public class AppleCell : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI numberText;

    public Image BackgroundImage
    {
        get { return backgroundImage; }
    }

    public TextMeshProUGUI NumberText
    {
        get { return numberText; }
    }

    private void Reset()
    {
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }

        if (numberText == null)
        {
            numberText = GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }
}
