using UnityEngine;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;

public class StageManager : MonoBehaviour
{
    [Header("Stage Settings")]
    [SerializeField] float stageAllSecond = 180f;
    [SerializeField] GameObject[] items;

    [Header("Spawn Radius")]
    [SerializeField] public float minDistance = 5f;    // プレイヤーから最低この距離以上
    [SerializeField] public float maxDistance = 15f;   // プレイヤーから最大この距離以内
    [SerializeField] public GameObject enemyPool;  //敵をまとめる場所

    [Header("Item Settings")]
    [SerializeField] float initialDropRate      = 0.10f;    //最初のアイテムドロップ率は10%
    [SerializeField] float minDropRate          = 0.01f;    //最低のドロップ率は1%
    [SerializeField] int maxEnemiesForMinRate = 100;    //最低になる時の敵数は100

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
    public float allElapsedTime = 0f;   // ゲーム起動からの経過時間
    bool isBoss = false;
    public bool isSpawnEnemey = true;   //敵キャラをspawnするか


    void Awake()
    {

    }
    void Start()
    {
        GameManager.Instance.ResetStageSaveData();  //ステージ開始時にリセットすべきデータを全てリセット
        var playerObj = GameObject.FindWithTag("Core");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        else
            Debug.LogError("Player オブジェクトが見つかりません");

        SoundManager.Instance.PlayBGM(bgm, bgmVol);

        allElapsedTime = 0f;
    }

    void Update()
    {
        // 経過時間をカウント
        allElapsedTime += Time.deltaTime;
        //時間前まではProgressBarを描画
        if (allElapsedTime < stageAllSecond) DrawProgressBar(allElapsedTime, stageAllSecond);
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

    void DrawProgressBar(float elapsedTime, float stageAllSecond)
    {
        float per = elapsedTime / stageAllSecond;
        progressBar.DrawBar(per);
        progressBar.DrawPlayerIcon(per);
    }

    /// <summary>
    /// 敵にアイテムを付与。敵数に応じて指数関数的にアイテムドロップ率を下げる
    /// </summary>
    /// <param name="enemy"></param>
    public void AddItem(GameObject enemy)
    {
        int enemyCount = enemyPool.transform.childCount;
        float ratio = Mathf.Clamp01(enemyCount / (float)maxEnemiesForMinRate);

        // minRate/initialRate を maxEnemies までに掛け合わせる
        float scale = minDropRate / initialDropRate;      // =0.01/0.10 = 0.1
        float dropRate = initialDropRate * Mathf.Pow(scale, ratio);
        // ratio=0 → dropRate = initialDropRate2
        // ratio=1 → dropRate = initialDropRate2 * scale = minDropRate2

        if (Random.value >= dropRate) return;

        int index = Random.Range(0, items.Length);
        enemy.GetComponent<Enemy>().item = items[index];
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
