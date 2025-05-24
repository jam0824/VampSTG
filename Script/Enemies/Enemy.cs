using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [SerializeField] public float hp = 10;
    private float maxHp = 10;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float rotateSpeed = 90f;   // 度/秒
    [SerializeField] float stopDistance = 0.1f;

    [SerializeField] GameObject explosion;
    [SerializeField] float offsetExplosionY = 0f;
    [Header("敵弾攻撃するか")]
    [SerializeField] bool isAttack = false;
    [SerializeField] float attackInterval = 4f;
    [SerializeField] float attackAnimationWait = 0.5f;
    [Header("アニメーション")]
    [SerializeField]Animator animator;
    IEnemyShooter enemyShooter;

    public GameObject item { get; set; } = null;

    Transform playerTransform;
    bool isDead = false;
    int fromBossDamage = 5; //敵キャラがボスにあたった時のダメージ

    

    void Start()
    {
        var playerObj = GameObject.FindWithTag("Core");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        maxHp = hp;
        if (isAttack)
        {
            enemyShooter = GetComponent<IEnemyShooter>();
            StartCoroutine(AttackCoroutine()); //もし攻撃設定されていたら
            if(animator == null) animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // ─── 移動 ───
        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.x = 0f;
        float dist = toPlayer.magnitude;
        if (dist > stopDistance)
        {
            float step = moveSpeed * Time.deltaTime;
            Vector3 stepPos = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
            stepPos.x = 0f;
            transform.position = stepPos;
        }

        // ─── 常にプレイヤー方向を向く ───
        if (toPlayer.sqrMagnitude > 0.001f) // プレイヤーと同位置だとQuaternion.LookRotationでエラーになるので念のため
        {
            // y成分を無視して水平方向だけで向きを計算
            Vector3 dir = toPlayer.normalized;

            // 目標回転
            Quaternion targetRot = Quaternion.LookRotation(dir);

            // スムーズに回転
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );
        }
        if ((!isDead) && (hp <= 0)) enemyDie();
    }

    private IEnumerator AttackCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackInterval);
            animator.SetTrigger("Attack");
            yield return new WaitForSeconds(attackAnimationWait);
            enemyShooter.Fire();
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        if (other.CompareTag("Boss"))
        {
            hp -= fromBossDamage;
        }
        else if (other.CompareTag("PlayerBullet"))
        {
            if (!other.TryGetComponent<ConfigPlayerBullet>(out var bullet)) return;
            hp = hit(bullet, hp);
            // 近似的に当たり位置を計算
            Vector3 hitPoint = other.ClosestPoint(transform.position);

            if (bullet.triggerEffect != null)
            {
                Instantiate(bullet.triggerEffect, hitPoint, other.gameObject.transform.rotation);
            }
            if (bullet.isDestroy) Destroy(other.gameObject);
        }
        if (hp <= 0) enemyDie();
    }

    float hit(ConfigPlayerBullet bullet, float enemyHp)
    {
        float damage = bullet.getDamage();
        Debug.Log("ダメージ：" + damage);
        enemyHp -= damage;
        AudioClip hitSe = bullet.hitSe;
        if (hitSe != null) SoundManager.Instance.PlaySE(bullet.hitSe, bullet.hitSeVolume);
        return enemyHp;
    }

    void enemyDie()
    {
        isDead = true;
        Vector3 pos = gameObject.transform.position;
        if (offsetExplosionY != 0) pos.y += offsetExplosionY;
        EffectController.Instance.PlaySmallExplosion(pos, gameObject.transform.rotation);
        AddKillCount();
        AddScore(maxHp);
        ApearItem(item);
        Destroy(gameObject);
    }

    void ApearItem(GameObject objItem)
    {
        if (objItem == null) return;
        // カメラのZ軸の範囲外にいたらアイテム出現しない
        if((GameManager.Instance.minZ > transform.position.z) || 
            (GameManager.Instance.maxZ < transform.position.z)) 
            return;
        Vector3 pos = gameObject.transform.position;
        Instantiate(objItem, pos, gameObject.transform.rotation);
        Debug.Log("アイテム出現");
    }

    void AddKillCount()
    {
        GameManager.Instance.killCount++;
        GameManager.Instance.allKillCount++;
    }

    void AddScore(float maxHp)
    {
        GameManager.Instance.AddScore(maxHp);
    }

}
