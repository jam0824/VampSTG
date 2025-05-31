using UnityEngine;
using System.Collections;

public abstract class BaseEnemy : MonoBehaviour
{
    [Header("基本設定")]
    [SerializeField] public float hp = 10;
    protected float maxHp = 10;
    
    [Header("エフェクト")]
    [SerializeField] protected GameObject explosion;
    [SerializeField] protected float offsetExplosionY = 0f;
    
    [Header("敵弾攻撃するか")]
    [SerializeField] protected bool isAttack = false;
    [SerializeField] protected float attackInterval = 4f;
    [SerializeField] protected float attackAnimationWait = 0.5f;
    [Header("攻撃設定")]
    [SerializeField] protected bool isDirectionAttack = false;    //Trueの場合方向指定攻撃。falseの場合はcore狙い
    [SerializeField] public float attackDirection = 0f;        //方向指定攻撃の場合の方向
    
    [Header("アニメーション")]
    [SerializeField] protected Animator animator;

    [Header("ボスからのダメージ")]
    [SerializeField] protected int fromBossDamage = 5;
    
    protected IEnemyShooter enemyShooter;
    public GameObject item { get; set; } = null;
    
    protected Transform playerTransform;
    protected bool isDead = false;
    protected bool isAttackAnimation = false;
    

    protected virtual void Start()
    {
        // プレイヤー参照を取得
        var playerObj = GameObject.FindWithTag("Core");
        if (playerObj != null)
            playerTransform = playerObj.transform;
            
        maxHp = hp;
        
        // 攻撃設定の初期化
        if (isAttack)
        {
            enemyShooter = GetComponent<IEnemyShooter>();
            StartCoroutine(AttackCoroutine());
            if (animator == null) animator = GetComponent<Animator>();
        }
        
        // 子クラス固有の初期化
        OnStart();
    }

    protected virtual void Update()
    {
        if (playerTransform == null) return;
        
        // 子クラス固有の移動処理
        HandleMovement();
        
        // 死亡判定
        if ((!isDead) && (hp <= 0)) enemyDie();
    }

    /// <summary>
    /// 子クラスで実装する初期化処理
    /// </summary>
    protected virtual void OnStart() { }

    /// <summary>
    /// 子クラスで実装する移動処理
    /// </summary>
    protected abstract void HandleMovement();

    /// <summary>
    /// 攻撃コルーチン
    /// </summary>
    protected virtual IEnumerator AttackCoroutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(attackInterval);
            if (animator != null)
                animator.SetTrigger("attack");
            isAttackAnimation = true;
            yield return new WaitForSeconds(attackAnimationWait);
            if (enemyShooter != null){
                if (isDirectionAttack){
                    enemyShooter.Fire(attackDirection);
                }
                else{
                    enemyShooter.Fire();
                }
            }
            yield return new WaitForSeconds(1f);
            isAttackAnimation = false;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
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

    protected virtual float hit(ConfigPlayerBullet bullet, float enemyHp)
    {
        float damage = bullet.getDamage();
        Debug.Log("ダメージ：" + damage);
        enemyHp -= damage;
        AudioClip hitSe = bullet.hitSe;
        if (hitSe != null) SoundManager.Instance.PlaySE(bullet.hitSe, bullet.hitSeVolume);
        return enemyHp;
    }

    protected virtual void enemyDie()
    {
        isDead = true;
        Explosion(maxHp);
        AddKillCount();
        AddScore(maxHp);
        ApearItem(item);
        Destroy(gameObject);
    }

    protected virtual void Explosion(float maxHp)
    {
        Vector3 pos = gameObject.transform.position;
        if (offsetExplosionY != 0) pos.y += offsetExplosionY;
        
        if (maxHp < 50)
        {
            EffectController.Instance.PlaySmallExplosion(pos, gameObject.transform.rotation);
            return;
        }
        if (maxHp < 100)
        {
            EffectController.Instance.PlayMiddleExplosion(pos, gameObject.transform.rotation);
            return;
        }
        EffectController.Instance.PlayLargeExplosion(pos, gameObject.transform.rotation);
    }

    protected virtual void ApearItem(GameObject objItem)
    {
        if (objItem == null) return;
        
        // カメラのZ軸の範囲外にいたらアイテム出現しない
        if ((GameManager.Instance.minZ > transform.position.z) || 
            (GameManager.Instance.maxZ < transform.position.z)) 
            return;
            
        Vector3 pos = gameObject.transform.position;
        Instantiate(objItem, pos, gameObject.transform.rotation);
        Debug.Log("アイテム出現");
    }

    protected virtual void AddKillCount()
    {
        GameManager.Instance.killCount++;
        GameManager.Instance.allKillCount++;
    }

    protected virtual void AddScore(float maxHp)
    {
        GameManager.Instance.AddScore(maxHp);
    }
} 