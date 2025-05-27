using UnityEngine;
using System.Collections;

public class GroundEnemy : MonoBehaviour
{
    [SerializeField] public float hp = 10;
    private float maxHp = 10;
    
    [Header("移動設定")]
    [SerializeField] float moveSpeed = 1f;          // 独自の移動速度
    [SerializeField] float rotateSpeed = 90f;       // Y軸回転速度（度/秒）
    [SerializeField] float stopProbability = 0.8f; // 止まっている確率（0.8 = 80%）
    [SerializeField] float moveCheckInterval = 2f;  // 移動判定の間隔（秒）
    
    [Header("スクロール設定")]
    [SerializeField] float scrollSpeed = 0.6f;      // BGスクロールスピード
    
    [Header("エフェクト")]
    [SerializeField] GameObject explosion;
    [SerializeField] float offsetExplosionY = 0f;
    
    [Header("敵弾攻撃するか")]
    [SerializeField] bool isAttack = false;
    [SerializeField] float attackInterval = 4f;
    [SerializeField] float attackAnimationWait = 0.5f;
    
    [Header("アニメーション")]
    [SerializeField] Animator animator;
    
    IEnemyShooter enemyShooter;
    public GameObject item { get; set; } = null;

    Transform playerTransform;
    bool isDead = false;
    bool isMoving = false;
    int fromBossDamage = 5;
    
    // BGスクロールスピード取得用
    IScrollSpeed scrollSpeedProvider;

    void Start()
    {
        var playerObj = GameObject.FindWithTag("Core");
        if (playerObj != null)
            playerTransform = playerObj.transform;
            
        maxHp = hp;
        
        // BGスクロールスピードを取得
        GetScrollSpeed();
        
        if (isAttack)
        {
            enemyShooter = GetComponent<IEnemyShooter>();
            StartCoroutine(AttackCoroutine());
            if(animator == null) animator = GetComponent<Animator>();
        }
        
        // 移動判定のコルーチンを開始
        StartCoroutine(MoveCheckCoroutine());
    }

    void Update()
    {
        if (playerTransform == null) return;

        // ─── Y軸のみの回転でプレイヤー方向を向く ───
        RotateTowardsPlayer();
        
        // ─── BGスクロールスピードに合わせてz軸方向に移動 ───
        MoveWithScroll();
        
        // ─── 独自の移動（z軸のみ） ───
        if (isMoving)
        {
            MoveForward();
        }
        
        // ─── x軸を0に固定 ───
        FixXPosition();
        
        // ─── 削除判定 ───
        CheckForDestroy();
        
        if ((!isDead) && (hp <= 0)) enemyDie();
    }
    
    /// <summary>
    /// BGスクロールスピードを取得
    /// </summary>
    void GetScrollSpeed()
    {
        // Stage3BGオブジェクトからIScrollSpeedを取得
        GameObject BG = GameObject.FindWithTag("BG");
        if (BG != null)
        {
            scrollSpeedProvider = BG.GetComponent<IScrollSpeed>();
            if (scrollSpeedProvider != null)
            {
                scrollSpeed = scrollSpeedProvider.ScrollSpeed;
            }
        }
    }
    
    /// <summary>
    /// Y軸のみの回転でプレイヤー方向を向く
    /// </summary>
    void RotateTowardsPlayer()
    {
        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0f; // Y成分を無視
        
        if (toPlayer.sqrMagnitude > 0.001f)
        {
            // Y軸のみの回転を計算
            Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized);
            
            // Y軸のみの回転に制限
            Vector3 eulerAngles = targetRot.eulerAngles;
            targetRot = Quaternion.Euler(0f, eulerAngles.y, 0f);
            
            // スムーズに回転
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );
        }
    }
    
    /// <summary>
    /// BGのスクロールスピードに合わせてz軸方向に移動
    /// </summary>
    void MoveWithScroll()
    {
        // x軸は0のまま、z軸方向のみ移動
        Vector3 scrollMovement = new Vector3(0f, 0f, -scrollSpeed * Time.deltaTime);
        transform.Translate(scrollMovement, Space.World);
    }
    
    /// <summary>
    /// 独自の前進移動（z軸方向のみ）
    /// </summary>
    void MoveForward()
    {
        // z軸方向のみの移動（x軸は0のまま、Y軸は固定）
        Vector3 forward = transform.forward;
        forward.x = 0f; // x軸は0のまま
        forward.y = 0f; // Y軸は固定
        forward.Normalize();
        
        transform.Translate(forward * moveSpeed * Time.deltaTime, Space.World);
    }
    
    /// <summary>
    /// 移動するかどうかを定期的に判定
    /// </summary>
    IEnumerator MoveCheckCoroutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(moveCheckInterval);
            
            // 確率で移動するかどうかを決定
            float randomValue = Random.Range(0f, 1f);
            isMoving = randomValue > stopProbability; // stopProbabilityより大きい場合のみ移動
            
            // 移動時間をランダムに設定（0.5秒〜2秒）
            if (isMoving)
            {
                float moveTime = Random.Range(0.5f, 2f);
                StartCoroutine(StopMovingAfterTime(moveTime));
            }
        }
    }
    
    /// <summary>
    /// 指定時間後に移動を停止
    /// </summary>
    IEnumerator StopMovingAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        isMoving = false;
    }
    
    /// <summary>
    /// x軸を0に固定
    /// </summary>
    void FixXPosition()
    {
        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.x) > 0.001f) // 微小な誤差を許容
        {
            pos.x = 0f;
            transform.position = pos;
        }
    }
    
    /// <summary>
    /// z軸方向の削除判定
    /// </summary>
    void CheckForDestroy()
    {
        if (transform.position.z < GameManager.Instance.minZ)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 攻撃コルーチン
    /// </summary>
    private IEnumerator AttackCoroutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(attackInterval);
            if (animator != null)
                animator.SetTrigger("Attack");
            yield return new WaitForSeconds(attackAnimationWait);
            if (enemyShooter != null)
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
        Explosion(maxHp);
        AddKillCount();
        AddScore(maxHp);
        ApearItem(item);
        Destroy(gameObject);
    }

    void Explosion(float maxHp)
    {
        Vector3 pos = gameObject.transform.position;
        if (offsetExplosionY != 0) pos.y += offsetExplosionY;
        
        if(maxHp < 50){
            EffectController.Instance.PlaySmallExplosion(pos, gameObject.transform.rotation);
            return;
        }
        if(maxHp < 100){
            EffectController.Instance.PlayMiddleExplosion(pos, gameObject.transform.rotation);
            return;
        }
        EffectController.Instance.PlayLargeExplosion(pos, gameObject.transform.rotation);
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