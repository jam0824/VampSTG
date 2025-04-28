using UnityEngine;
using System.Collections;

public class StageManager : MonoBehaviour
{
    [Header("Stage Settings")]
    [SerializeField] float stageAllSecond = 180f;
    [Header("Enemy Settings")]
    [SerializeField] GameObject[] enemies;      // スポーンする敵プレハブ
    [SerializeField] GameObject[] items;
    [SerializeField] int spawnCount = 1;        // 一度のタイミングでスポーンする数

    [Header("Spawn Radius")]
    [SerializeField] float minDistance = 5f;    // プレイヤーから最低この距離以上
    [SerializeField] float maxDistance = 15f;   // プレイヤーから最大この距離以内

    [Header("Spawn Timing")]
    [SerializeField] float initialInterval = 5f;    // ゲーム開始直後のスポーン間隔（秒）
    [SerializeField] float minInterval = 0.5f;      // 最短スポーン間隔（秒）
    [SerializeField] float decayRate = 0.05f;       // インターバル減少率（秒／秒）

    [Header("Item Settings")]
    [SerializeField] float batteryDropRate = 0.1f;
    
    [Header("UI Settings")]
    [SerializeField] ProgressBar progressBar;

    Transform playerTransform;
    float elapsedTime;   // ゲーム起動からの経過時間

    void Start()
    {
        var playerObj = GameObject.Find("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogError("Player オブジェクトが見つかりません");

        elapsedTime = 0f;

        StartCoroutine(SpawnRoutine());
    }

    void Update()
    {
        // 経過時間をカウント
        elapsedTime += Time.deltaTime;
        float per = elapsedTime/stageAllSecond;
        progressBar.DrawProgressBar(per);
        progressBar.DrawPlayerIcon(per);

    }

    /// <summary>
    /// 経過時間に応じて、スポーン間隔を計算して返す
    /// 線形で減少し、minInterval 以下にはならない
    /// </summary>
    float GetCurrentInterval()
    {
        // 線形減少：initialInterval から経過時間 * decayRate を引く
        float interval = initialInterval - elapsedTime * decayRate;
        return Mathf.Max(interval, minInterval);
    }

    /// <summary>
    /// コルーチンで繰り返しスポーン
    /// </summary>
    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // 一度に spawnCount 体ずつスポーン
            for (int i = 0; i < spawnCount; i++)
                SpawnSingleEnemy();

            // 現在の間隔だけ待機
            float waitTime = GetCurrentInterval();
            yield return new WaitForSeconds(waitTime);
        }
    }

    /// <summary>
    /// プレイヤーから指定距離内のランダム位置に敵を１体生成
    /// </summary>
    void SpawnSingleEnemy()
    {
        var prefab = enemies[Random.Range(0, enemies.Length)];

        // XZ平面のランダム方向
        Vector2 circle = Random.insideUnitCircle.normalized;
        float distance = Random.Range(minDistance, maxDistance);
        Vector3 spawnPos = new Vector3(0f, circle.y,circle.x) * distance;

        //Vector3 spawnPos = playerTransform.position + offset;
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        AddItem(enemy);
    }

    void AddItem(GameObject enemy){
        if(Random.value > batteryDropRate) return;
        int index = Random.Range(0, items.Length);
        enemy.GetComponent<Enemy>().item = items[index];

    }
}
