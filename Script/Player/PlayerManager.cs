using System.Collections;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int hp = 5;
    public PlayerController playerController;
    public PlayerItemManager playerItemManager;

    [Header("無敵フレーム設定")]
    public float invincibilityDuration = 2f;   // 無敵時間（秒）
    public float blinkInterval        = 0.1f; // 点滅間隔（秒）

    [Header("エフェクトのオフセット")]
    [SerializeField] private float effectOffset = 0.7f;

    private bool isInvincible = false;
    private Collider playerCollider;
    private int playerLayer;
    private int enemyLayer;
    private int oldHp = 0;
    private bool isDead = false;

    void Start()
    {
        playerCollider = GetComponent<Collider>();
        // レイヤー名はプロジェクト側で設定しておくこと
        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer  = LayerMask.NameToLayer("Enemy");
        
    }

    void Update()
    {
        if(oldHp != hp){
            //HPの表示
            UIManager.Instance.SetHp(hp);
            oldHp = hp;
        }
    }

    //全ての攻撃をoffにする
    public void AllBatteryActiveFalse(){
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
            if(cfg.isGet) return;
            cfg.isGet = true;
            Debug.Log("アイテムゲット：" + cfg.itemType);
            playerItemManager.getItem(cfg.itemType);
            Destroy(other.gameObject);
            EffectController.Instance.PlayPowerUp(gameObject.transform.position);
            
            return;
        }
    }

    // CoreがEnemyとHitしたときに呼ばれる
    public void HitEnemy(){
        if(isInvincible) return;
        Damage();
        if(hp < 0) StartCoroutine(Death());
        StartCoroutine(InvincibilityCoroutine());
    }

    void Damage(){
        hp--;
        Vector3 pos = gameObject.transform.position;
        pos.y += effectOffset;
        EffectController.Instance.PlayHitToPlayer(pos);
    }

    

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        // 敵とのレイヤー衝突を無効化（Player ↔ Enemy）
        Physics.IgnoreLayerCollision(playerLayer, enemyLayer, true);

        // 点滅用に自キャラの Renderer を取得
        var renderers = GetComponentsInChildren<Renderer>();
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
    private IEnumerator Death(){
        isDead = true;
        SoundManager.Instance.StopBGM();
        ScreenFader screenFader = GameObject.Find("ScreenFader").GetComponent<ScreenFader>();
        yield return new WaitForSeconds(0.5f);
        screenFader.FadeToBlack();

    }
}
