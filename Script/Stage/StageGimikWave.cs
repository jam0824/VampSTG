using UnityEngine;
using System.Collections;

/// <summary>
/// 右側からのみ敵を出現させるギミック用ウェーブクラス
/// </summary>
public class StageGimikWave : MonoBehaviour
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

    [Header("右側スポーン設定")]
    [SerializeField] private float zOffset = 4f;           // maxZからのオフセット距離
    [SerializeField] private float minY = -3f;             // Y軸の最小値
    [SerializeField] private float maxY = 3f;              // Y軸の最大値

    private bool isStartCoroutine = false;
    private bool isSpawn = true;                           // 外部からのストップ指示の時にこれで止める
    private StageManager stageManager = null;
    private float waveElapsedTime = 0f;

    void Start()
    {
        stageManager = GetComponentInParent<StageManager>();
    }

    void Update()
    {
        if(isStartCoroutine) waveElapsedTime += Time.deltaTime;
        CheckWave(stageManager.allElapsedTime);
    }

    /// <summary>
    /// ウェーブの開始・終了条件をチェック
    /// </summary>
    void CheckWave(float allElapsedTime)
    {
        if ((!isStartCoroutine) && 
            (isSpawn) &&
            (startWaveTime <= allElapsedTime) && 
            (endWaveTime >= allElapsedTime))
        {
            isStartCoroutine = true;
            StartCoroutine(SpawnRoutine());
            Debug.Log("GimikWave開始 : " + gameObject.name);
        }
        else if((isStartCoroutine) && 
                (allElapsedTime > endWaveTime))
        {
            StopWave();
        }
        //StageManager側で敵出現をストップしたら
        else if((!stageManager.isSpawnEnemey) &&
                (isStartCoroutine) &&
                (isSpawn))
        {
            StopWave();
            isSpawn = false;
        }
    }

    /// <summary>
    /// ウェーブを停止
    /// </summary>
    void StopWave()
    {
        isStartCoroutine = false;
        StopAllCoroutines();
        Debug.Log("GimikWave終了 : " + gameObject.name);
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
    /// 右側指定位置に敵を１体生成
    /// </summary>
    void SpawnSingleEnemy()
    {
        if(!isSpawn) return;
        
        if(enemies.Length == 0)
        {
            Debug.LogWarning($"StageGimikWave '{gameObject.name}' には敵プレハブが設定されていません");
            return;
        }

        Vector3 spawnPos = SpawnRightSidePosition();
        GameObject enemyPrefab = enemies[Random.Range(0, enemies.Length)];
        
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        stageManager.AddItem(enemy);
        enemy.transform.SetParent(stageManager.enemyPool.transform); // 親をEnemyPoolにする
        GameManager.Instance.AddStageAllEnemyCount();
    }

    /// <summary>
    /// 右側（maxZ + zOffset）のランダムY位置でスポーン位置を決定
    /// </summary>
    Vector3 SpawnRightSidePosition()
    {
        return new Vector3(
            0f,
            Random.Range(minY, maxY),
            GameManager.Instance.maxZ + zOffset
        );
    }

    /// <summary>
    /// 外部からウェーブを停止
    /// </summary>
    public void ForceStopWave()
    {
        isSpawn = false;
        StopWave();
    }

    /// <summary>
    /// ウェーブが実行中かどうか
    /// </summary>
    public bool IsWaveActive()
    {
        return isStartCoroutine;
    }

    /// <summary>
    /// 現在のスポーン間隔を取得（デバッグ用）
    /// </summary>
    public float GetCurrentSpawnInterval()
    {
        return GetCurrentInterval();
    }
} 