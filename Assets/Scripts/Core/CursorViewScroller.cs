using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 커서가 화면 가장자리에 있을 때 카메라를 상하로 이동시키는 컴포넌트.
/// 커서가 상단 가장자리 → 카메라 위로 이동 (모니터 영역).
/// 커서가 하단 가장자리 → 카메라 아래로 이동 (서류 영역).
/// 커서가 중앙 → 카메라 정지.
/// 최대/최소 높이에 도달하면 더 이상 이동하지 않는다.
///
/// [사용법]
/// Main Camera에 이 컴포넌트를 추가한다.
/// minY, maxY로 카메라 이동 한계를 설정한다.
/// </summary>
public class CursorViewScroller : MonoBehaviour
{
    [Header("이동 한계 (월드 Y 좌표)")]
    [Tooltip("카메라가 내려갈 수 있는 최소 Y 좌표")]
    [SerializeField] private float minY = -9.6f;

    [Tooltip("카메라가 올라갈 수 있는 최대 Y 좌표")]
    [SerializeField] private float maxY = 9.6f;

    [Header("스크롤 설정")]
    [Tooltip("카메라 이동 최대 속도 (월드 단위/초)")]
    [SerializeField] private float scrollSpeed = 10f;

    [Tooltip("화면 가장자리에서 얼마나 가까울 때 스크롤 시작 (0~0.5). 0.2 = 상하 20% 영역")]
    [SerializeField] private float edgeThreshold = 0.2f;

    private Camera cam;
    private bool isScrollEnabled = true;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (!isScrollEnabled || cam == null) return;

        // 마우스 위치 취득 (New Input System)
        if (Mouse.current == null) return;
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // 카메라 viewport 기준으로 마우스 Y를 0~1로 정규화
        Rect viewRect = cam.rect;
        float viewBottom = viewRect.y * Screen.height;
        float viewHeight = viewRect.height * Screen.height;
        float mouseNormY = Mathf.Clamp01((mousePos.y - viewBottom) / viewHeight);

        // 스크롤 방향과 세기 계산
        float scrollAmount = 0f;

        if (mouseNormY > 1f - edgeThreshold)
        {
            // 상단 가장자리 → 위로 스크롤
            // 가장자리에 가까울수록 빠르게 (0~1)
            float edgeRatio = (mouseNormY - (1f - edgeThreshold)) / edgeThreshold;
            scrollAmount = edgeRatio;
        }
        else if (mouseNormY < edgeThreshold)
        {
            // 하단 가장자리 → 아래로 스크롤
            float edgeRatio = (edgeThreshold - mouseNormY) / edgeThreshold;
            scrollAmount = -edgeRatio;
        }

        // 카메라 이동 (속도 적용)
        Vector3 pos = transform.position;
        pos.y += scrollAmount * scrollSpeed * Time.deltaTime;

        // 최대/최소 높이 클램프
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }

    /// <summary>
    /// 스크롤 활성화/비활성화.
    /// </summary>
    public void SetScrollEnabled(bool enabled)
    {
        isScrollEnabled = enabled;
    }
}
