using UnityEngine;
using System.Collections.Generic;

public class FloatingObjectSpawner : MonoBehaviour
{
    [Header("浮遊オブジェクト")]
    [SerializeField] private List<GameObject> listFloatingObjects;
    
    [Header("生成設定")]
    [SerializeField] private float spawnInterval = 10f; // 生成間隔（秒）
    [SerializeField] private float spawnIntervalVariation = 3f; // 間隔のランダム幅
    [SerializeField] private int maxFloatingObjects = 5; // 最大同時存在数
    
    [Header("生成位置設定")]
    [SerializeField] private float spawnZOffset = 1f; // maxZからのオフセット

    [Header("自動削除時間")]
    [SerializeField] private float autoDestroyTime = 30f; // 自動削除時間

    private float nextSpawnTime;
    private List<GameObject> activeFloatingObjects = new List<GameObject>();
    
    void Start()
    {
        // 最初の生成時間を設定
        SetNextSpawnTime();
    }
    
    void Update()
    {
        // 非アクティブなオブジェクトをリストから削除
        CleanupInactiveObjects();
        
        // 生成時間になったかチェック
        if (Time.time >= nextSpawnTime)
        {
            // 最大数に達していなければ生成
            if (activeFloatingObjects.Count < maxFloatingObjects)
            {
                SpawnFloatingObject();
            }
            
            // 次の生成時間を設定
            SetNextSpawnTime();
        }
    }
    
    /// <summary>
    /// 浮遊オブジェクトを生成
    /// </summary>
    private void SpawnFloatingObject()
    {
        if (listFloatingObjects == null || listFloatingObjects.Count == 0)
        {
            Debug.LogWarning("listFloatingObjectsが空またはnullです");
            return;
        }
        
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instanceがnullです");
            return;
        }
        
        // ランダムにオブジェクトを選択
        int randomIndex = Random.Range(0, listFloatingObjects.Count);
        GameObject selectedObject = listFloatingObjects[randomIndex];
        
        if (selectedObject == null)
        {
            Debug.LogWarning($"listFloatingObjects[{randomIndex}]がnullです");
            return;
        }
        
        // 生成位置を計算
        float spawnZ = GameManager.Instance.maxZ + spawnZOffset;
        float spawnY = Random.Range(GameManager.Instance.minY, GameManager.Instance.maxY);
        float spawnX = 0f; // X軸は固定
        
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, spawnZ);
        
        // オブジェクトを生成
        GameObject spawnedObject = Instantiate(selectedObject, spawnPosition, Quaternion.identity);
        
        // Rigidbodyがない場合は追加
        if (spawnedObject.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = spawnedObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearDamping = 2f;
            rb.angularDamping = 5f;
        }
        
        // FloatingObjectスクリプトがない場合は追加
        if (spawnedObject.GetComponent<FloatingObject>() == null)
        {
            spawnedObject.AddComponent<FloatingObject>();
        }
        
        // 自動削除スクリプトを追加
        AutoDestroy autoDestroy = spawnedObject.AddComponent<AutoDestroy>();
        autoDestroy.SetDestroyTime(autoDestroyTime); // 自動削除時間
        
        // アクティブリストに追加
        activeFloatingObjects.Add(spawnedObject);
        
        Debug.Log($"浮遊オブジェクト生成: {selectedObject.name} at {spawnPosition}");
    }
    
    /// <summary>
    /// 次の生成時間を設定
    /// </summary>
    private void SetNextSpawnTime()
    {
        float variation = Random.Range(-spawnIntervalVariation, spawnIntervalVariation);
        nextSpawnTime = Time.time + spawnInterval + variation;
    }
    
    /// <summary>
    /// 非アクティブなオブジェクトをリストから削除
    /// </summary>
    private void CleanupInactiveObjects()
    {
        for (int i = activeFloatingObjects.Count - 1; i >= 0; i--)
        {
            if (activeFloatingObjects[i] == null)
            {
                activeFloatingObjects.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// 生成間隔を動的に変更
    /// </summary>
    public void SetSpawnInterval(float newInterval)
    {
        spawnInterval = newInterval;
    }
    
    /// <summary>
    /// 最大同時存在数を動的に変更
    /// </summary>
    public void SetMaxFloatingObjects(int newMax)
    {
        maxFloatingObjects = newMax;
    }
    
    /// <summary>
    /// すべての浮遊オブジェクトを削除
    /// </summary>
    public void ClearAllFloatingObjects()
    {
        foreach (GameObject obj in activeFloatingObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        activeFloatingObjects.Clear();
    }
}

/// <summary>
/// 自動削除用のヘルパークラス
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    private float destroyTime;
    
    public void SetDestroyTime(float time)
    {
        destroyTime = time;
        Invoke(nameof(DestroyObject), time);
    }
    
    private void DestroyObject()
    {
        Destroy(gameObject);
    }
} 