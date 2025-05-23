using UnityEngine;
using System.Collections;

public class StageWave : MonoBehaviour
{
    [Header("ウェーブの時間")]
    [SerializeField] public float startWaveTime = 0f;
    [SerializeField] public float endWaveTime = 60f;

    [Header("Enemy Settings")]
    [SerializeField] public GameObject[] enemies;      // スポーンする敵プレハブ
    [SerializeField] public int spawnCount = 1;        // 一度のタイミングでスポーンする数

    [Header("Spawn Timing")]
    [SerializeField] public float initialInterval = 5f;    // ゲーム開始直後のスポーン間隔（秒）
    [SerializeField] public float minInterval = 0.5f;      // 最短スポーン間隔（秒）
    [SerializeField] public float decayRate = 0.05f;       // インターバル減少率（秒／秒）

    bool isStartCoroutine = false;
    bool isSpawn  = true;   //外部からのストップ支持の時にこれで止める
    StageManager stageManager = null;
    float waveElapsedTime = 0f;

    void Start()
    {
        stageManager = GetComponentInParent<StageManager>();
    }

    void Update()
    {
        if(isStartCoroutine) waveElapsedTime += Time.deltaTime;
        CheckWave(stageManager.allElapsedTime);
    }

    void CheckWave(float allElapsedTime){
        if ((!isStartCoroutine) && 
            (isSpawn) &&
            (startWaveTime <= allElapsedTime) && 
            (endWaveTime >= allElapsedTime))
        {
            isStartCoroutine = true;
            StartCoroutine(SpawnRoutine());
            Debug.Log("Wave開始 : " + gameObject.name);
        }
        else if((isStartCoroutine) && 
                (allElapsedTime > endWaveTime))
        {
            StopWave();
        }
        //StageManager側で敵出現をストップしたら
        else if((!stageManager.isSpawnEnemey)&&
                (isStartCoroutine) &&
                (isSpawn))
        {
            StopWave();
            isSpawn = false;
        }
    }

    void StopWave(){
        isStartCoroutine = false;
        StopAllCoroutines();
        Debug.Log("Wave終了 : " + gameObject.name);
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
    /// 経過時間に応じて、スポーン間隔を計算して返す
    /// 線形で減少し、minInterval 以下にはならない
    /// </summary>
    float GetCurrentInterval()
    {
        // 線形減少：initialInterval から経過時間 * decayRate を引く
        float interval = initialInterval - waveElapsedTime * decayRate;
        return Mathf.Max(interval, minInterval);
    }

    /// <summary>
    /// プレイヤーから指定距離内のランダム位置に敵を１体生成
    /// </summary>
    void SpawnSingleEnemy()
    {
        if(!isSpawn) return;
        var prefab = enemies[Random.Range(0, enemies.Length)];

        // XZ平面のランダム方向
        Vector2 circle = Random.insideUnitCircle.normalized;
        float distance = Random.Range(stageManager.minDistance, stageManager.maxDistance);
        Vector3 spawnPos = new Vector3(0f, circle.y, circle.x) * distance;

        //Vector3 spawnPos = playerTransform.position + offset;
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        stageManager.AddItem(enemy);
        enemy.transform.SetParent(stageManager.enemyPool.transform); //親をEnemyPoolにする
    }


}
