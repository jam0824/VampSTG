using UnityEngine;
using System.Collections;

public class BossAlien3 : MonoBehaviour, IBoss
{
    [SerializeField] float hp = 500;
    float maxHp = 500;

    public Animator animator;
    [SerializeField] ScatterShooter acidAttackPoint;
    [SerializeField] ScatterShooter handAttackPoint;
    [SerializeField] RandomBulletShooter[] directionAttacks;
    [SerializeField] int directionAttackTime = 300;
    public float attackInterval = 2f;
    [SerializeField] private float moveSpeed = 1.0f;
    [SerializeField] private float turnDuration = 1.0f;
    [SerializeField] BossHpBar bossHpBar;
    [SerializeField] private ScreenFader fader;

    [SerializeField] private float entryStartZ = 10f;  // 登場開始位置（Inspectorで調整可）
    [SerializeField] private float entryEndZ = 6f;   // 登場終了位置（Inspectorで調整可）
    [SerializeField] private StageManager stageManager;
    [Header("BGM")]
    [SerializeField] AudioClip bgm;
    [SerializeField] float bgmVol = 0.8f;
    [Header("ホーミングミサイルのターゲットオブジェクトのname")]
    [SerializeField] private string missileTargetName = "MissileTarget";

    private bool isDead = false;
    private bool isStart = false;   //スタート演出が終わったか
    private Coroutine attackLoopCoroutine;
    private string tagName = "Boss";


    private float screenHeight = 6f;
    private PlayerManager playerManager;


    void Update()
    {
        if (isDead) return;          // 死亡後はUpdate処理をすべてキャンセル
        DrawHpBar();
    }

    /// <summary>
    /// 外部から呼び出して、登場演出を開始します
    /// </summary>
    public void PlayEntry()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        playerManager = GameObject.FindGameObjectWithTag("Core").GetComponent<PlayerManager>();
        maxHp = hp;
        SetTagName(tagName);

        // もし他のコルーチンが動いていたらクリア
        StopAllCoroutines();

        // 初期位置をセット
        Vector3 pos = transform.position;
        pos.z = entryStartZ;
        transform.position = pos;

        // 登場コルーチンを開始
        StartCoroutine(EntryCoroutine());
    }

    private IEnumerator EntryCoroutine()
    {
        //少し待ち。静けさ。
        yield return new WaitForSeconds(5f);

        bossHpBar.StartFadeIn(3f);  // HPバー表示

        // 歩行アニメ
        animator.SetTrigger("walk");
        Vector3 targetPos = new Vector3(transform.position.x, transform.position.y, entryEndZ);

        while (Mathf.Abs(transform.position.z - entryEndZ) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        // 歩行停止アニメ
        animator.SetTrigger("stopWalk");
        SoundManager.Instance.PlayBGM(bgm, bgmVol);
        yield return new WaitForSeconds(attackInterval);
        isStart = true;
        // 登場完了したら攻撃ループを開始
        attackLoopCoroutine = StartCoroutine(AttackLoop());
    }

    private IEnumerator AttackLoop()
    {
        // 生存中だけループ
        while (!isDead)
        {
            float r = Random.value;
            if (r < 0.45f)
                yield return AttackLeftArmCoroutine();
            else if (r < 0.9f)
                yield return AttackHeadCoroutine();
            else
                yield return AttackMoveCoroutine();

            yield return new WaitForSeconds(attackInterval);
        }
    }

    private IEnumerator AttackLeftArmCoroutine()
    {
        if (isDead) yield break;
        animator.SetTrigger("attack1_l");
        yield return new WaitForSeconds(1.4f);
        if (isDead) yield break;
        handAttackPoint.FireScatter();
    }

    private IEnumerator AttackHeadCoroutine()
    {
        if (isDead) yield break;
        animator.SetTrigger("attack3");
        yield return new WaitForSeconds(1.15f);
        if (isDead) yield break;
        acidAttackPoint.FireScatter();
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator AttackMoveCoroutine()
    {
        if (isDead) yield break;
        animator.SetTrigger("walk");

        float currentZ = transform.position.z;
        float targetZ = currentZ > 0f ? -6f : 6f;
        Vector3 targetPos = new Vector3(transform.position.x, transform.position.y, targetZ);

        int frameCount = 0;
        while (Mathf.Abs(transform.position.z - targetZ) > 0.01f)
        {
            if (isDead) yield break;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            if (frameCount % directionAttackTime == 0)
            {
                foreach (var shooter in directionAttacks)
                {
                    shooter.Fire();
                }
            }

            frameCount++;
            yield return null;
        }

        if (isDead) yield break;
        animator.SetTrigger("stopWalk");

        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 180f, 0f);
        float elapsed = 0f;
        while (elapsed < turnDuration)
        {
            if (isDead) yield break;

            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / turnDuration);
            yield return null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isStart) return;    //スタート演出が終わるまでダメージは受けない
        if (!other.CompareTag("PlayerBullet") || isDead) return;
        if (!other.TryGetComponent<ConfigPlayerBullet>(out var bullet)) return;

        hp -= bullet.getDamage();
        if (bullet.hitSe != null)
            SoundManager.Instance.PlaySE(bullet.hitSe, bullet.hitSeVolume);
        if (bullet.triggerEffect != null)
        {
            Instantiate(bullet.triggerEffect,
                other.gameObject.transform.position,
                other.gameObject.transform.rotation);
        }
        if (bullet.isDestroy) Destroy(other.gameObject);

        if (hp <= 0)
            Die(other.transform);
    }

    private void Die(Transform hitPoint)
    {
        isDead = true;
        StopEverything();

        // 死亡アニメ再生
        animator.SetTrigger("dead");
        StartCoroutine(RandomExplosionCoroutine());
        StartCoroutine(FadeOut());
        AddKillCount();
        AddScore(maxHp);

        // 必要ならスクリプト自体を無効化
        this.enabled = false;

    }

    private void StopEverything()
    {
        playerManager.AllBatteryActiveFalse();  //全てのBatteryを止める
        stageManager.isSpawnEnemey = false; //全ての敵の出現を止める
        stageManager.KillAllEnemies();  //雑魚敵を全て消す
        SoundManager.Instance.StopBGM();    //BGMを消す
        // 進行中のコルーチンをすべて止める
        if (attackLoopCoroutine != null)
            StopCoroutine(attackLoopCoroutine);
        StopAllCoroutines();
        // 他のトリガーをリセット（念のため）
        /*
        animator.ResetTrigger("attack1_l");
        animator.ResetTrigger("attack3");
        animator.ResetTrigger("walk");
        animator.ResetTrigger("stopWalk");
        */
    }

    private IEnumerator FadeOut()
    {
        //フェードアウトまでの待ち時間
        yield return new WaitForSeconds(3f);
        //フェードアウト処理
        fader.FadeToWhite("Result");
    }

    private IEnumerator RandomExplosionCoroutine()
    {

        for (int i = 0; i < 50; i++)
        {
            Vector3 pos = transform.position;
            pos.x = 1f; //少し画面の手前に出す
                        // y は 0 ～ 6 の範囲
            pos.y = Random.value * screenHeight;
            // z を ±1 の範囲でランダムにずらす
            float zOffset = (Random.value - 0.5f) * 2f;
            pos.z += zOffset;

            //ランダム爆発
            float r = Random.value;
            if (r < 0.3)
            {
                EffectController.Instance.PlaySmallExplosion(pos,transform.rotation);

            }
            else if (r < 0.6)
            {
                EffectController.Instance.PlayMiddleExplosion(pos,transform.rotation);
            }
            else
            {
                EffectController.Instance.PlayLargeExplosion(pos,transform.rotation);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    // 以下は既存のまま
    void DrawHpBar()
    {
        float per = (maxHp - hp) / (float)maxHp;
        bossHpBar.DrawBar(per);
    }

    void SetTagName(string tag)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(includeInactive: true))
            if (t.gameObject.name != missileTargetName) t.gameObject.tag = tag;
    }

    void AddKillCount(){
        GameManager.Instance.killCount++;
        GameManager.Instance.allKillCount++;
    }

    void AddScore(float maxHp){
        GameManager.Instance.AddScore(maxHp);
    }
}
