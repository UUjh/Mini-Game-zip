using UnityEngine;
using Mediapipe.Tasks.Vision.FaceLandmarker;
using Mediapipe.Unity;
using Mediapipe.Unity.Sample.FaceLandmarkDetection;

namespace MiniGame.CameraGame
{
    /// <summary>
    /// FaceLandmarkerResultAnnotationController로부터 결과를 받아
    /// FaceResultHolder에 전달하는 브릿지 스크립트.
    /// FaceLandmarkerResultAnnotationController와 같은 GameObject에 추가.
    /// </summary>
    [RequireComponent(typeof(FaceLandmarkerResultAnnotationController))]
    public class FaceResultBridge : MonoBehaviour
    {
        [SerializeField] private FaceResultHolder resultHolder;
        
        private FaceLandmarkerResultAnnotationController annotationController;
        
        // Reflection으로 private 필드 접근용
        private System.Reflection.FieldInfo latestResultField;
        
        private void Start()
        {
            annotationController = GetComponent<FaceLandmarkerResultAnnotationController>();
            
            // Reflection으로 AnnotationController의 private 결과 필드 찾기
            var type = annotationController.GetType().BaseType; // AnnotationController<FaceLandmarkerResult>
            if (type != null)
            {
                latestResultField = type.GetField("_currentTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            }
        }
        
        private void Update()
        {
            if (resultHolder == null)
            {
                Debug.LogWarning("[FaceResultBridge] resultHolder가 null입니다!");
                return;
            }
            
            if (annotationController == null)
            {
                Debug.LogWarning("[FaceResultBridge] annotationController가 null입니다!");
                return;
            }
            
            if (latestResultField == null)
            {
                Debug.LogWarning("[FaceResultBridge] latestResultField가 null입니다! Reflection 실패.");
                return;
            }
            
            try
            {
                // Reflection으로 최신 결과 가져오기
                var result = latestResultField.GetValue(annotationController);
                if (result is FaceLandmarkerResult faceResult)
                {
                    resultHolder.UpdateResult(faceResult);
                    
                    // 첫 프레임에만 로그
                    if (Time.frameCount % 60 == 0)
                    {
                        Debug.Log($"[FaceResultBridge] 얼굴 인식 데이터 전달 성공! 얼굴 개수: {faceResult.faceLandmarks?.Count ?? 0}");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FaceResultBridge] Reflection 오류: {e.Message}");
            }
        }
    }
}
