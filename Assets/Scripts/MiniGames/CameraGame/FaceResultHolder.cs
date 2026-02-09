using UnityEngine;
using Mediapipe.Tasks.Vision.FaceLandmarker;

namespace MiniGame.CameraGame
{
    /// <summary>
    /// 얼굴 인식 결과만 저장하는 간단한 홀더.
    /// </summary>
    public class FaceResultHolder : MonoBehaviour
    {
        public FaceLandmarkerResult? LatestResult { get; private set; }
        
        public bool IsDetected => LatestResult.HasValue && 
                                   LatestResult.Value.faceLandmarks != null && 
                                   LatestResult.Value.faceLandmarks.Count > 0;

        /// <summary>
        /// 외부에서 결과를 업데이트할 때 호출.
        /// </summary>
        public void UpdateResult(FaceLandmarkerResult result)
        {
            LatestResult = result;
        }
    }
}
