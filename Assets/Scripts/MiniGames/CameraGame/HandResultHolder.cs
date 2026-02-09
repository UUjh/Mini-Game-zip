using UnityEngine;
using Mediapipe.Tasks.Vision.HandLandmarker;

namespace MiniGame.CameraGame
{
    /// <summary>
    /// 손 인식 결과만 저장하는 간단한 홀더.
    /// </summary>
    public class HandResultHolder : MonoBehaviour
    {
        public HandLandmarkerResult? LatestResult { get; private set; }
        
        public bool IsDetected => LatestResult.HasValue && 
                                   LatestResult.Value.handLandmarks != null && 
                                   LatestResult.Value.handLandmarks.Count > 0;

        /// <summary>
        /// 외부에서 결과를 업데이트할 때 호출.
        /// </summary>
        public void UpdateResult(HandLandmarkerResult result)
        {
            LatestResult = result;
        }
    }
}
