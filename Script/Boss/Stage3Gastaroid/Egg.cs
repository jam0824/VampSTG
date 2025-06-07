using UnityEngine;
using System.Collections;

public class Egg : MonoBehaviour, IEnemy
{
    #region 設定パラメータ
    [Header("生まれる敵")]
    [SerializeField] private GameObject enemyPrefab;
    
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 0.1f;
    [SerializeField] private float groundY = -4f;
    [SerializeField] private float hachingDelay = 3f;
    
    [Header("ステータス")]
    [SerializeField] private float hp = 10f;
    
    [Header("エフェクト・アイテム")]
    [SerializeField] private float offsetExplosionY = 0f;
    [SerializeField] public GameObject item{get; set;} = null;
    #endregion

    #region 状態管理
    public bool isGround = false;
    private bool isHatched = false;
    private bool isDead = false;
    private float maxHp;
    private Animator animator;
    
    // BGスクロールスピード取得用
    private IScrollSpeed scrollSpeedProvider;
    private StageManager stageManager;
    #endregion

    #region 定数定義
    private const float ENEMY_SPAWN_HEIGHT_OFFSET = 0.1f;
    private const float HATCHING_ANIMATION_WAIT = 0.5f;
    private const float DESTRUCTION_DELAY = 1f;
    private const float SMALL_EXPLOSION_THRESHOLD = 50f;
    private const float MIDDLE_EXPLOSION_THRESHOLD = 100f;
    #endregion

    #region 初期化・基本更新
    void Start()
    {
        InitializeEgg();
        StartCoroutine(ExecuteHatchingSequence());
    }

    void Update()
    {
        if (!isDead)
        {
            // BGスクロールスピードに合わせて移動
            MoveWithScroll();
            
            if (!isGround)
            {
                PerformFallMovement();
            }
        }
    }

    /// <summary>
    /// 卵の初期化
    /// </summary>
    private void InitializeEgg()
    {
        animator = GetComponent<Animator>();
        maxHp = hp;
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
    }
    #endregion

    #region 移動システム
    /// <summary>
    /// 落下移動を実行
    /// </summary>
    private void PerformFallMovement()
    {
        Vector3 currentPosition = transform.position;
        
        if (HasReachedGround(currentPosition.y))
        {
            HandleGroundLanding();
        }
        else
        {
            ExecuteFallMovement();
        }
    }

    /// <summary>
    /// 地面に到達したかチェック
    /// </summary>
    private bool HasReachedGround(float currentY)
    {
        return currentY <= groundY;
    }

    /// <summary>
    /// 落下移動の実行
    /// </summary>
    private void ExecuteFallMovement()
    {
        Vector3 position = transform.position;
        position.y -= moveSpeed * Time.deltaTime;
        transform.position = position;
    }

    /// <summary>
    /// 地面着地処理
    /// </summary>
    private void HandleGroundLanding()
    {
        if (!isGround)
        {
            isGround = true;
            
        }
    }
    #endregion

    #region 孵化システム
    /// <summary>
    /// 孵化シーケンスの実行
    /// </summary>
    private IEnumerator ExecuteHatchingSequence()
    {
        yield return new WaitForSeconds(hachingDelay);
        
        if (!isHatched && !isDead && animator != null)
        {
            yield return StartCoroutine(PerformHatchingProcess());
        }
        
        yield return new WaitForSeconds(DESTRUCTION_DELAY);
        DestroyEgg();
    }

    /// <summary>
    /// 孵化プロセスの実行
    /// </summary>
    private IEnumerator PerformHatchingProcess()
    {
        isHatched = true;
        
        // 孵化アニメーション開始
        animator.SetTrigger("hatching");
        
        // 物理設定の変更
        ConfigureEggPhysicsForHatching();
        
        // アニメーション待機
        yield return new WaitForSeconds(HATCHING_ANIMATION_WAIT);
        
        // 敵の生成
        SpawnEnemy();
    }

    /// <summary>
    /// 孵化時の物理設定変更
    /// </summary>
    private void ConfigureEggPhysicsForHatching()
    {
        SetColliderToTrigger();
        FreezeEggMovement();
    }

    /// <summary>
    /// ColliderをTriggerに変更
    /// </summary>
    private void SetColliderToTrigger()
    {
        Collider eggCollider = GetComponent<Collider>();
        if (eggCollider != null)
        {
            eggCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// 卵の移動を固定
    /// </summary>
    private void FreezeEggMovement()
    {
        Rigidbody eggRigidbody = GetComponent<Rigidbody>();
        if (eggRigidbody != null)
        {
            eggRigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }
    }

    /// <summary>
    /// 敵の生成
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyPrefab != null)
        {
            Vector3 spawnPosition = CalculateEnemySpawnPosition();
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }

    /// <summary>
    /// 敵の生成位置を計算
    /// </summary>
    private Vector3 CalculateEnemySpawnPosition()
    {
        Vector3 spawnPosition = transform.position;
        spawnPosition.y += ENEMY_SPAWN_HEIGHT_OFFSET;
        return spawnPosition;
    }
    #endregion

    #region 衝突判定システム
    /// <summary>
    /// Trigger衝突判定
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        HandleCollisionWithObject(other.gameObject, other);
    }

    /// <summary>
    /// Collision衝突判定
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        HandleCollisionWithObject(collision.gameObject, collision.collider);
    }

    /// <summary>
    /// オブジェクトとの衝突処理
    /// </summary>
    private void HandleCollisionWithObject(GameObject collisionObject, Collider collider)
    {
        if (collisionObject.CompareTag("Ground"))
        {
            HandleGroundCollision();
        }
        else if (collisionObject.CompareTag("PlayerBullet"))
        {
            HandlePlayerBulletCollision(collider);
        }
    }

    /// <summary>
    /// 地面衝突処理
    /// </summary>
    private void HandleGroundCollision()
    {
        HandleGroundLanding();
    }

    /// <summary>
    /// プレイヤー弾衝突処理
    /// </summary>
    private void HandlePlayerBulletCollision(Collider bulletCollider)
    {
        if (!bulletCollider.TryGetComponent<ConfigPlayerBullet>(out var bullet)) return;
        
        ProcessBulletDamage(bullet, bulletCollider);
        
        if (hp <= 0)
        {
            ExecuteDeathSequence();
        }
    }

    /// <summary>
    /// 弾のダメージ処理
    /// </summary>
    private void ProcessBulletDamage(ConfigPlayerBullet bullet, Collider bulletCollider)
    {
        hp = CalculateDamageResult(bullet, hp);
        CreateHitEffect(bullet, bulletCollider);
        
        if (bullet.isDestroy)
        {
            Destroy(bulletCollider.gameObject);
        }
    }

    /// <summary>
    /// ヒットエフェクトの生成
    /// </summary>
    private void CreateHitEffect(ConfigPlayerBullet bullet, Collider bulletCollider)
    {
        Vector3 hitPoint = bulletCollider.ClosestPoint(transform.position);
        
        if (bullet.triggerEffect != null)
        {
            Instantiate(bullet.triggerEffect, hitPoint, bulletCollider.transform.rotation);
        }
    }
    #endregion

    #region ダメージ・死亡システム
    /// <summary>
    /// ダメージ計算結果を取得
    /// </summary>
    private float CalculateDamageResult(ConfigPlayerBullet bullet, float currentHp)
    {
        float damage = bullet.getDamage();
        
        PlayHitSound(bullet);
        
        return currentHp - damage;
    }

    /// <summary>
    /// ヒット音の再生
    /// </summary>
    private void PlayHitSound(ConfigPlayerBullet bullet)
    {
        if (bullet.hitSe != null)
        {
            SoundManager.Instance.PlaySE(bullet.hitSe, bullet.hitSeVolume);
        }
    }

    /// <summary>
    /// 死亡シーケンスの実行
    /// </summary>
    private void ExecuteDeathSequence()
    {
        isDead = true;
        CreateExplosionEffect();
        AddGameProgress();
        SpawnDropItem();
        DestroyEgg();
    }

    /// <summary>
    /// 爆発エフェクトの生成
    /// </summary>
    private void CreateExplosionEffect()
    {
        Vector3 explosionPosition = CalculateExplosionPosition();
        
        if (maxHp < SMALL_EXPLOSION_THRESHOLD)
        {
            EffectController.Instance.PlaySmallExplosion(explosionPosition, transform.rotation);
        }
        else if (maxHp < MIDDLE_EXPLOSION_THRESHOLD)
        {
            EffectController.Instance.PlayMiddleExplosion(explosionPosition, transform.rotation);
        }
        else
        {
            EffectController.Instance.PlayLargeExplosion(explosionPosition, transform.rotation);
        }
    }

    /// <summary>
    /// 爆発位置の計算
    /// </summary>
    private Vector3 CalculateExplosionPosition()
    {
        Vector3 position = transform.position;
        if (offsetExplosionY != 0)
        {
            position.y += offsetExplosionY;
        }
        return position;
    }

    /// <summary>
    /// ゲーム進行状況の更新
    /// </summary>
    private void AddGameProgress()
    {
        AddKillCount();
        AddScore(maxHp);
    }

    /// <summary>
    /// キル数の加算
    /// </summary>
    private void AddKillCount()
    {
        GameManager.Instance.killCount++;
        GameManager.Instance.allKillCount++;
    }

    /// <summary>
    /// スコアの加算
    /// </summary>
    private void AddScore(float score)
    {
        GameManager.Instance.AddScore(score);
    }

    /// <summary>
    /// ドロップアイテムの生成
    /// </summary>
    private void SpawnDropItem()
    {
        if (item == null) return;
        if (!IsWithinCameraBounds()) return;
        
        Vector3 itemPosition = transform.position;
        Instantiate(item, itemPosition, transform.rotation);
        Debug.Log("アイテム出現");
    }

    /// <summary>
    /// カメラ範囲内かチェック
    /// </summary>
    private bool IsWithinCameraBounds()
    {
        float currentZ = transform.position.z;
        return currentZ >= GameManager.Instance.minZ && currentZ <= GameManager.Instance.maxZ;
    }

    /// <summary>
    /// 卵オブジェクトの削除
    /// </summary>
    private void DestroyEgg()
    {
        Destroy(gameObject);
    }
    #endregion

    #region BGスクロールシステム

    
    /// <summary>
    /// BGのスクロールスピードに合わせてz軸方向に移動
    /// </summary>
    private void MoveWithScroll()
    {
        // z軸方向のみ移動
        Vector3 scrollMovement = new Vector3(0f, 0f, -stageManager.scrollSpeed * Time.deltaTime);
        transform.Translate(scrollMovement, Space.World);
    }
    #endregion
}
