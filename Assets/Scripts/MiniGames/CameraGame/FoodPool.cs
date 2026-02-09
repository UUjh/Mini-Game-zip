using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 간단한 음식 오브젝트 풀.
/// </summary>
public class FoodPool : MonoBehaviour
{
    [SerializeField] private GameObject[] foodPrefabs;
    [SerializeField] private int poolSizePerPrefab = 10;

    private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

    private void Start()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (var prefab in foodPrefabs)
        {
            if (prefab == null) continue;

            pools[prefab] = new Queue<GameObject>();

            for (int i = 0; i < poolSizePerPrefab; i++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                pools[prefab].Enqueue(obj);
            }
        }

        Debug.Log($"[FoodPool] {foodPrefabs.Length}종류 음식, 각 {poolSizePerPrefab}개씩 풀 생성 완료");
    }

    public GameObject GetRandomFood()
    {
        if (foodPrefabs == null || foodPrefabs.Length == 0)
            return null;

        GameObject prefab = foodPrefabs[Random.Range(0, foodPrefabs.Length)];
        return Get(prefab);
    }

    public GameObject Get(GameObject prefab)
    {
        if (!pools.ContainsKey(prefab))
            return null;

        GameObject obj;

        if (pools[prefab].Count > 0)
        {
            obj = pools[prefab].Dequeue();
        }
        else
        {
            // 풀이 비었으면 새로 생성
            obj = Instantiate(prefab, transform);
        }

        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj, GameObject prefab)
    {
        if (obj == null) return;

        obj.SetActive(false);

        if (pools.ContainsKey(prefab))
        {
            pools[prefab].Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }

    public void ReturnAll()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                
                // 어느 풀에 속하는지 찾아서 반환
                foreach (var pair in pools)
                {
                    pair.Value.Enqueue(child.gameObject);
                    break;
                }
            }
        }
    }
}
