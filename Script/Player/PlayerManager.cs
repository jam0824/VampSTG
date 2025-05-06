using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int hp = 5;
    public PlayerController playerController;
    public PlayerItemManager playerItemManager;
    public GameObject itemCollectionArea;


    [Header("無敵フレーム設定")]
    public float invincibilityDuration = 2f;   // 無敵時間（秒）
    public float blinkInterval = 0.1f; // 点滅間隔（秒）

    [Header("エフェクトのオフセット")]
    [SerializeField] private float effectOffset = 0.7f;

    [Header("星と倍率のオフセット(星3が1倍)")]
    [SerializeField] private float baseStarOffset = 3f;

    private bool isInvincible = false;
    private Collider playerCollider;
    private int playerLayer;
    private int enemyLayer;
    private int oldHp = 0;
    private bool isDead = false;
    private GameObject playerModel;
    private Animator animator;

    public float powerMagnification = 1f;
    public float speedMagnification = 1f;


    void Start()
    {
        playerModel = Instantiate(GameManager.Instance.selectedCharacter.playModel);  //PlayerModelを作成
        GameManager.Instance.playerCore = gameObject;
        playerCollider = GetComponent<Collider>();
        // レイヤー名はプロジェクト側で設定しておくこと
        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");

        InitializePlayer();
    }

    //CharacterDataのパラメーター反映
    void InitializePlayer()
    {
        CharacterData characterData = GameManager.Instance.selectedCharacter;
        SphereCollider sc = itemCollectionArea.GetComponent<SphereCollider>();

        hp = characterData.life;
        powerMagnification = characterData.power / baseStarOffset;
        speedMagnification = characterData.speed / baseStarOffset;
        playerController.speed *= speedMagnification;
        sc.radius *= characterData.pickupRange / baseStarOffset;
        playerController.playerModelOffset = characterData.playerModelOffset;
        playerItemManager.getItem(characterData.initialItem, powerMagnification);
        //EquipBatteryForModel();
    }

    

    void Update()
    {
        //PlayerModelを取得できるまでループさせる
        if (playerModel == null)
        {
            playerModel = GameObject.FindGameObjectWithTag("PlayerModel");
        }
        if (playerModel != null)
        {
            animator = playerModel.GetComponent<Animator>();
        }
        if (playerModel == null) return;

        if (oldHp != hp)
        {
            //HPの表示
            UIManager.Instance.SetHp(hp);
            oldHp = hp;
        }
    }

    //全ての攻撃をoffにする
    public void AllBatteryActiveFalse()
    {
        playerItemManager.AllBatteryActiveFalse();
    }

    void OnTriggerEnter(Collider other)
    {
        // ——————————————
        // アイテムは常に取得
        // ——————————————
        if (other.CompareTag("Item"))
        {
            var cfg = other.GetComponent<ConfigItem>();
            if (cfg.isGet) return;
            cfg.isGet = true;
            Debug.Log("アイテムゲット：" + cfg.itemType);
            playerItemManager.getItem(cfg.itemType, powerMagnification);
            Destroy(other.gameObject);
            EffectController.Instance.PlayPowerUp(gameObject.transform.position);

            return;
        }

        if (other != null &&
        (other.CompareTag("Enemy") || other.CompareTag("Boss") || other.CompareTag("EnemyBullet")))
        {
            HitEnemy();
        }
    }

    // CoreがEnemyとHitしたときに呼ばれる
    public void HitEnemy()
    {
        if (isInvincible) return;
        Damage();
        SetMiss();
        if (hp < 0) StartCoroutine(Death());
        StartCoroutine(InvincibilityCoroutine());
    }

    void Damage()
    {
        hp--;
        Vector3 pos = gameObject.transform.position;
        pos.y += effectOffset;
        EffectController.Instance.PlayHitToPlayer(pos);
    }

    /// <summary>
    /// そのステージで死んだ回数。ノーミス判定用
    /// </summary>
    void SetMiss()
    {
        GameManager.Instance.stageDeadCount++;
    }



    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        // 敵とのレイヤー衝突を無効化（Player ↔ Enemy）
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        // 点滅用に自キャラの Renderer を取得
        var renderers = playerModel.GetComponentsInChildren<Renderer>();
        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            // 点滅：全 Renderer を on/off
            foreach (var r in renderers)
                r.enabled = !r.enabled;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        // 最終的に表示を戻す
        foreach (var r in renderers)
            r.enabled = true;

        // 敵とのレイヤー衝突を再度有効化
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        isInvincible = false;
    }

    //ゲームオーバー
    private IEnumerator Death()
    {
        isDead = true;
        animator.SetTrigger("Dead");
        playerController.SetCanMove(false);
        playerItemManager.StopAllCoroutine();
        SoundManager.Instance.StopBGM();
        ScreenFader screenFader = GameObject.Find("ScreenFader").GetComponent<ScreenFader>();
        yield return new WaitForSeconds(0.5f);
        screenFader.FadeToBlack(GameManager.Instance.whenDeathToSceneName);

    }

    /// <summary>
    /// Batteryをモデルの位置に配置する
    /// </summary>
    public void EquipBatteryForModel()
    {
        foreach (GameObject battery in playerItemManager.items)
        {
            IItem iitem = battery.GetComponent<IItem>();
            string type = iitem.itemType;  // 例: "RedBattery", "BlueBattery" など

            // playerModel 以下の全子 Transform（自分自身含む）を一括取得
            Transform[] allChildren = playerModel.GetComponentsInChildren<Transform>(true);

            // 名前に type を含む最初のものを探す
            Transform slot = null;
            foreach (var t in allChildren)
            {
                if (t.name.Contains(type))
                {
                    slot = t;
                    break;
                }
            }

            if (slot != null)
            {
                // スロットにバッテリーをアタッチ
                battery.transform.SetParent(slot, false);
                battery.transform.localPosition = Vector3.zero;
                battery.transform.localRotation = Quaternion.identity;
            }
            else
            {
                Debug.LogWarning($"型名 '{type}' を含むスロットが見つかりませんでした。");
            }
        }
    }
}
