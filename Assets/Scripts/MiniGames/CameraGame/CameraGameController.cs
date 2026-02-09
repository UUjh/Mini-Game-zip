using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Mediapipe.Unity;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Tasks.Vision.FaceLandmarker;
using Mediapipe.Unity.Sample.HandLandmarkDetection;
using Mediapipe.Unity.Sample.FaceLandmarkDetection;

/// <summary>
/// 간단한 카메라 게임 컨트롤러.
/// Hand + Face 샘플 씬에서 바로 동작하도록 만든 버전.
/// 손/얼굴 위치는 나중에 MediaPipe 연결 후 업데이트.
/// </summary>
public class CameraGameController : MonoBehaviour
{
    [Header("게임 설정")]
    [SerializeField] private float gameTime = 60f;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float foodFallSpeed = 5f;
    [SerializeField] private Vector2 spawnXRange = new Vector2(-8f, 8f);
    [SerializeField] private float spawnY = 10f;
    [SerializeField] private float groundY = -10f;
    
    [Header("충돌 감지")]
    [SerializeField] private float handCollisionRadius = 1.0f;
    [SerializeField] private float mouthCollisionRadius = 0.8f;
    
    [Header("음식 풀")]
    [SerializeField] private FoodPool foodPool;
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI debugText;
    
    [Header("MediaPipe")]
    [SerializeField] private Camera gameCamera;
    
    // MediaPipe AnnotationController (Reflection으로 결과 읽기)
    [SerializeField] private HandLandmarkerResultAnnotationController handAnnotationController;
    [SerializeField] private FaceLandmarkerResultAnnotationController faceAnnotationController;
    private System.Reflection.FieldInfo handResultField;
    private System.Reflection.FieldInfo faceResultField;
    
    private float remainingTime;
    private int score;
    private List<GameObject> activeFoods = new List<GameObject>();
    private float nextSpawnTime;
    
    private bool gameStarted = false;
    
    // 손/입 위치
    private List<Vector3> handPositions = new List<Vector3>(); // 양손 모두 저장
    private Vector3 mouthWorldPos;
    private bool faceDetected = false;
    
    private void Start()
    {
        if (gameCamera == null)
            gameCamera = Camera.main;
        
        if (handAnnotationController != null)
        {
            handResultField = handAnnotationController.GetType().GetField("_currentTarget", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Debug.Log($"[CameraGame] ✅ Hand Annotation 찾음 ({handAnnotationController.gameObject.name})");
        }
        else
        {
            Debug.LogWarning("[CameraGame] ⚠️ HandLandmarkerResultAnnotationController가 씬에 없습니다!");
        }
        
        if (faceAnnotationController != null)
        {
            faceResultField = faceAnnotationController.GetType().GetField("_currentTarget", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Debug.Log($"[CameraGame] ✅ Face Annotation 찾음 ({faceAnnotationController.gameObject.name})");
        }
        else
        {
            Debug.LogWarning("[CameraGame] ⚠️ FaceLandmarkerResultAnnotationController가 씬에 없습니다!");
        }
            
        remainingTime = gameTime;
        score = 0;
        nextSpawnTime = spawnInterval;
        
        UpdateUI();
        
        // 3초 후 게임 시작
        Invoke(nameof(StartGame), 3f);
    }
    
    private void StartGame()
    {
        gameStarted = true;
        Debug.Log("[CameraGame] 게임 시작!");
    }
    
    private void Update()
    {
        if (!gameStarted) return;
        
        // 타이머
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            EndGame();
            return;
        }
        
        // 손/입 위치 업데이트
        UpdateHandAndMouthPosition();
        
        // 음식 스폰
        nextSpawnTime -= Time.deltaTime;
        if (nextSpawnTime <= 0f)
        {
            SpawnFood();
            nextSpawnTime = spawnInterval;
        }
        
        // 음식 이동 및 충돌 체크
        UpdateFoods();
        
        // UI 업데이트
        UpdateUI();
    }
    
    private void UpdateHandAndMouthPosition()
    {
        // 손 위치 업데이트 (양손 모두 인식)
        handPositions.Clear();
        
        if (handAnnotationController != null && handResultField != null)
        {
            try
            {
                var result = handResultField.GetValue(handAnnotationController);
                if (result is HandLandmarkerResult handResult)
                {
                    if (handResult.handLandmarks != null && handResult.handLandmarks.Count > 0)
                    {
                        // 모든 손을 순회
                        foreach (var landmarks in handResult.handLandmarks)
                        {
                            if (landmarks.landmarks.Count > 8)
                            {
                                var indexFinger = landmarks.landmarks[8];
                                Vector2 screenNorm = new Vector2(indexFinger.x, 1f - indexFinger.y);
                                Vector3 screenPos = new Vector3(
                                    screenNorm.x * UnityEngine.Screen.width,
                                    screenNorm.y * UnityEngine.Screen.height,
                                    10f
                                );
                                Vector3 worldPos = gameCamera.ScreenToWorldPoint(screenPos);
                                handPositions.Add(worldPos);
                            }
                        }
                    }
                }
            }
            catch { }
        }
        
        // 얼굴 위치 업데이트 (Reflection으로 AnnotationController에서 직접 읽기)
        if (faceAnnotationController != null && faceResultField != null)
        {
            try
            {
                var result = faceResultField.GetValue(faceAnnotationController);
                if (result is FaceLandmarkerResult faceResult)
                {
                    if (faceResult.faceLandmarks != null && faceResult.faceLandmarks.Count > 0)
                    {
                        var landmarks = faceResult.faceLandmarks[0];
                        if (landmarks.landmarks.Count > 13)
                        {
                            var mouthCenter = landmarks.landmarks[13];
                            Vector2 screenNorm = new Vector2(mouthCenter.x, 1f - mouthCenter.y);
                            Vector3 screenPos = new Vector3(
                                screenNorm.x * UnityEngine.Screen.width,
                                screenNorm.y * UnityEngine.Screen.height,
                                10f
                            );
                            mouthWorldPos = gameCamera.ScreenToWorldPoint(screenPos);
                            faceDetected = true;
                            return;
                        }
                    }
                }
            }
            catch { }
        }
        
        faceDetected = false;
    }
    
    private void SpawnFood()
    {
        if (foodPool == null)
        {
            Debug.LogWarning("[CameraGame] 음식 풀이 없습니다!");
            return;
        }
        
        GameObject food = foodPool.GetRandomFood();
        if (food == null) return;
        
        float randomX = Random.Range(spawnXRange.x, spawnXRange.y);
        Vector3 spawnPos = new Vector3(randomX, spawnY, 0f);
        
        food.transform.position = spawnPos;
        food.transform.rotation = Quaternion.identity;
        activeFoods.Add(food);
    }
    
    private void UpdateFoods()
    {
        for (int i = activeFoods.Count - 1; i >= 0; i--)
        {
            GameObject food = activeFoods[i];
            if (food == null)
            {
                activeFoods.RemoveAt(i);
                continue;
            }
            
            // 음식 아래로 이동
            food.transform.position += Vector3.down * foodFallSpeed * Time.deltaTime;
            
            // 바닥에 닿으면 풀에 반환
            if (food.transform.position.y < groundY)
            {
                activeFoods.RemoveAt(i);
                ReturnFoodToPool(food);
                continue;
            }
            
            // 손과 충돌 체크 (양손 모두)
            bool eaten = false;
            foreach (var handPos in handPositions)
            {
                float distToHand = Vector3.Distance(food.transform.position, handPos);
                if (distToHand < handCollisionRadius)
                {
                    EatFood(i);
                    eaten = true;
                    break;
                }
            }
            if (eaten) continue;
            
            // 입과 충돌 체크
            if (faceDetected)
            {
                float distToMouth = Vector3.Distance(food.transform.position, mouthWorldPos);
                if (distToMouth < mouthCollisionRadius)
                {
                    EatFood(i);
                    continue;
                }
            }
        }
    }
    
    private void EatFood(int index)
    {
        score++;
        GameObject food = activeFoods[index];
        activeFoods.RemoveAt(index);
        ReturnFoodToPool(food);
    }
    
    private void ReturnFoodToPool(GameObject food)
    {
        if (foodPool != null)
        {
            // 풀에 반환 (프리팹 정보는 풀이 알아서 처리)
            food.SetActive(false);
        }
        else
        {
            Destroy(food);
        }
    }
    
    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
            
        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(remainingTime)}s";
            
        if (debugText != null)
        {
            string debug = "";
            debug += handPositions.Count > 0 ? $"Hands: {handPositions.Count}\n" : "Hands: Not Detected\n";
            debug += faceDetected ? "Face: Detected\n" : "Face: Not Detected\n";
            debugText.text = debug;
        }
    }
    
    private void EndGame()
    {
        gameStarted = false;
        Debug.Log($"[CameraGame] 게임 종료! 최종 점수: {score}");
        
        // 남은 음식 풀에 반환
        foreach (var food in activeFoods)
        {
            if (food != null)
                ReturnFoodToPool(food);
        }
        activeFoods.Clear();
    }
}
