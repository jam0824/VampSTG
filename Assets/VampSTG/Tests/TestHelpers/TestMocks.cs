using System.Collections;
using UnityEngine;

namespace VampSTG.Tests.Mocks
{
    /// <summary>
    /// テスト用のモックEnemyShooter（MonoBehaviourを継承しない版）
    /// </summary>
    public class MockEnemyShooter : IEnemyShooter
    {
        public int fireCallCount = 0;
        public float lastDirection = 0f;
        public bool wasDirectionUsed = false;
        
        public void Fire()
        {
            fireCallCount++;
            wasDirectionUsed = false;
            Debug.Log($"MockEnemyShooter.Fire() called. Total calls: {fireCallCount}");
        }
        
        public void Fire(float _direction)
        {
            fireCallCount++;
            lastDirection = _direction;
            wasDirectionUsed = true;
            Debug.Log($"MockEnemyShooter.Fire({_direction}) called. Total calls: {fireCallCount}");
        }
        
        public void ResetCallCount()
        {
            fireCallCount = 0;
            lastDirection = 0f;
            wasDirectionUsed = false;
        }
    }
    
    /// <summary>
    /// テスト用の簡略化されたBaseEnemy
    /// </summary>
    public class TestEnemy : BaseEnemy
    {
        // テスト用フラグ
        public bool hasMovementBeenCalled = false;
        public bool hasStartBeenCalled = false;
        public bool hasDeathBeenCalled = false;
        
        // テスト用パラメータ
        public Vector3 movementDirection = Vector3.forward;
        
        #region BaseEnemyのオーバーライド（テスト環境対応）
        
        protected override void HandleMovement()
        {
            hasMovementBeenCalled = true;
            transform.Translate(movementDirection * Time.deltaTime);
        }
        
        protected override void Start()
        {
            hasStartBeenCalled = true;
            
            // プレイヤー参照を取得
            var playerObj = GameObject.FindWithTag("Core");
            if (playerObj != null)
                playerTransform = playerObj.transform;
                
            maxHp = hp;
            
            // テスト環境対応: GameManager.Instanceのnullチェック
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddStageAllHp(maxHp);
            }
            
            // 攻撃設定の初期化
            if (isAttack)
            {
                enemyShooter = GetComponent<IEnemyShooter>();
                if (Application.isPlaying)
                {
                    StartCoroutine(AttackCoroutine());
                }
                if (animator == null) animator = GetComponent<Animator>();
            }
            
            OnStart();
        }
        
        protected override IEnumerator AttackCoroutine()
        {
            while (!isDead)
            {
                yield return new WaitForSeconds(attackInterval);
                
                // テスト環境対応: GameManager.Instanceのnullチェック
                if (GameManager.Instance != null)
                {
                    // 境界チェック
                    if (IsOutOfBounds()) continue;
                }
                
                PerformAttack();
                yield return new WaitForSeconds(attackAnimationWait);
                yield return new WaitForSeconds(1f);
                isAttackAnimation = false;
            }
        }
        
        protected override void AddKillCount()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.killCount++;
                GameManager.Instance.allKillCount++;
            }
        }
        
        protected override void AddScore(float _maxHp)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(_maxHp);
            }
        }
        
        protected override void ApearItem(GameObject _objItem)
        {
            if (_objItem == null) return;
            
            if (GameManager.Instance != null && IsOutOfBounds()) return;
            
            Vector3 pos = gameObject.transform.position;
            Instantiate(_objItem, pos, gameObject.transform.rotation);
        }
        
        protected override float hit(ConfigPlayerBullet _bullet, float _enemyHp)
        {
            float damage = 0f;
            if (_bullet != null)
            {
                // テスト環境では直接damageフィールドを使用
                damage = _bullet.damage;
            }
            
            _enemyHp -= damage;
            
            // テスト環境対応: SoundManager.Instanceのnullチェック
            if (_bullet != null && _bullet.hitSe != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySE(_bullet.hitSe, _bullet.hitSeVolume);
            }
            
            return _enemyHp;
        }
        
        protected override void Explosion(float _maxHp)
        {
            Vector3 pos = gameObject.transform.position;
            if (offsetExplosionY != 0) pos.y += offsetExplosionY;
            
            // テスト環境対応: EffectController.Instanceのnullチェック
            if (EffectController.Instance != null)
            {
                if (_maxHp < 50)
                    EffectController.Instance.PlaySmallExplosion(pos, gameObject.transform.rotation);
                else if (_maxHp < 100)
                    EffectController.Instance.PlayMiddleExplosion(pos, gameObject.transform.rotation);
                else
                    EffectController.Instance.PlayLargeExplosion(pos, gameObject.transform.rotation);
            }
        }
        
        #endregion
        
        #region テスト用パブリックメソッド
        
        public void TestUpdate()
        {
            if (Application.isPlaying) return; // 実環境では呼ばない
            
            // Updateの処理をテスト用に実行
            if (playerTransform == null) return;
            HandleMovement();
        }
        
        public void TestOnTriggerEnter(Collider _other)
        {
            OnTriggerEnter(_other);
        }
        
        protected override void OnTriggerEnter(Collider _other)
        {
            if (isDead) return;
            
            // テスト環境用のOnTriggerEnterオーバーライド
            if (_other.CompareTag("Boss"))
            {
                hp -= fromBossDamage;
            }
            else if (_other.CompareTag("PlayerBullet"))
            {
                ConfigPlayerBullet config = _other.gameObject.GetComponent<ConfigPlayerBullet>();
                if (config != null)
                {
                    hp = hit(config, hp);
                    
                    // テスト環境では DestroyImmediate を使用
                    if (Application.isEditor && !Application.isPlaying)
                    {
                        DestroyImmediate(_other.gameObject);
                    }
                    else
                    {
                        Destroy(_other.gameObject);
                    }
                }
            }
            else if (_other.CompareTag("BossBullet"))
            {
                hp -= fromBossDamage;
                
                // テスト環境では DestroyImmediate を使用
                if (Application.isEditor && !Application.isPlaying)
                {
                    DestroyImmediate(_other.gameObject);
                }
                else
                {
                    Destroy(_other.gameObject);
                }
            }
            
            if (hp <= 0) enemyDie();
        }
        
        public void TestHit(ConfigPlayerBullet _bullet, float _enemyHp)
        {
            hp = hit(_bullet, _enemyHp);
        }
        
        public void TestEnemyDie()
        {
            hasDeathBeenCalled = true;
            enemyDie();
        }
        
        protected override void enemyDie()
        {
            hasDeathBeenCalled = true;
            
            if (GameManager.Instance != null)
            {
                AddKillCount();
                AddScore(maxHp);
            }
            
            Explosion(maxHp);
            
            if (item != null)
            {
                ApearItem(item);
            }
            
            // テスト環境では DestroyImmediate を使用
            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void TestExplosion(float _maxHp)
        {
            Explosion(_maxHp);
        }
        
        public void TestAddKillCount()
        {
            AddKillCount();
        }
        
        public void TestAddScore(float _maxHp)
        {
            AddScore(_maxHp);
        }
        
        public void SetPlayerTransformForTest(Transform _playerTransform)
        {
            this.playerTransform = _playerTransform;
        }
        
        /// <summary>
        /// テスト環境用の初期化メソッド
        /// </summary>
        public void InitializeForTest()
        {
            if (!hasStartBeenCalled)
            {
                Debug.Log("InitializeForTest: Calling Start()");
                Start();
            }
        }
        
        public void SetEnemyShooterForTest(IEnemyShooter _shooter)
        {
            Debug.Log($"SetEnemyShooterForTest called with: {_shooter?.GetType().Name ?? "null"}");
            this.enemyShooter = _shooter;
            Debug.Log($"After setting: enemyShooter = {this.enemyShooter?.GetType().Name ?? "null"}");
            Debug.Log($"HasEnemyShooter = {HasEnemyShooter}");
        }
        
        public void PerformSingleAttackForTest()
        {
            PerformAttack();
        }
        
        public IEnumerator GetAttackCoroutineForTest()
        {
            return AttackCoroutineForTest();
        }
        
        public void StartManualCoroutineForTest()
        {
            Debug.Log("StartManualCoroutineForTest called");
            var coroutine = AttackCoroutineForTest();
            StartCoroutine(coroutine);
            Debug.Log("Manual coroutine started");
        }
        
        public void ExecuteAttackSequenceForTest()
        {
            Debug.Log("ExecuteAttackSequenceForTest called - simulating attack sequence without coroutine");
            // コルーチンを使わずに攻撃シーケンスを実行
            PerformAttack();
        }
        
        public void ExecuteImmediateAttackForTest()
        {
            Debug.Log("ExecuteImmediateAttackForTest called");
            Debug.Log($"enemyShooter null check: {enemyShooter == null}");
            PerformAttack();
            Debug.Log("ExecuteImmediateAttackForTest completed");
        }
        
        public void StartAttackCoroutineForTest()
        {
            // テスト環境では常にコルーチンを開始
            Debug.Log("StartAttackCoroutineForTest called");
            StartCoroutine(AttackCoroutineForTest());
            Debug.Log("StartCoroutine(AttackCoroutineForTest) executed");
        }
        
        public void StartSimpleAttackCoroutineForTest()
        {
            // テスト環境用の簡単な攻撃コルーチン
            Debug.Log("StartSimpleAttackCoroutineForTest called");
            StartCoroutine(SimpleAttackCoroutineForTest());
            Debug.Log("StartCoroutine(SimpleAttackCoroutineForTest) executed");
        }
        
        #endregion
        
        #region テスト用プロパティ
        
        public bool HasPlayerTransform => playerTransform != null;
        public bool HasEnemyShooter => enemyShooter != null;
        
        public int FromBossDamage
        {
            get => fromBossDamage;
            set => fromBossDamage = value;
        }
        
        public bool IsAttack
        {
            get => isAttack;
            set => isAttack = value;
        }
        
        public float AttackInterval
        {
            get => attackInterval;
            set => attackInterval = value;
        }
        
        public float AttackAnimationWait
        {
            get => attackAnimationWait;
            set => attackAnimationWait = value;
        }
        
        public bool IsDirectionAttack
        {
            get => isDirectionAttack;
            set => isDirectionAttack = value;
        }
        
        public float AttackDirection
        {
            get => attackDirection;
            set => attackDirection = value;
        }
        
        #endregion
        
        #region プライベートヘルパーメソッド
        
        private bool IsOutOfBounds()
        {
            if (GameManager.Instance == null) return false;
            
            return (GameManager.Instance.minZ > transform.position.z) || 
                   (GameManager.Instance.maxZ < transform.position.z) ||
                   (GameManager.Instance.minY > transform.position.y) || 
                   (GameManager.Instance.maxY < transform.position.y);
        }
        
        private void PerformAttack()
        {
            Debug.Log("TestEnemy.PerformAttack() called");
            
            if (animator != null)
                animator.SetTrigger("attack");
                
            isAttackAnimation = true;
            
            if (enemyShooter != null)
            {
                Debug.Log("TestEnemy: enemyShooter found, firing!");
                if (isDirectionAttack)
                    enemyShooter.Fire(attackDirection);
                else
                    enemyShooter.Fire();
            }
            else
            {
                Debug.Log("TestEnemy: enemyShooter is null!");
            }
        }
        
        private IEnumerator AttackCoroutineForTest()
        {
            Debug.Log($"AttackCoroutineForTest started");
            
            // 即座に攻撃を実行（テスト環境用）
            Debug.Log("AttackCoroutineForTest: About to call PerformAttack immediately");
            PerformAttack();
            Debug.Log("AttackCoroutineForTest: PerformAttack called");
            
            // 1フレーム待機して完了
            yield return null;
            Debug.Log("AttackCoroutineForTest completed");
        }
        
        private IEnumerator SimpleAttackCoroutineForTest()
        {
            // より単純なテスト用攻撃コルーチン
            Debug.Log("SimpleAttackCoroutineForTest started");
            Debug.Log("SimpleAttackCoroutineForTest: About to call PerformAttack immediately");
            PerformAttack();
            Debug.Log("SimpleAttackCoroutineForTest: PerformAttack called");
            yield return null; // 1フレーム待機
            Debug.Log("SimpleAttackCoroutineForTest completed");
        }
        
        #endregion
    }
    
    /// <summary>
    /// テスト敵セットアップの結果
    /// </summary>
    public class TestEnemySetupResult
    {
        public GameObject enemyObject;
        public TestEnemy enemy;
        public BoxCollider collider;
    }
} 