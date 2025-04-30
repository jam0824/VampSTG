using UnityEngine;
using System.Collections;

public class BossAlien3 : MonoBehaviour
{
    [SerializeField] int hp = 500;
    int maxHp = 500;

    [Tooltip("Animator コンポーネント（Inspector でセット、未設定なら同じオブジェクトを自動取得）")]
    public Animator animator;
    [Header("攻撃ポイント")]
    [SerializeField] ScatterShooter acidAttackPoint;
    [SerializeField] ScatterShooter handAttackPoint;

    [Tooltip("攻撃トリガー名（交互に発火させる）")]
    public string[] triggerNames;

    [Tooltip("何秒おきに攻撃アニメを発動するか")]
    public float attackInterval = 2f;
    [SerializeField] private float moveSpeed = 1.0f;      // z=6 → z=-6 に移動する速さ
    [SerializeField] private float turnDuration = 1.0f;   // 180°回転にかける時間
    [Header("UI Settings")]
    [SerializeField] BossHpBar bossHpBar;

    public ScatterShooter scatterShooter;

    private int nextIndex = 0;
    private bool isDead = false;

    private string tagName = "Enemy";

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        this.maxHp = hp;
        SetTagName(tagName);

        StartCoroutine(AttackLoop());
    }

    void Update()
    {
        DrawHpBar();
    }

    void DrawHpBar()
    {
        float per = ((float)maxHp - (float)hp) / (float)maxHp;
        bossHpBar.DrawHpBar(per);
    }

    void SetTagName(string tagName)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(includeInactive: true))
        {
            t.gameObject.tag = tagName;
        }
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            float r = Random.value;  // 0.0～1.0 のランダム値

            if (r < 0.4f)
            {
                // 45% の確率で左腕攻撃
                yield return AttackLeftArmCoroutine();
            }
            else if (r < 0.80f)
            {
                // 次の 45%（合計 90%）で頭攻撃
                yield return AttackHeadCoroutine();
            }
            else
            {
                // 残りの 10% で移動
                yield return AttackMoveCoroutine();
            }

            // 次の攻撃まで指定秒数待機
            yield return new WaitForSeconds(attackInterval);
        }
    }

    private IEnumerator AttackLeftArmCoroutine()
    {
        // トリガー発火
        animator.SetTrigger("attack1_l");
        // 1.4秒待機(手がちょうどいいところに来る時間)
        yield return new WaitForSeconds(1.4f);
        // 発射処理
        handAttackPoint.FireScatter();
    }
    private IEnumerator AttackHeadCoroutine()
    {
        // トリガー発火
        animator.SetTrigger("attack3");
        //攻撃がいいタイミングになるまでの時間
        yield return new WaitForSeconds(1.15f);
        acidAttackPoint.FireScatter();
        //次の行動までのマージン
        yield return new WaitForSeconds(1f);
    }
    private IEnumerator AttackMoveCoroutine()
    {
        // 1. 歩き出しアニメーション
        animator.SetTrigger("walk");

        // 2. 現在の z に応じて行き先を決定（z > 0 → -6、z <= 0 → +6）
        float currentZ = transform.position.z;
        float targetZ = currentZ > 0f ? -6f : 6f;
        Vector3 targetPos = new Vector3(transform.position.x, transform.position.y, targetZ);

        // 3. 毎フレーム少しずつ移動
        while (Mathf.Abs(transform.position.z - targetZ) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        // 4. 歩き停止アニメーション
        animator.SetTrigger("stopWalk");

        // 5. ゆっくり 180° 回転
        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 180f, 0f);
        float elapsed = 0f;
        while (elapsed < turnDuration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(
                startRot,
                endRot,
                elapsed / turnDuration
            );
            yield return null;
        }
    }



    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerBullet")) return;
        if (!other.TryGetComponent<ConfigPlayerBullet>(out var bullet)) return;
        if (isDead) return;

        hp = hit(bullet, hp);
        if (hp <= 0) enemyDie();

    }

    int hit(ConfigPlayerBullet bullet, int enemyHp)
    {
        enemyHp -= bullet.getDamage();
        AudioClip hitSe = bullet.hitSe;
        if (hitSe != null) SoundManager.Instance.PlaySE(bullet.hitSe, bullet.hitSeVolume);
        return enemyHp;
    }

    void enemyDie()
    {
        isDead = true;
        Vector3 pos = gameObject.transform.position;
        EffectController.Instance.PlaySmallExplosion(pos, gameObject.transform.rotation);
        Destroy(gameObject);
    }

}
