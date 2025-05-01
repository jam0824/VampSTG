using UnityEngine;
using System.Collections;

public class BossAlien3 : MonoBehaviour
{
    [SerializeField] int hp = 500;
    int maxHp = 500;

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

    private bool isDead = false;
    private Coroutine attackLoopCoroutine;
    private string tagName = "Boss";

    private float screenHeight = 6f;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        maxHp = hp;
        SetTagName(tagName);

        // コルーチンを保持しておく
        attackLoopCoroutine = StartCoroutine(AttackLoop());
    }

    void Update()
    {
        if (isDead) return;          // 死亡後はUpdate処理をすべてキャンセル
        DrawHpBar();
    }

    private IEnumerator AttackLoop()
    {
        // 生存中だけループ
        while (!isDead)
        {
            float r = Random.value;
            if (r < 0.4f)
                yield return AttackLeftArmCoroutine();
            else if (r < 0.8f)
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
        if (!other.CompareTag("PlayerBullet") || isDead) return;
        if (!other.TryGetComponent<ConfigPlayerBullet>(out var bullet)) return;

        hp -= bullet.getDamage();
        if (bullet.hitSe != null)
            SoundManager.Instance.PlaySE(bullet.hitSe, bullet.hitSeVolume);

        if (hp <= 0)
            Die(other.transform);
    }

    private void Die(Transform hitPoint)
    {
        isDead = true;

        // 進行中のコルーチンをすべて止める
        if (attackLoopCoroutine != null)
            StopCoroutine(attackLoopCoroutine);
        StopAllCoroutines();

        // 他のトリガーをリセット（念のため）
        animator.ResetTrigger("attack1_l");
        animator.ResetTrigger("attack3");
        animator.ResetTrigger("walk");
        animator.ResetTrigger("stopWalk");

        // 死亡アニメ再生
        animator.SetTrigger("dead");
        StartCoroutine(RandomExplosionCoroutine());

        // 必要ならスクリプト自体を無効化
        this.enabled = false;

        //フェードアウト処理
        fader.FadeToWhite(() => {
           
        });


    }

    private IEnumerator RandomExplosionCoroutine(){

    for(int i = 0; i < 50; i++){
        Vector3 pos = transform.position;
        pos.x = 1f; //少し画面の手前に出す
        // y は 0 ～ 6 の範囲
        pos.y = Random.value * screenHeight;
        // z を ±1 の範囲でランダムにずらす
        float zOffset = (Random.value - 0.5f) * 2f; 
        pos.z += zOffset;

        // エフェクト
        EffectController.Instance.PlayLargeExplosion(
            pos,
            transform.rotation
        );
        yield return new WaitForSeconds(0.1f);
    }
}

    // 以下は既存のまま
    void DrawHpBar()
    {
        float per = (maxHp - hp) / (float)maxHp;
        bossHpBar.DrawHpBar(per);
    }

    void SetTagName(string tag)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>(includeInactive: true))
            t.gameObject.tag = tag;
    }
}
