// BaseBoss.cs
using UnityEngine;
using System.Collections;

public abstract class BaseBoss : MonoBehaviour, IBoss
{
    [Header("ステータス")]
    [SerializeField] protected float hp = 500f;
    [SerializeField] protected float attackInterval = 2f;
    [SerializeField] protected BossHpBar bossHpBar;
    [SerializeField] protected ScreenFader fader;

    [Header("ホーミングミサイルのターゲット名")]
    [SerializeField] protected string missileTargetName = "MissileTarget";

    protected float maxHp;
    protected PlayerManager playerManager;
    protected GameObject core;
    protected string tagName = "Boss";
    protected bool isDead = false;
    protected bool isStart = false;
    protected bool canDamage = true;
    protected Animator animator;

    protected virtual void Start()
    {
        // 必要ならサブクラスで拡張
    }

    protected virtual void Update()
    {
        if (isDead) return;
        DrawHpBar();
    }

    // IBoss 実装
    public virtual void PlayEntry()
    {
        Debug.Log("ボス出現");
        if (animator == null)
            animator = GetComponent<Animator>();

        core = GameObject.FindGameObjectWithTag("Core");
        playerManager = core.GetComponent<PlayerManager>();

        maxHp = hp;
        SetTagName(tagName);

        StopAllCoroutines();
        StartCoroutine(EntryCoroutine());
    }
    /// <summary>
    /// 共通のトリガー処理。PlayerBullet タグの弾だけを受け付け、
    /// ダメージ計算→共通 SE/エフェクト→死亡処理まで行う。
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!isStart || isDead || !canDamage) return;
        if (!other.CompareTag("PlayerBullet")) return;
        if (!other.TryGetComponent<ConfigPlayerBullet>(out var bullet)) return;

        HandleBulletHit(bullet, other);
    }

    /// <summary>
    /// 弾がヒットしたときの共通処理。
    /// 必要ならサブクラスでオーバーライドして前後に処理をはさめる。
    /// </summary>
    protected virtual void HandleBulletHit(ConfigPlayerBullet bullet, Collider other)
    {
        // HP 減少
        hp -= bullet.getDamage();

        // ヒット SE
        if (bullet.hitSe != null)
            SoundManager.Instance.PlaySE(bullet.hitSe, bullet.hitSeVolume);

        // ヒットエフェクト
        if (bullet.triggerEffect != null)
            Instantiate(
                bullet.triggerEffect,
                other.transform.position,
                other.transform.rotation
            );

        // 弾の破壊
        if (bullet.isDestroy)
            Destroy(other.gameObject);

        // 死亡判定
        if (hp <= 0)
            Die(other.transform);
    }

    protected virtual IEnumerator EntryCoroutine()
    {
        Debug.Log("出現演出開始");
        // 例：エントリー演出
        yield return new WaitForSeconds(5f);
        Debug.Log("5秒終わり");
        bossHpBar.StartFadeIn(3f);
        gameObject.SetActive(true);
        SoundManager.Instance.PlayBGM(GetEntryBGM(), GetEntryBGMVolume());
        yield return new WaitForSeconds(attackInterval);
        isStart = true;
    }

    /// <summary>
    /// 演出用のBGM。サブクラスで差し替えたい場合は override。
    /// </summary>
    protected virtual AudioClip GetEntryBGM() => null;
    protected virtual float GetEntryBGMVolume() => 1f;

    /// <summary>
    /// ダメージによる死亡処理。サブクラスから呼ぶ or override 可。
    /// </summary>
    public virtual void Die(Transform hitPoint)
    {
        isDead = true;
        // 共通：キルカウント＆スコア加算
        AddKillCount();
        AddScore(maxHp);
        // 必要ならフェードアウトなど
    }

    /// <summary>
    /// 子オブジェクトにタグをセット
    /// </summary>
    public virtual void SetTagName(string tag)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t.gameObject.name != missileTargetName)
                t.gameObject.tag = tag;
        }
    }

    /// <summary>
    /// HPバーを更新
    /// </summary>
    public virtual void DrawHpBar()
    {
        float per = (maxHp - hp) / maxHp;
        bossHpBar.DrawBar(per);
    }

    /// <summary>
    /// キル数をインクリメント
    /// </summary>
    public virtual void AddKillCount()
    {
        GameManager.Instance.killCount++;
        GameManager.Instance.allKillCount++;
    }

    /// <summary>
    /// スコアに加算
    /// </summary>
    public virtual void AddScore(float maxHp)
    {
        GameManager.Instance.AddScore(maxHp);
    }
}
