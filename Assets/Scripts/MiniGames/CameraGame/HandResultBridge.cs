using UnityEngine;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample.HandLandmarkDetection;

namespace MiniGame.CameraGame
{
    /// <summary>
    /// HandLandmarkerResultAnnotationController로부터 결과를 받아
    /// HandResultHolder에 전달하는 브릿지 스크립트.
    /// HandLandmarkerResultAnnotationController와 같은 GameObject에 추가.
    /// </summary>
    [RequireComponent(typeof(HandLandmarkerResultAnnotationController))]
    public class HandResultBridge : MonoBehaviour
    {
        [SerializeField] private HandResultHolder resultHolder;
        
        private HandLandmarkerResultAnnotationController annotationController;
        
        // Reflection으로 private 필드 접근용
        private System.Reflection.FieldInfo latestResultField;
        
        private void Start()
        {
            annotationController = GetComponent<HandLandmarkerResultAnnotationController>();
            
            // Reflection으로 AnnotationController의 private 결과 필드 찾기
            var type = annotationController.GetType().BaseType; // AnnotationController<HandLandmarkerResult>
            if (type != null)
            {
                latestResultField = type.GetField("_currentTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            }
        }
        
        private void Update()
        {
            if (resultHolder == null)
            {
                Debug.LogWarning("[HandResultBridge] resultHolder가 null입니다!");
                return;
            }
            
            if (annotationController == null)
            {
                Debug.LogWarning("[HandResultBridge] annotationController가 null입니다!");
                return;
            }
            
            if (latestResultField == null)
            {
                Debug.LogWarning("[HandResultBridge] latestResultField가 null입니다! Reflection 실패.");
                return;
            }
            
            try
            {
                // Reflection으로 최신 결과 가져오기
                var result = latestResultField.GetValue(annotationController);
                if (result is HandLandmarkerResult handResult)
                {
                    resultHolder.UpdateResult(handResult);
                    
                    // 첫 프레임에만 로그
                    if (Time.frameCount % 60 == 0)
                    {
                        Debug.Log($"[HandResultBridge] 손 인식 데이터 전달 성공! 손 개수: {handResult.handLandmarks?.Count ?? 0}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[HandResultBridge] Reflection 오류: {e.Message}");
            }
        }
    }
}
