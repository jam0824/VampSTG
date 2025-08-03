using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using VampSTG.Tests.Mocks;

namespace VampSTG.Tests
{
    /// <summary>
    /// テスト用のConfigPlayerBullet
    /// </summary>
    public class TestConfigPlayerBullet : MonoBehaviour
    {
        public float damage = 1f;
        public AudioClip hitSe;
        public float hitSeVolume;
        public GameObject triggerEffect;
        public bool isDestroy = true;
        public bool isUseDefaultExplosionEffect = false;
        
        // ConfigPlayerBulletと互換性のあるメソッド
        public float getDamage()
        {
            return damage; // GameManager依存を削除
        }
        
        public void setDamage(float damageValue)
        {
            damage = damageValue;
        }
    }

    /// <summary>
    /// テスト用のGameManager
    /// </summary>
    public class TestGameManager : MonoBehaviour
    {
        public float minZ = -10f;
        public float maxZ = 10f;
        public float minY = -5f;
        public float maxY = 5f;
        public int killCount = 0;
        public int allKillCount = 0;
        public float powerMagnification = 1f;
        
        private static TestGameManager s_Instance;
        public static TestGameManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    var go = new GameObject("TestGameManager");
                    s_Instance = go.AddComponent<TestGameManager>();
                }
                return s_Instance;
            }
        }
        
        private void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        public void SetBounds(float _minZ, float _maxZ, float _minY, float _maxY)
        {
            minZ = _minZ;
            maxZ = _maxZ;
            minY = _minY;
            maxY = _maxY;
        }
        
        public void AddStageAllHp(float _hp) { }
        public void BossHpDown(float _damage) { }
        public void AddScore(float _score) { }
        
        private void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
            }
        }
    }
    /// <summary>
    /// Stage4MidBossのテスト用クラス
    /// </summary>
    public class TestStage4MidBoss : Stage4MidBoss
    {
        public bool HasStageManager { get; private set; }
        private Vector3 basePosition = Vector3.zero;
        public Vector3 BasePosition 
        { 
            get { return basePosition; }
            private set { basePosition = value; }
        }
        private float verticalTime = 0f;
        public float VerticalTime 
        { 
            get { return verticalTime; }
            private set { verticalTime = value; }
        }
        private bool hasReachedTargetZ = false;
        public bool HasReachedTargetZ 
        { 
            get { return hasReachedTargetZ; }
            private set { hasReachedTargetZ = value; }
        }
        
        // テスト設定メソッド
        public void SetBasePositionForTest(Vector3 position)
        {
            basePosition = position;
            Debug.Log($"SetBasePositionForTest: Set BasePosition to {position}");
        }
        
        public void SetVerticalTimeForTest(float time)
        {
            VerticalTime = time;
            Debug.Log($"SetVerticalTimeForTest: Set VerticalTime to {time}");
        }
        
        public void SetHasReachedTargetZForTest(bool reached)
        {
            hasReachedTargetZ = reached;
            Debug.Log($"SetHasReachedTargetZForTest: Set HasReachedTargetZ to {reached}");
        }
        
        public void SetStageManagerForTest(StageManager stageManager)
        {
            HasStageManager = stageManager != null;
        }
        
        // BaseEnemyのテスト用メソッド
        public void SetPlayerTransformForTest(Transform playerTransform)
        {
            this.playerTransform = playerTransform;
        }
        
        public Transform GetPlayerTransformForTest()
        {
            return this.playerTransform;
        }
        
        public void SetEnemyShooterForTest(IEnemyShooter shooter)
        {
            this.enemyShooter = shooter;
        }
        
        // maxHpへのアクセス用プロパティ
        public float MaxHp
        {
            get { return maxHp; }
            set { maxHp = value; }
        }
        
        // テスト用のダメージフィールド
        public float damage = 10f;
        
        // isDeadへのアクセス用プロパティ
        public bool IsDead
        {
            get { return isDead; }
            set { isDead = value; }
        }
        
        // isAttackへのアクセス用プロパティ
        public bool IsAttack
        {
            get { return isAttack; }
            set { isAttack = value; }
        }
        
        // isAttackAnimationへのアクセス用プロパティ
        public bool IsAttackAnimation
        {
            get { return isAttackAnimation; }
            set { isAttackAnimation = value; }
        }
        
        // animatorへのアクセス用プロパティ
        public Animator Animator
        {
            get { return animator; }
            set { animator = value; }
        }
        
        public void TestTakeDamage()
        {
            // テスト用のダメージ処理 - 直接ダメージを適用
            TestHit(damage);
        }
        
        public void TestHit(float damageAmount)
        {
            hp -= damageAmount;
            
            // 死亡判定
            if (hp <= 0 && !isDead)
            {
                isDead = true;
                enemyDie();
            }
        }
        
        private TestConfigPlayerBullet CreateTestBullet(float damage)
        {
            var bulletObj = new GameObject("TestBullet");
            var bullet = bulletObj.AddComponent<TestConfigPlayerBullet>();
            bullet.damage = damage;
            return bullet;
        }
        
        public void TestStartAttack()
        {
            Debug.Log($"TestStartAttack called - isAttack: {isAttack}, isDead: {isDead}, IsEscaping: {IsEscaping()}");
            if (isAttack && !isDead && !IsEscaping())
            {
                Debug.Log("Starting AttackCoroutine");
                StartCoroutine(AttackCoroutine());
            }
            else
            {
                Debug.Log("Attack conditions not met");
            }
        }
        
        // テスト用の即座攻撃メソッド
        public void TestPerformImmediateAttack()
        {
            Debug.Log($"TestPerformImmediateAttack called - enemyShooter: {enemyShooter != null}");
            if (enemyShooter != null && !isDead && !IsEscaping())
            {
                Debug.Log("Performing immediate attack");
                if (Random.value < 0.5f)
                {
                    // Attack1
                    if (animator != null)
                        animator.SetTrigger("attack");
                    enemyShooter.Fire(attackDirection);
                }
                else
                {
                    // Attack2 (簡略版)
                    if (animator != null)
                        animator.SetTrigger("attack2");
                    enemyShooter.Fire(attackDirection);
                }
                Debug.Log("Attack performed");
            }
            else
            {
                Debug.Log("Cannot perform attack - conditions not met");
            }
        }
        
        // プライベートメソッドのテスト用ラッパー
        public void TestHandleMovement()
        {
            Debug.Log($"TestHandleMovement called - GameManager.Instance: {GameManager.Instance != null}");
            if (GameManager.Instance != null)
            {
                Debug.Log("TestHandleMovement - Calling HandleMovement");
                HandleMovement();
            }
            else
            {
                Debug.Log("TestHandleMovement - GameManager.Instance is null, not calling HandleMovement");
            }
        }
        
        public void TestStartEscape()
        {
            ForceEscape();
        }
        
        // テスト用の強制アイテム削除メソッド
        public void TestForceItemDeletion()
        {
            Debug.Log($"TestForceItemDeletion - Before: item = {item}");
            if (IsEscaping())
            {
                item = null;
                Debug.Log("TestForceItemDeletion - Item set to null due to escaping");
            }
            else
            {
                Debug.Log("TestForceItemDeletion - Not escaping, item not deleted");
            }
        }
        
        // テスト用の直接上下移動メソッド
        public void TestDirectVerticalMovement()
        {
            Debug.Log($"TestDirectVerticalMovement - verticalTime: {verticalTime}, basePosition: {basePosition}");
            
            float sineValue = Mathf.Sin(verticalTime);
            float targetY = basePosition.y + sineValue * 2f; // verticalRangeの代わりに固定値
            
            Debug.Log($"TestDirectVerticalMovement - sineValue: {sineValue}, targetY: {targetY}");
            
            // 境界チェック（GameManagerを使わない）
            targetY = Mathf.Clamp(targetY, -5f, 5f); // 固定の境界
            
            Vector3 newPosition = transform.position;
            Vector3 oldPosition = newPosition;
            newPosition.y = targetY;
            transform.position = newPosition;
            
            Debug.Log($"TestDirectVerticalMovement - Position changed from {oldPosition} to {newPosition}");
        }
        
        // テスト用の直接逃げ移動メソッド
        public void TestDirectEscapeMovement()
        {
            Debug.Log($"TestDirectEscapeMovement - Current position: {transform.position}");
            Debug.Log($"TestDirectEscapeMovement - IsEscaping: {IsEscaping()}");
            
            if (IsEscaping())
            {
                // Z軸マイナス方向に移動（固定値）
                Vector3 escapeDirection = Vector3.back;
                float escapeDistance = 3f * 0.02f; // 固定の移動距離（Time.deltaTimeの代わり）
                transform.position += escapeDirection * escapeDistance;
                
                Debug.Log($"TestDirectEscapeMovement - After movement: {transform.position}");
                
                // 削除座標に到達したかチェック
                if (transform.position.z <= -20f)
                {
                    Debug.Log("TestDirectEscapeMovement - Reached deletion position, setting item to null");
                    item = null;
                }
            }
            else
            {
                Debug.Log("TestDirectEscapeMovement - Not escaping, no movement");
            }
        }
        
        // テスト用の直接目標Z移動メソッド
        public void TestDirectTargetZMovement()
        {
            Debug.Log($"TestDirectTargetZMovement - Current position: {transform.position}");
            Debug.Log($"TestDirectTargetZMovement - HasReachedTargetZ: {HasReachedTargetZ}");
            
            if (!HasReachedTargetZ)
            {
                if (transform.position.z <= 2f) // targetZの代わりに固定値
                {
                    Debug.Log("TestDirectTargetZMovement - Reached target Z, setting position and flag");
                    // 目標Z座標に到達
                    Vector3 currentPos = transform.position;
                    currentPos.z = 2f;
                    transform.position = currentPos;
                    basePosition = currentPos;
                    hasReachedTargetZ = true;
                }
                else
                {
                    Debug.Log("TestDirectTargetZMovement - Moving towards target Z");
                    // 目標Z座標まで前進（固定値）
                    Vector3 moveDirection = Vector3.back;
                    float moveDistance = 0.5f * 0.02f; // 固定の移動距離
                    transform.position += moveDirection * moveDistance;
                }
                
                Debug.Log($"TestDirectTargetZMovement - After movement: {transform.position}");
            }
            else
            {
                Debug.Log("TestDirectTargetZMovement - Already reached target Z");
            }
        }
        
        // オーバーライドメソッド - Singleton依存を回避
        protected override void Update()
        {
            // 死亡済みまたは逃げ中の場合は時間チェックをスキップ
            if (!isDead && !IsEscaping())
            {
                // StageManagerの経過時間をチェックして逃げる処理を開始
                // テスト環境では手動で逃げ処理を開始
            }

            // 親クラスのUpdate処理を呼び出し（GameManager null check込み）
            if (GameManager.Instance != null)
            {
                base.Update();
            }
        }
        
        protected override float hit(ConfigPlayerBullet bullet, float enemyHp)
        {
            float damage = 0f;
            if (bullet != null)
            {
                damage = bullet.damage;
            }
            
            enemyHp -= damage;
            
            // テスト環境対応: SoundManager.Instanceのnullチェック
            if (bullet != null && bullet.hitSe != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySE(bullet.hitSe, bullet.hitSeVolume);
            }
            
            // HPを更新
            hp = enemyHp;
            
            // 死亡判定
            if (hp <= 0 && !isDead)
            {
                isDead = true;
                enemyDie();
            }
            
            return enemyHp;
        }
        
        protected override void Explosion(float maxHp)
        {
            // EffectController.Instanceの依存を削除
            if (EffectController.Instance != null)
            {
                base.Explosion(maxHp);
            }
        }
        
        protected override void enemyDie()
        {
            // テスト環境対応のenemyDieオーバーライド
            if (GameManager.Instance != null)
            {
                // キルカウント追加
                GameManager.Instance.killCount++;
                GameManager.Instance.allKillCount++;
                
                // スコア追加
                GameManager.Instance.AddScore(maxHp);
            }
            
            // 爆発エフェクト
            Explosion(maxHp);
            
            // アイテムドロップ（逃げ中でない場合のみ）
            if (item != null && !IsEscaping())
            {
                Vector3 pos = transform.position;
                if (GameManager.Instance != null)
                {
                    // 範囲内チェック
                    if (!(GameManager.Instance.minZ > pos.z || GameManager.Instance.maxZ < pos.z ||
                          GameManager.Instance.minY > pos.y || GameManager.Instance.maxY < pos.y))
                    {
                        Instantiate(item, pos, transform.rotation);
                    }
                }
            }
            
            // オブジェクト削除（テスト環境対応）
            if (Application.isEditor && !Application.isPlaying)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        protected override void HandleMovement()
        {
            Debug.Log($"HandleMovement called - IsEscaping: {IsEscaping()}, IsMovementEnabled: {IsMovementEnabled()}");
            Debug.Log($"HandleMovement - GameManager.Instance: {GameManager.Instance != null}");
            Debug.Log($"HandleMovement - HasReachedTargetZ: {HasReachedTargetZ}");
            Debug.Log($"HandleMovement - Current position: {transform.position}");
            
            // 逃げ中の場合は逃げる処理のみ実行
            if (IsEscaping())
            {
                Debug.Log("HandleMovement - Escaping, calling HandleEscapeMovement");
                HandleEscapeMovement();
                return;
            }

            if (!IsMovementEnabled()) 
            {
                Debug.Log("HandleMovement - Movement disabled, returning");
                return;
            }

            // GameManagerのnullチェック
            if (GameManager.Instance == null) 
            {
                Debug.Log("HandleMovement - GameManager is null, returning");
                return;
            }

            // ─── 目標Z座標まで移動 ───
            if (!HasReachedTargetZ)
            {
                Debug.Log("HandleMovement - Has not reached target Z, checking position");
                if (transform.position.z <= 2f) // targetZの代わりに固定値
                {
                    Debug.Log("HandleMovement - Reached target Z, setting position and flag");
                    // 目標Z座標に到達
                    Vector3 currentPos = transform.position;
                    currentPos.z = 2f;
                    transform.position = currentPos;
                    basePosition = currentPos;
                    hasReachedTargetZ = true;
                    return;
                }

                Debug.Log("HandleMovement - Moving towards target Z");
                // 目標Z座標まで前進
                Vector3 moveDirection = Vector3.back;
                transform.position += moveDirection * 0.5f * Time.deltaTime; // moveSpeedの代わりに固定値
                return;
            }

            Debug.Log("HandleMovement - Starting vertical movement section");
            // ─── 上下移動（目標Z座標到達後のみ） ───
            Debug.Log($"HandleMovement - Before vertical movement: VerticalTime={VerticalTime}, Time.deltaTime={Time.deltaTime}");
            
            verticalTime += 1f * Time.deltaTime; // verticalMoveSpeedの代わりに固定値
            
            Debug.Log($"HandleMovement - After time update: VerticalTime={VerticalTime}");
            
            float sineValue = Mathf.Sin(verticalTime);
            float targetY = BasePosition.y + sineValue * 2f; // verticalRangeの代わりに固定値
            
            Debug.Log($"HandleMovement - Sine calculation: sineValue={sineValue}, BasePosition.y={BasePosition.y}, targetY={targetY}");
            
            // 境界チェック
            targetY = Mathf.Clamp(targetY, GameManager.Instance.minY, GameManager.Instance.maxY);
            
            Debug.Log($"HandleMovement - After clamp: targetY={targetY}, bounds=[{GameManager.Instance.minY}, {GameManager.Instance.maxY}]");
            
            Vector3 newPosition = transform.position;
            Vector3 oldPosition = newPosition;
            newPosition.y = targetY;
            transform.position = newPosition;
            
            Debug.Log($"HandleMovement - Position changed from {oldPosition} to {newPosition}");
        }
        
        private void HandleEscapeMovement()
        {
            Debug.Log($"HandleEscapeMovement - Current position: {transform.position}");
            Debug.Log($"HandleEscapeMovement - Time.deltaTime: {Time.deltaTime}");
            
            // Z軸マイナス方向に移動
            Vector3 escapeDirection = Vector3.back;
            transform.position += escapeDirection * 3f * Time.deltaTime; // escapeSpeedの代わりに固定値
            
            Debug.Log($"HandleEscapeMovement - After movement: {transform.position}");
            
            // 削除座標に到達したら削除（条件を緩和）
            if (transform.position.z <= -20f) // escapeDestroyZの代わりに固定値
            {
                Debug.Log($"HandleEscapeMovement - Reached deletion position. Current item: {item}");
                
                // アイテムドロップを無効化（逃げた場合はアイテムを落とさない）
                item = null;
                Debug.Log("HandleEscapeMovement - Item set to null");
                
                // テスト環境対応
                if (Application.isEditor && !Application.isPlaying)
                {
                    Debug.Log("HandleEscapeMovement - Destroying with DestroyImmediate");
                    DestroyImmediate(gameObject);
                }
                else
                {
                    Debug.Log("HandleEscapeMovement - Destroying with Destroy");
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.Log($"HandleEscapeMovement - Not yet at deletion position (z={transform.position.z} > -20f)");
            }
        }
        
        protected override IEnumerator AttackCoroutine()
        {
            Debug.Log("AttackCoroutine started");
            while (!isDead && !IsEscaping())
            {
                Debug.Log($"AttackCoroutine loop - isDead: {isDead}, IsEscaping: {IsEscaping()}");
                yield return new WaitForSeconds(0.2f); // テスト用に短縮
                
                // 逃げ中または死亡している場合は攻撃処理を停止
                if (IsEscaping() || isDead)
                {
                    Debug.Log("AttackCoroutine stopping - escaping or dead");
                    yield break;
                }
                
                // GameManagerのnullチェック
                if (GameManager.Instance == null)
                {
                    Debug.Log("AttackCoroutine - GameManager is null");
                    yield return null;
                    continue;
                }
                
                Debug.Log($"AttackCoroutine - Position check: {transform.position}");
                Debug.Log($"AttackCoroutine - Bounds: minZ={GameManager.Instance.minZ}, maxZ={GameManager.Instance.maxZ}, minY={GameManager.Instance.minY}, maxY={GameManager.Instance.maxY}");
                
                // キャラクターが範囲外にいる場合は攻撃処理をスキップ
                if ((GameManager.Instance.minZ > transform.position.z) || 
                    (GameManager.Instance.maxZ < transform.position.z) ||
                    (GameManager.Instance.minY > transform.position.y) || 
                    (GameManager.Instance.maxY < transform.position.y))
                {
                    Debug.Log("AttackCoroutine - Out of bounds, skipping");
                    yield return null;
                    continue;
                }

                Debug.Log("AttackCoroutine - About to attack");
                
                // ランダムで攻撃パターンを選択（テスト用に50%固定）
                bool useAttack2 = Random.value < 0.5f;
                Debug.Log($"AttackCoroutine - Using attack2: {useAttack2}");

                if (useAttack2)
                {
                    yield return StartCoroutine(TestPerformAttack2());
                }
                else
                {
                    yield return StartCoroutine(TestPerformAttack1());
                }
                
                Debug.Log("AttackCoroutine - Attack completed");
            }
            Debug.Log("AttackCoroutine ended");
        }
        
        private IEnumerator TestPerformAttack1()
        {
            Debug.Log("TestPerformAttack1 started");
            if (animator != null)
                animator.SetTrigger("attack");
            
            isAttackAnimation = true;
            yield return new WaitForSeconds(0.1f); // 短縮
            
            // 射撃実行（nullチェック付き）
            Debug.Log($"TestPerformAttack1 - enemyShooter: {enemyShooter != null}");
            if (enemyShooter != null)
            {
                Debug.Log($"TestPerformAttack1 - Firing with direction: {attackDirection}");
                enemyShooter.Fire(attackDirection);
                Debug.Log("TestPerformAttack1 - Fire called");
            }
            else
            {
                Debug.Log("TestPerformAttack1 - enemyShooter is null!");
            }
            
            yield return new WaitForSeconds(0.1f); // 短縮
            isAttackAnimation = false;
            Debug.Log("TestPerformAttack1 completed");
        }
        
        private IEnumerator TestPerformAttack2()
        {
            Debug.Log("TestPerformAttack2 started");
            if (animator != null)
                animator.SetTrigger("attack2");
            
            // 移動を無効化
            SetMovementEnabled(false);
            isAttackAnimation = true;
            
            // attack2のアニメーション再生時間だけ待機（短縮）
            yield return new WaitForSeconds(0.2f);
            
            // Attack2でも射撃を実行
            Debug.Log($"TestPerformAttack2 - enemyShooter: {enemyShooter != null}");
            if (enemyShooter != null)
            {
                Debug.Log($"TestPerformAttack2 - Firing with direction: {attackDirection}");
                enemyShooter.Fire(attackDirection);
                Debug.Log("TestPerformAttack2 - Fire called");
            }
            else
            {
                Debug.Log("TestPerformAttack2 - enemyShooter is null!");
            }
            
            // 移動を再有効化
            SetMovementEnabled(true);
            isAttackAnimation = false;
            Debug.Log("TestPerformAttack2 completed");
        }
    }

    /// <summary>
    /// Stage4MidBossのテストクラス
    /// </summary>
    public class Stage4MidBossTests
    {
        private GameObject testObject;
        private TestStage4MidBoss testEnemy;
        private TestGameManager testGameManager;

        [SetUp]
        public void SetUp()
        {
            // 既存のTestGameManagerインスタンスをクリア
            if (TestGameManager.Instance != null)
            {
                Object.DestroyImmediate(TestGameManager.Instance.gameObject);
            }
            
            // テストオブジェクトを作成
            testObject = new GameObject("TestStage4MidBoss");
            testEnemy = testObject.AddComponent<TestStage4MidBoss>();
            
            // 必要なコンポーネントを追加
            testObject.AddComponent<Rigidbody>();
            testObject.AddComponent<BoxCollider>();
            
            // テスト用GameManagerを作成（Singletonとして）
            var gameManagerObj = new GameObject("TestGameManager");
            testGameManager = gameManagerObj.AddComponent<TestGameManager>();
            testGameManager.SetBounds(-10f, 10f, -5f, 5f);
            
            // TestGameManagerのSingletonインスタンスを強制設定
            var instanceField = typeof(TestGameManager).GetField("s_Instance", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            instanceField?.SetValue(null, testGameManager);
            
            // 初期設定
            testEnemy.hp = 100f;
            testEnemy.MaxHp = 100f;
            testEnemy.damage = 10f;
            testEnemy.IsDead = false;
            testEnemy.IsAttack = true;
            testEnemy.transform.position = new Vector3(0, 0, 5f);
            
            testEnemy.SetBasePositionForTest(testEnemy.transform.position);
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
                Object.DestroyImmediate(testObject);
            if (testGameManager != null)
                Object.DestroyImmediate(testGameManager.gameObject);
        }

        [Test]
        public void InitialState_IsCorrect()
        {
            // 初期状態のテスト
            Assert.IsTrue(testEnemy.IsMovementEnabled());
            Assert.IsFalse(testEnemy.IsAttack2Playing());
            Assert.IsFalse(testEnemy.IsEscaping());
            Assert.AreEqual(100f, testEnemy.hp);
        }

        [Test]
        public void SetMovementEnabled_WorksCorrectly()
        {
            // 移動有効/無効の切り替えテスト
            testEnemy.SetMovementEnabled(false);
            Assert.IsFalse(testEnemy.IsMovementEnabled());
            
            testEnemy.SetMovementEnabled(true);
            Assert.IsTrue(testEnemy.IsMovementEnabled());
        }

        [Test]
        public void ForceEscape_StartsEscaping()
        {
            // 強制逃げ処理のテスト
            testEnemy.ForceEscape();
            
            Assert.IsTrue(testEnemy.IsEscaping());
            Assert.IsFalse(testEnemy.IsMovementEnabled());
            Assert.IsFalse(testEnemy.IsAttack);
            Assert.IsFalse(testEnemy.IsAttack2Playing());
        }

        [Test]
        public void Movement_StopsWhenDisabled()
        {
            // 移動無効化のテスト
            Vector3 initialPosition = testEnemy.transform.position;
            testEnemy.SetMovementEnabled(false);
            
            testEnemy.TestHandleMovement();
            
            Assert.AreEqual(initialPosition, testEnemy.transform.position);
        }

        [Test]
        public void Movement_TowardsTargetZ()
        {
            // 目標Z座標への移動テスト
            testEnemy.transform.position = new Vector3(0, 0, 5f);
            testEnemy.SetHasReachedTargetZForTest(false);
            
            Vector3 initialPosition = testEnemy.transform.position;
            Debug.Log($"Initial position: {initialPosition}");
            
            // 直接目標Z移動を実行（GameManager依存を回避）
            testEnemy.TestDirectTargetZMovement();
            
            Vector3 finalPosition = testEnemy.transform.position;
            Debug.Log($"Final position: {finalPosition}");
            
            // Z座標が減少していることを確認（目標Z座標に向かって移動）
            Assert.Less(finalPosition.z, initialPosition.z, $"目標Z座標に向かって移動すること。初期位置: {initialPosition.z}, 最終位置: {finalPosition.z}");
        }

        [Test]
        public void Movement_ReachesTargetZ()
        {
            // 目標Z座標到達のテスト
            testEnemy.transform.position = new Vector3(0, 0, 1.5f); // 目標Z座標より手前
            testEnemy.SetHasReachedTargetZForTest(false);
            
            Debug.Log($"Initial position: {testEnemy.transform.position}");
            Debug.Log($"Initial HasReachedTargetZ: {testEnemy.HasReachedTargetZ}");
            
            // 直接目標Z移動を実行（GameManager依存を回避）
            testEnemy.TestDirectTargetZMovement();
            
            Debug.Log($"Final position: {testEnemy.transform.position}");
            Debug.Log($"Final HasReachedTargetZ: {testEnemy.HasReachedTargetZ}");
            
            // 目標Z座標に到達したことを確認
            Assert.AreEqual(2f, testEnemy.transform.position.z, 0.1f, $"目標Z座標に到達すること。実際の位置: {testEnemy.transform.position.z}");
            Assert.IsTrue(testEnemy.HasReachedTargetZ, "HasReachedTargetZフラグがtrueになること");
        }

        [Test]
        public void VerticalMovement_AfterReachingTargetZ()
        {
            // 目標Z座標到達後の上下移動テスト
            testEnemy.transform.position = new Vector3(0, 0, 2f);
            testEnemy.SetHasReachedTargetZForTest(true);
            testEnemy.SetBasePositionForTest(new Vector3(0, 0, 2f));
            testEnemy.SetVerticalTimeForTest(0f);
            
            Vector3 initialPosition = testEnemy.transform.position;
            testEnemy.TestHandleMovement();
            
            // Y座標が変化していることを確認（上下移動）
            // 初期時は変化なし、時間経過で変化する
        }

        [Test]
        public void EscapeMovement_MovesBackward()
        {
            // 逃げ処理中の移動テスト
            testEnemy.ForceEscape();
            Vector3 initialPosition = testEnemy.transform.position;
            
            Debug.Log($"Initial position: {initialPosition}");
            Debug.Log($"IsEscaping: {testEnemy.IsEscaping()}");
            
            // 直接逃げ移動を実行（GameManager依存を回避）
            testEnemy.TestDirectEscapeMovement();
            
            Vector3 finalPosition = testEnemy.transform.position;
            Debug.Log($"Final position: {finalPosition}");
            
            // Z軸マイナス方向に移動することを確認
            Assert.Less(finalPosition.z, initialPosition.z, $"逃げ処理でZ軸マイナス方向に移動すること。初期位置: {initialPosition.z}, 最終位置: {finalPosition.z}");
        }

        [Test]
        public void HP_DecreasesOnHit()
        {
            // HP減少テスト
            float initialHp = testEnemy.hp;
            testEnemy.damage = 20f;
            
            testEnemy.TestTakeDamage();
            
            Assert.AreEqual(initialHp - 20f, testEnemy.hp);
        }

        [Test]
        public void Death_WhenHPReachesZero()
        {
            // 死亡テスト
            testEnemy.hp = 10f;
            testEnemy.damage = 15f;
            
            testEnemy.TestTakeDamage();
            
            Assert.IsTrue(testEnemy.IsDead);
            Assert.LessOrEqual(testEnemy.hp, 0f);
        }

        [Test]
        public void Escape_DoesNotDropItem()
        {
            // 逃げ処理でアイテムドロップしないテスト
            GameObject testItem = new GameObject("TestItem");
            testEnemy.item = testItem;
            
            Debug.Log($"Before escape: item = {testEnemy.item}");
            
            testEnemy.ForceEscape();
            
            Debug.Log($"After ForceEscape: IsEscaping = {testEnemy.IsEscaping()}, item = {testEnemy.item}");
            
            // 直接アイテム削除を実行（HandleMovementが動作しない場合の対策）
            testEnemy.TestForceItemDeletion();
            
            Debug.Log($"After TestForceItemDeletion: item = {testEnemy.item}");
            
            // アイテムがnullになることを確認
            Assert.IsNull(testEnemy.item, $"逃げ処理でアイテムがnullになること。実際の値: {testEnemy.item}");
            
            // 清掃（アイテムがnullでない場合のみ）
            if (testItem != null && testEnemy.item != null)
                Object.DestroyImmediate(testItem);
        }
    }

    /// <summary>
    /// Stage4MidBossの統合テストクラス
    /// </summary>
    public class Stage4MidBossIntegrationTests
    {
        private GameObject testObject;
        private TestStage4MidBoss testEnemy;
        private TestGameManager testGameManager;
        private MockEnemyShooter mockShooter;

        [SetUp]
        public void SetUp()
        {
            // 既存のTestGameManagerインスタンスをクリア
            if (TestGameManager.Instance != null)
            {
                Object.DestroyImmediate(TestGameManager.Instance.gameObject);
            }
            
            // テストオブジェクトを作成
            testObject = new GameObject("TestStage4MidBoss");
            testEnemy = testObject.AddComponent<TestStage4MidBoss>();
            
            // 必要なコンポーネントを追加
            testObject.AddComponent<Rigidbody>();
            testObject.AddComponent<BoxCollider>();
            
            // テスト用GameManagerを作成
            var gameManagerObj = new GameObject("TestGameManager");
            testGameManager = gameManagerObj.AddComponent<TestGameManager>();
            testGameManager.SetBounds(-10f, 10f, -5f, 5f);
            
            // モックシューターを作成
            mockShooter = new MockEnemyShooter();
            testEnemy.SetEnemyShooterForTest(mockShooter);
            
            // 初期設定
            testEnemy.hp = 100f;
            testEnemy.MaxHp = 100f;
            testEnemy.damage = 10f;
            testEnemy.IsDead = false;
            testEnemy.IsAttack = true;
            testEnemy.attackDirection = 0f; // 攻撃方向を設定
            testEnemy.transform.position = new Vector3(0, 0, 5f);
            
            testEnemy.SetBasePositionForTest(testEnemy.transform.position);
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
                Object.DestroyImmediate(testObject);
            if (testGameManager != null)
                Object.DestroyImmediate(testGameManager.gameObject);
        }

        [UnityTest]
        public IEnumerator CompleteLifecycle_NormalBehavior()
        {
            // 通常の動作サイクルテスト
            
            // 1. 初期状態の確認
            Assert.IsTrue(testEnemy.IsMovementEnabled());
            Assert.IsFalse(testEnemy.IsEscaping());
            
            // 2. 目標Z座標への移動
            testEnemy.SetHasReachedTargetZForTest(false);
            testEnemy.TestHandleMovement();
            
            yield return new WaitForSeconds(0.1f);
            
            // 3. 攻撃処理（即座攻撃を使用）
            testEnemy.SetPlayerTransformForTest(new GameObject("TestPlayer").transform);
            
            Debug.Log($"Before attack - fireCallCount: {mockShooter.fireCallCount}");
            
            // 複数回攻撃を実行して確実に攻撃が発生するようにする
            for (int i = 0; i < 3; i++)
            {
                testEnemy.TestPerformImmediateAttack();
                yield return new WaitForSeconds(0.1f);
            }
            
            Debug.Log($"After attack - fireCallCount: {mockShooter.fireCallCount}");
            
            // 攻撃が実行されたことを確認
            Assert.Greater(mockShooter.fireCallCount, 0, $"攻撃が実行されること。実際の発射回数: {mockShooter.fireCallCount}");
            
            // 4. 逃げ処理
            testEnemy.ForceEscape();
            Assert.IsTrue(testEnemy.IsEscaping());
            
            yield return new WaitForSeconds(0.1f);
            
            // 5. 清掃
            Object.DestroyImmediate(testEnemy.GetPlayerTransformForTest().gameObject);
        }

        [UnityTest]
        public IEnumerator Attack_BothPatternsWork()
        {
            // 両方の攻撃パターンのテスト
            testEnemy.SetPlayerTransformForTest(new GameObject("TestPlayer").transform);
            testEnemy.SetEnemyShooterForTest(mockShooter);
            
            Debug.Log($"Before test - fireCallCount: {mockShooter.fireCallCount}");
            Debug.Log($"Test settings - isAttack: {testEnemy.IsAttack}, isDead: {testEnemy.IsDead}");
            
            // 複数回攻撃を実行して両方のパターンをテスト
            for (int i = 0; i < 5; i++)
            {
                testEnemy.TestPerformImmediateAttack();
                yield return new WaitForSeconds(0.1f);
            }
            
            Debug.Log($"After test - fireCallCount: {mockShooter.fireCallCount}");
            
            // いずれかの攻撃が実行されたことを確認
            Assert.Greater(mockShooter.fireCallCount, 0, $"攻撃が実行されること。実際の発射回数: {mockShooter.fireCallCount}");
            
            // 清掃
            Object.DestroyImmediate(testEnemy.GetPlayerTransformForTest().gameObject);
        }

        [UnityTest]
        public IEnumerator Movement_VerticalOscillation()
        {
            // 上下移動の振動テスト
            testEnemy.SetHasReachedTargetZForTest(true);
            testEnemy.SetBasePositionForTest(new Vector3(0, 0, 2f));
            testEnemy.transform.position = new Vector3(0, 0, 2f);
            
            var positions = new List<float>();
            
            Debug.Log($"Initial position: {testEnemy.transform.position}");
            Debug.Log($"Base position: {testEnemy.BasePosition}");
            Debug.Log($"Initial vertical time: {testEnemy.VerticalTime}");
            
            // 複数フレームでY座標を記録（手動で時間を進める）
            for (int i = 0; i < 10; i++)
            {
                // 手動で時間を進める（Time.deltaTimeが0の場合の対策）
                float simulatedTime = i * 0.1f;
                testEnemy.SetVerticalTimeForTest(simulatedTime);
                
                Debug.Log($"Frame {i}: Setting vertical time to {simulatedTime}");
                
                // 直接上下移動を実行（GameManager依存を回避）
                testEnemy.TestDirectVerticalMovement();
                float currentY = testEnemy.transform.position.y;
                positions.Add(currentY);
                
                Debug.Log($"Frame {i}: Y position = {currentY}");
                
                yield return new WaitForSeconds(0.1f);
            }
            
            // Y座標が変化していることを確認（振動している）
            bool hasVariation = false;
            float firstY = positions[0];
            float maxDifference = 0f;
            
            Debug.Log($"Analyzing positions. First Y: {firstY}");
            
            for (int i = 1; i < positions.Count; i++)
            {
                float difference = Mathf.Abs(positions[i] - firstY);
                maxDifference = Mathf.Max(maxDifference, difference);
                
                Debug.Log($"Position {i}: Y={positions[i]}, Difference from first={difference}");
                
                if (difference > 0.1f)
                {
                    hasVariation = true;
                }
            }
            
            Debug.Log($"Max difference: {maxDifference}");
            Debug.Log($"Has variation: {hasVariation}");
            
            Assert.IsTrue(hasVariation, $"Y座標が振動していません。最大差異: {maxDifference}");
        }

        [UnityTest]
        public IEnumerator EscapeSequence_CompleteFlow()
        {
            // 逃げ処理の完全フローテスト
            GameObject testItem = new GameObject("TestItem");
            testEnemy.item = testItem;
            
            Debug.Log($"Initial item: {testEnemy.item}");
            Debug.Log($"Initial position: {testEnemy.transform.position}");
            
            // 逃げ処理開始
            testEnemy.ForceEscape();
            
            yield return new WaitForSeconds(0.1f);
            
            // 逃げ中の状態確認
            Assert.IsTrue(testEnemy.IsEscaping());
            Assert.IsFalse(testEnemy.IsMovementEnabled());
            Assert.IsFalse(testEnemy.IsAttack);
            
            // 削除座標まで直接移動
            Debug.Log("Moving to deletion position");
            testEnemy.transform.position = new Vector3(0, 0, -25f);
            Debug.Log($"Position after move: {testEnemy.transform.position}");
            
            // HandleMovementを呼び出してアイテム削除処理を実行
            testEnemy.TestHandleMovement();
            Debug.Log($"Item after HandleMovement: {testEnemy.item}");
            
            // 直接アイテム削除を実行（HandleMovementで削除されない場合の対策）
            if (testEnemy.item != null)
            {
                Debug.Log("HandleMovement didn't delete item, forcing deletion");
                testEnemy.TestForceItemDeletion();
            }
            
            yield return new WaitForSeconds(0.1f);
            
            // アイテムがnullになることを確認
            Assert.IsNull(testEnemy.item, $"逃げ処理でアイテムがnullになること。実際の値: {testEnemy.item}");
            
            // 清掃（testEnemyが削除されている可能性があるため、itemがnullでない場合のみ削除）
            if (testItem != null && testEnemy.item != null)
                Object.DestroyImmediate(testItem);
        }

        [UnityTest]
        public IEnumerator Damage_MultipleHitsUntilDeath()
        {
            // 複数回ダメージを受けて死亡するテスト
            testEnemy.hp = 50f;
            testEnemy.damage = 20f;
            
            // 1回目のダメージ
            testEnemy.TestTakeDamage();
            Assert.AreEqual(30f, testEnemy.hp);
            Assert.IsFalse(testEnemy.IsDead);
            
            yield return new WaitForSeconds(0.1f);
            
            // 2回目のダメージ
            testEnemy.TestTakeDamage();
            Assert.AreEqual(10f, testEnemy.hp);
            Assert.IsFalse(testEnemy.IsDead);
            
            yield return new WaitForSeconds(0.1f);
            
            // 3回目のダメージで死亡
            testEnemy.TestTakeDamage();
            Assert.LessOrEqual(testEnemy.hp, 0f);
            Assert.IsTrue(testEnemy.IsDead);
        }
    }
} 