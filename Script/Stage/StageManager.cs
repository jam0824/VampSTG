using UnityEngine;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;

public class StageManager : MonoBehaviour
{
    [Header("Stage Settings")]
    [SerializeField] float stageAllSecond = 180f;
    GameObject[] enemies;      // スポーンする敵プレハブ
    [SerializeField] GameObject[] items;
    int spawnCount = 1;        // 一度のタイミングでスポーンする数

    [Header("Spawn Radius")]
    [SerializeField] float minDistance = 5f;    // プレイヤーから最低この距離以上
    [SerializeField] float maxDistance = 15f;   // プレイヤーから最大この距離以内
    [SerializeField] GameObject enemyPool;  //敵をまとめる場所

    float initialInterval = 5f;    // ゲーム開始直後のスポーン間隔（秒）
    float minInterval = 0.5f;      // 最短スポーン間隔（秒）
    float decayRate = 0.05f;       // インターバル減少率（秒／秒）

    [Header("Item Settings")]
    [SerializeField] float batteryDropRate = 0.1f;
    [Header("Boss")]
    [SerializeField] GameObject boss;
    [Header("BGM")]
    [SerializeField] AudioClip bgm;
    [SerializeField] float bgmVol = 0.8f;

    [Header("UI Settings")]
    [SerializeField] ProgressBar progressBar;
    [Header("PlayerModel")]
    [SerializeField] public GameObject playerModel;

    Transform playerTransform;
    StageWave[] stageWaves;
    StageWave nowStageWave = null;
    float allElapsedTime = 0f;   // ゲーム起動からの経過時間
    float waveElapsedTime = 0f;
    bool isBoss = false;
    bool isSpawnEnemy = true;
    GameManager gm;

    void Start()
    {
        GameManager.Instance.ResetStageSaveData();  //ステージ開始時にリセットすべきデータを全てリセット
        var playerObj = GameObject.FindWithTag("Core");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogError("Player オブジェクトが見つかりません");

        SoundManager.Instance.PlayBGM(bgm, bgmVol);

        stageWaves = GetChildStageWave();

        allElapsedTime = 0f;

        StartCoroutine(SpawnRoutine());
    }

    void Update()
    {
        // 経過時間をカウント
        allElapsedTime += Time.deltaTime;
        waveElapsedTime += Time.deltaTime;
        //時間前まではProgressBarを描画
        if(allElapsedTime < stageAllSecond) DrawProgressBar(allElapsedTime, stageAllSecond);
        if ((allElapsedTime >= stageAllSecond) && (!isBoss)) AppearBoss();
    }

    //ボス登場
    void AppearBoss()
    {
        isBoss = true;
        boss.SetActive(true);
        boss.GetComponent<IBoss>().PlayEntry();
        SoundManager.Instance.StopBGMWithFadeOut(2f);
        progressBar.StartFadeOut(1f);
    }

    public void SetSpawnEnemyFlag(bool isSpawn)
    {
        isSpawnEnemy = isSpawn;
    }

    void CheckWave(float allElapsedTime)
    {
        foreach (StageWave stageWave in stageWaves)
        {
            //全体の経過時間がstagewaveの開始時間終了時間以内に収まっていたら
            if ((stageWave.startWaveTime <= allElapsedTime) && (stageWave.endWaveTime >= allElapsedTime))
            {
                //wave切り替え
                if (nowStageWave != stageWave)
                {
                    this.nowStageWave = stageWave;
                    InitWave(stageWave);
                    return;
                }
            }
        }
    }

    void InitWave(StageWave stageWave)
    {
        Debug.Log("ウェーブ初期化");
        this.enemies = stageWave.enemies;
        this.spawnCount = stageWave.spawnCount;
        this.initialInterval = stageWave.initialInterval;
        this.minInterval = stageWave.minInterval;
        this.decayRate = stageWave.decayRate;
        this.waveElapsedTime = 0f;
    }

    void DrawProgressBar(float elapsedTime, float stageAllSecond)
    {
        float per = elapsedTime / stageAllSecond;
        progressBar.DrawBar(per);
        progressBar.DrawPlayerIcon(per);
    }



    /// <summary>
    /// コルーチンで繰り返しスポーン
    /// </summary>
    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            // フラグが false なら毎フレームここで止める
            if (!isSpawnEnemy)
            {
                yield return null;
                continue;
            }
            CheckWave(allElapsedTime);
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
        var prefab = enemies[Random.Range(0, enemies.Length)];

        // XZ平面のランダム方向
        Vector2 circle = Random.insideUnitCircle.normalized;
        float distance = Random.Range(minDistance, maxDistance);
        Vector3 spawnPos = new Vector3(0f, circle.y, circle.x) * distance;

        //Vector3 spawnPos = playerTransform.position + offset;
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        AddItem(enemy);
        enemy.transform.SetParent(enemyPool.transform); //親をEnemyPoolにする
    }

    void AddItem(GameObject enemy)
    {
        if (Random.value > batteryDropRate) return;
        int index = Random.Range(0, items.Length);
        enemy.GetComponent<Enemy>().item = items[index];

    }

    /// <summary>
    /// このオブジェクト配下のすべての子 StageWave（自分自身を除く）を返します
    /// </summary>
    private StageWave[] GetChildStageWave()
    {
        return GetComponentsInChildren<StageWave>()
               .Where(t => t != transform)
               .ToArray();
    }

    /// <summary>
    /// enemyPool 配下のすべての Enemy の hp を 0 にする
    /// </summary>
    public void KillAllEnemies()
    {
        if (enemyPool == null)
        {
            Debug.LogWarning("enemyPool がアサインされていません");
            return;
        }

        // 子オブジェクトを順に走査
        foreach (Transform child in enemyPool.transform)
        {
            var enemyComp = child.GetComponent<Enemy>();
            if (enemyComp != null)
            {
                enemyComp.hp = 0;
            }
        }
    }
}
