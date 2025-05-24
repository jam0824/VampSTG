using UnityEngine;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using System.Collections.Generic;

public class StageManager : MonoBehaviour
{
    [Header("Stage Settings")]
    [SerializeField] float stageAllSecond = 180f;
    [SerializeField] List<ItemData> items = new List<ItemData>();

    [Header("Spawn Radius")]
    [SerializeField] public float minDistance = 5f;    // プレイヤーから最低この距離以上
    [SerializeField] public float maxDistance = 15f;   // プレイヤーから最大この距離以内
    [SerializeField] public GameObject enemyPool;  //敵をまとめる場所

    [Header("Item Settings")]
    [SerializeField] float initialDropRate      = 0.10f;    //最初のアイテムドロップ率は10%
    [SerializeField] float minDropRate          = 0.01f;    //最低のドロップ率は1%
    [SerializeField] int maxEnemiesForMinRate = 100;    //最低になる時の敵数は100
    [Header("Itemドロップ率2倍の期間")]
    [SerializeField] float itemDropRate2xPeriod = 30f;    //30秒間

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

        AddItemsFromGameManager();  //GameManagerのアイテムを追加

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

        //アイテムドロップ率2倍の期間中はドロップ率を2倍にする
        if (allElapsedTime < itemDropRate2xPeriod)
        {
            dropRate *= 2f;
        }
        Debug.Log("dropRate: " + dropRate.ToString("F3"));
        if (Random.value >= dropRate) return;

        int index = Random.Range(0, items.Count);
        Debug.Log("ドロップ予定アイテム: " + items[index].type);
        if(items[index].itemObj == null)
        {
            Debug.LogError("アイテムオブジェクトが設定されていません : " + items[index].type);
            return;
        }
        enemy.GetComponent<Enemy>().item = items[index].itemObj;
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

    /// <summary>
    /// GameManagerのgotItems（アンロック済みアイテム）からアイテムを追加する
    /// </summary>
    private void AddItemsFromGameManager()
    {
        var gotItems = GameManager.Instance.gotItems;
        AddItemsFromList(gotItems, "アンロック済みアイテム", "有効なアンロック済みアイテムがありません");
    }

    /// <summary>
    /// GameManagerのstageGetNewItems（ステージで新しく取得したアイテム）からアイテムを追加する
    /// </summary>
    public void AddItemsFromStageGetNewItems()
    {
        var stageGetNewItems = GameManager.Instance.stageGetNewItems;
        AddItemsFromList(stageGetNewItems, "ステージ新規取得アイテム", "有効なステージ新規取得アイテムがありません");
    }

    /// <summary>
    /// アイテムリストからアイテムを重複チェック付きで追加する共通メソッド
    /// </summary>
    /// <param name="itemTypeList">追加するアイテムタイプのリスト</param>
    /// <param name="logPrefix">ログに表示するプレフィックス</param>
    /// <param name="emptyWarningMessage">有効なアイテムがない場合の警告メッセージ</param>
    private void AddItemsFromList(List<string> itemTypeList, string logPrefix, string emptyWarningMessage)
    {
        if (GameManager.Instance?.itemDataDB == null)
        {
            Debug.LogWarning("GameManagerまたはItemDataDBが見つかりません");
            return;
        }

        if (itemTypeList.Count == 0)
        {
            Debug.LogWarning($"{logPrefix}がありません");
            return;
        }

        // itemTypeListからItemDataを取得（重複チェック付き）
        var validItemDataList = new List<ItemData>();
        foreach (string itemType in itemTypeList)
        {
            ItemData itemData = GameManager.Instance.itemDataDB.GetItemData(itemType);
            if (itemData != null)
            {
                // 既存のitemsリストに同じタイプのアイテムが存在するかチェック
                bool alreadyExists = items.Any(existingItem => existingItem.type.ToLower() == itemType.ToLower());
                if (!alreadyExists)
                {
                    validItemDataList.Add(itemData);
                }
                else
                {
                    Debug.Log($"アイテムタイプ '{itemType}' は既に存在するためスキップしました");
                }
            }
            else
            {
                Debug.LogWarning($"アイテムタイプ '{itemType}' に対応するItemDataが見つかりません");
            }
        }

        if (validItemDataList.Count == 0)
        {
            Debug.LogWarning(emptyWarningMessage);
            return;
        }

        // アイテムをitemsリストに追加
        items.AddRange(validItemDataList);
        
        Debug.Log($"{logPrefix}から{validItemDataList.Count}個のアイテムを追加しました。合計アイテム数: {items.Count}");
    }
}
