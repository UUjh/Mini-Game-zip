using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MiniGame.CameraGame;

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
    
    [Header("MediaPipe (나중에 연결)")]
    [SerializeField] private HandResultHolder handResultHolder;
    [SerializeField] private FaceResultHolder faceResultHolder;
    [SerializeField] private Camera gameCamera;
    
    private float remainingTime;
    private int score;
    private List<GameObject> activeFoods = new List<GameObject>();
    private float nextSpawnTime;
    
    private bool gameStarted = false;
    
    // 임시: 손/입 위치 (MediaPipe 연결 전 테스트용)
    private Vector3 handWorldPos;
    private Vector3 mouthWorldPos;
    private bool handDetected = false;
    private bool faceDetected = false;
    
    private void Start()
    {
        if (gameCamera == null)
            gameCamera = Camera.main;
            
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
        // 60프레임마다 한 번씩 로그
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[CameraGameController] handResultHolder: {(handResultHolder != null ? "연결됨" : "NULL")}, faceResultHolder: {(faceResultHolder != null ? "연결됨" : "NULL")}");
        }
        
        // MediaPipe ResultHolder에서 손/얼굴 위치 가져오기
        if (handResultHolder != null && handResultHolder.IsDetected)
        {
            var result = handResultHolder.LatestResult.Value;
            if (result.handLandmarks != null && result.handLandmarks.Count > 0)
            {
                var landmarks = result.handLandmarks[0];
                if (landmarks.landmarks.Count > 8)
                {
                    var indexFinger = landmarks.landmarks[8];
                    Vector2 screenNorm = new Vector2(indexFinger.x, 1f - indexFinger.y);
                    Vector3 screenPos = new Vector3(
                        screenNorm.x * Screen.width,
                        screenNorm.y * Screen.height,
                        10f
                    );
                    handWorldPos = gameCamera.ScreenToWorldPoint(screenPos);
                    handDetected = true;
                }
            }
        }
        else
        {
            handDetected = false;
        }
        
        if (faceResultHolder != null && faceResultHolder.IsDetected)
        {
            var result = faceResultHolder.LatestResult.Value;
            if (result.faceLandmarks != null && result.faceLandmarks.Count > 0)
            {
                var landmarks = result.faceLandmarks[0];
                if (landmarks.landmarks.Count > 13)
                {
                    var mouthCenter = landmarks.landmarks[13];
                    Vector2 screenNorm = new Vector2(mouthCenter.x, 1f - mouthCenter.y);
                    Vector3 screenPos = new Vector3(
                        screenNorm.x * Screen.width,
                        screenNorm.y * Screen.height,
                        10f
                    );
                    mouthWorldPos = gameCamera.ScreenToWorldPoint(screenPos);
                    faceDetected = true;
                }
            }
        }
        else
        {
            faceDetected = false;
        }
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
            
            // 손과 충돌 체크
            if (handDetected)
            {
                float distToHand = Vector3.Distance(food.transform.position, handWorldPos);
                if (distToHand < handCollisionRadius)
                {
                    EatFood(i);
                    continue;
                }
            }
            
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
        Debug.Log($"[CameraGame] 음식 먹음! 점수: {score}");
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
            scoreText.text = $"점수: {score}";
            
        if (timerText != null)
            timerText.text = $"시간: {Mathf.CeilToInt(remainingTime)}초";
            
        if (debugText != null)
        {
            string debug = "";
            debug += handDetected ? "✅ 손 인식됨\n" : "❌ 손 인식 안됨\n";
            debug += faceDetected ? "✅ 얼굴 인식됨\n" : "❌ 얼굴 인식 안됨\n";
            debug += $"활성 음식: {activeFoods.Count}개\n";
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
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // 손 위치 표시 (초록)
        if (handDetected)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(handWorldPos, handCollisionRadius);
        }
        
        // 입 위치 표시 (빨강)
        if (faceDetected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(mouthWorldPos, mouthCollisionRadius);
        }
    }
}
