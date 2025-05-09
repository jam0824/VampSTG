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
    public float allElapsedTime = 0f;   // ゲーム起動からの経過時間
    bool isBoss = false;
    public bool isSpawnEnemey = true;   //敵キャラをspawnするか


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

    void DrawProgressBar(float elapsedTime, float stageAllSecond)
    {
        float per = elapsedTime / stageAllSecond;
        progressBar.DrawBar(per);
        progressBar.DrawPlayerIcon(per);
    }

    public void AddItem(GameObject enemy)
    {
        if (Random.value > batteryDropRate) return;
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
