// BaseBoss.cs
using UnityEngine;
using System.Collections;
using UnityEditor.Rendering;
using Unity.IO.LowLevel.Unsafe;

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
    protected StageManager stageManager;
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
        var tmp = GameObject.Find("StageManager");
        stageManager = tmp.GetComponent<StageManager>();

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
        //貫通系の場合複数回ダメージをウケないようにするため敵を登録しておく
        if(!bullet.isDestroy){
            if(bullet.isHitEnemy(gameObject)){
                return;
            }
            bullet.addHitEnemy(gameObject);
        }

        // HP 減少
        hp -= bullet.getDamage();
        // 近似的に当たり位置を計算
        Vector3 hitPoint = other.ClosestPoint(transform.position);

        // ヒット SE
        if (bullet.hitSe != null)
            SoundManager.Instance.PlaySE(bullet.hitSe, bullet.hitSeVolume);

        // ヒットエフェクト
        if (bullet.triggerEffect != null)
            Instantiate(
                bullet.triggerEffect,
                hitPoint,
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
        stageManager.scrollSpeed = 0f;  //ボス出現時にスクロールを止める
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
        StopEverything();

        // 死亡アニメ再生
        animator.SetTrigger("dead");
        StartCoroutine(RandomExplosionCoroutine());
        StartCoroutine(FadeOut());
        AddKillCount();
        AddScore(maxHp);
        ChangeMuteki();

        // 必要ならスクリプト自体を無効化
        this.enabled = false;
    }

    protected virtual void StopEverything()
    {
        playerManager.AllBatteryActiveFalse();  //全てのBatteryを止める
        stageManager.isSpawnEnemey = false; //全ての敵の出現を止める
        stageManager.KillAllEnemies();  //雑魚敵を全て消す
        SoundManager.Instance.StopBGM();    //BGMを消す
        StopAllCoroutines();
    }

    protected virtual IEnumerator FadeOut()
    {
        //フェードアウトまでの待ち時間
        yield return new WaitForSeconds(3f);
        //フェードアウト処理
        fader.FadeToWhite("Result");
    }

    protected virtual IEnumerator RandomExplosionCoroutine()
    {

        for (int i = 0; i < 50; i++)
        {
            Vector3 pos = transform.position;
            pos.x = 1f; //少し画面の手前に出す
                        // y は 0 ～ 6 の範囲
            pos.y = Random.value * GameManager.Instance.maxY;
            // z を ±1 の範囲でランダムにずらす
            float zOffset = (Random.value - 0.5f) * 2f;
            pos.z += zOffset;

            //ランダム爆発
            float r = Random.value;
            if (r < 0.3)
            {
                EffectController.Instance.PlaySmallExplosion(pos, transform.rotation, false);

            }
            else if (r < 0.6)
            {
                EffectController.Instance.PlayMiddleExplosion(pos, transform.rotation, false);
            }
            else
            {
                EffectController.Instance.PlayLargeExplosion(pos, transform.rotation, false);
            }

            yield return new WaitForSeconds(0.1f);
        }
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
    public virtual void ChangeMuteki()
    {
        playerManager.isMuteki = true;
    }
}
