using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VampSTG.Tests.Base;
using VampSTG.Tests.Helpers;
using VampSTG.Tests.Mocks;

namespace VampSTG.Tests.Integration
{
    /// <summary>
    /// BaseEnemyクラスの統合テスト（リファクタリング版）
    /// </summary>
    public class BaseEnemyIntegrationTestsRefactored : BaseEnemyTestBase
    {
        private GameObject m_TestSceneRoot;
        
        protected override void OnSetUp()
        {
            // テストシーンのルートオブジェクト作成
            m_TestSceneRoot = new GameObject("TestSceneRoot");
            AddToCleanupList(m_TestSceneRoot);
            
            // オブジェクトを階層化
            m_PlayerObject.transform.SetParent(m_TestSceneRoot.transform);
            m_EnemySetup.enemyObject.transform.SetParent(m_TestSceneRoot.transform);
        }
        
        #region ライフサイクルテスト
        
        [UnityTest]
        public IEnumerator TestEnemyLifecycle()
        {
            // Arrange
            SetEnemyHp(10f);
            Assert.AreEqual(10f, m_TestEnemy.hp, "初期HPが正しく設定されること");
            
            // Act - プレイヤー弾との衝突をシミュレート
            var bulletObject = CreatePlayerBullet(15f);
            var bulletCollider = bulletObject.GetComponent<Collider>();
            
            m_TestEnemy.TestOnTriggerEnter(bulletCollider);
            
            // Assert
            Assert.LessOrEqual(m_TestEnemy.hp, 0f, "致命的なダメージ後にHPが0以下になること");
            
            yield return null;
        }
        
        [UnityTest]
        public IEnumerator TestMultipleHits()
        {
            // Arrange
            SetEnemyHp(100f);
            const int hitCount = 3;
            const float damagePerHit = 25f;
            
            // Act - 複数回のダメージ
            for (int i = 0; i < hitCount; i++)
            {
                var bulletObject = CreatePlayerBullet(damagePerHit);
                var bulletCollider = bulletObject.GetComponent<Collider>();
                m_TestEnemy.TestOnTriggerEnter(bulletCollider);
                yield return new WaitForFixedUpdate();
            }
            
            // Assert
            float expectedHp = BaseEnemyTestHelper.c_DefaultTestHp - (hitCount * damagePerHit);
            Assert.AreEqual(expectedHp, m_TestEnemy.hp, "複数回の攻撃後にHPが正しく減少すること");
        }
        
        #endregion
        
        #region 衝突システムテスト
        
        [Test]
        public void TestBossCollision()
        {
            // Arrange
            SetEnemyHp(100f);
            SetEnemyBossDamage(30);
            
            var bossObject = CreateBossObject();
            var bossCollider = bossObject.GetComponent<Collider>();
            
            // Act
            m_TestEnemy.TestOnTriggerEnter(bossCollider);
            
            // Assert
            Assert.AreEqual(70f, m_TestEnemy.hp, "ボスとの衝突でHPが正しく減少すること");
        }
        
        [Test]
        public void TestItemPropertySetting()
        {
            // Arrange
            var itemPrefab = new GameObject("ItemPrefab");
            AddToCleanupList(itemPrefab);
            
            // Act
            m_TestEnemy.item = itemPrefab;
            
            // Assert
            Assert.AreEqual(itemPrefab, m_TestEnemy.item, "アイテムプロパティが正しく設定されること");
        }
        
        #endregion
        
        #region 攻撃システム統合テスト
        
        [UnityTest]
        public IEnumerator TestAttackBehavior()
        {
            // Arrange
            SetupEnemyForAttack();
            var mockShooter = CreateMockShooter();
            
            Debug.Log($"Before SetEnemyShooterForTest: HasEnemyShooter = {m_TestEnemy.HasEnemyShooter}");
            Debug.Log($"MockShooter type: {mockShooter.GetType().Name}");
            Debug.Log($"MockShooter null check: {mockShooter == null}");
            
            m_TestEnemy.SetEnemyShooterForTest(mockShooter);
            
            Debug.Log($"After SetEnemyShooterForTest: HasEnemyShooter = {m_TestEnemy.HasEnemyShooter}");
            
            Assert.IsTrue(m_TestEnemy.HasEnemyShooter, "EnemyShooterが設定されていること");
            
            // Act
            m_TestEnemy.PerformSingleAttackForTest();
            yield return null;
            
            // Assert
            Assert.IsTrue(mockShooter.fireCallCount > 0, 
                $"攻撃が実行されること。実際の発射回数: {mockShooter.fireCallCount}");
        }
        
        [UnityTest]
        public IEnumerator TestAttackCoroutineBehavior()
        {
            // Arrange
            SetupEnemyForAttack();
            var mockShooter = CreateMockShooter();
            m_TestEnemy.SetEnemyShooterForTest(mockShooter);
            
            Debug.Log($"Before test: fireCallCount = {mockShooter.fireCallCount}");
            Debug.Log($"Attack settings: Interval={m_TestEnemy.AttackInterval}, AnimationWait={m_TestEnemy.AttackAnimationWait}");
            Debug.Log($"TestEnemy enabled: {m_TestEnemy.enabled}");
            Debug.Log($"TestEnemy gameObject active: {m_TestEnemy.gameObject.activeInHierarchy}");
            
            // Act
            m_TestEnemy.StartAttackCoroutineForTest();
            
            // 短いフレーム待機
            Debug.Log("Waiting for a few frames...");
            for (int i = 0; i < 3; i++) // 短いフレーム数待機
            {
                yield return null;
            }
            
            Debug.Log($"After wait: fireCallCount = {mockShooter.fireCallCount}");
            
            // Assert
            Assert.IsTrue(mockShooter.fireCallCount > 0, 
                $"攻撃コルーチンが動作すること。実際の発射回数: {mockShooter.fireCallCount}");
        }
        
        [Test]
        public void TestDirectAttackCall()
        {
            // Arrange
            SetupEnemyForAttack();
            var mockShooter = CreateMockShooter();
            m_TestEnemy.SetEnemyShooterForTest(mockShooter);
            
            Debug.Log($"Direct attack test - Before: fireCallCount = {mockShooter.fireCallCount}");
            Debug.Log($"EnemyShooter null check: {m_TestEnemy.HasEnemyShooter}");
            
            // Act - 直接PerformSingleAttackForTestを呼ぶ
            m_TestEnemy.PerformSingleAttackForTest();
            
            Debug.Log($"Direct attack test - After: fireCallCount = {mockShooter.fireCallCount}");
            
            // Assert
            Assert.IsTrue(mockShooter.fireCallCount > 0, 
                $"直接攻撃呼び出しが動作すること。実際の発射回数: {mockShooter.fireCallCount}");
        }
        
        [Test]
        public void TestAttackSequenceExecution()
        {
            // Arrange
            SetupEnemyForAttack();
            var mockShooter = CreateMockShooter();
            m_TestEnemy.SetEnemyShooterForTest(mockShooter);
            
            Debug.Log($"Attack sequence test - Before: fireCallCount = {mockShooter.fireCallCount}");
            
            // Act - 攻撃シーケンスを実行
            m_TestEnemy.ExecuteAttackSequenceForTest();
            
            Debug.Log($"Attack sequence test - After: fireCallCount = {mockShooter.fireCallCount}");
            
            // Assert
            Assert.IsTrue(mockShooter.fireCallCount > 0, 
                $"攻撃シーケンス実行が動作すること。実際の発射回数: {mockShooter.fireCallCount}");
        }
        
        [Test]
        public void TestImmediateAttackExecution()
        {
            // Arrange
            SetupEnemyForAttack();
            var mockShooter = CreateMockShooter();
            m_TestEnemy.SetEnemyShooterForTest(mockShooter);
            
            Debug.Log($"Immediate attack test - Before: fireCallCount = {mockShooter.fireCallCount}");
            Debug.Log($"Mock shooter type: {mockShooter.GetType().Name}");
            
            // Act - 即座に攻撃を実行
            m_TestEnemy.ExecuteImmediateAttackForTest();
            
            Debug.Log($"Immediate attack test - After: fireCallCount = {mockShooter.fireCallCount}");
            
            // Assert
            Assert.IsTrue(mockShooter.fireCallCount > 0, 
                $"即座攻撃実行が動作すること。実際の発射回数: {mockShooter.fireCallCount}");
        }
        
        [UnityTest]
        public IEnumerator TestManualCoroutineExecution()
        {
            // Arrange
            SetupEnemyForAttack();
            var mockShooter = CreateMockShooter();
            m_TestEnemy.SetEnemyShooterForTest(mockShooter);
            
            Debug.Log($"Manual coroutine test - Before: fireCallCount = {mockShooter.fireCallCount}");
            
            // Act - TestEnemyを通してコルーチンを実行
            m_TestEnemy.StartManualCoroutineForTest();
            
            // フレームベースの待機
            for (int i = 0; i < 15; i++)
            {
                yield return null;
            }
            
            Debug.Log($"Manual coroutine test - After: fireCallCount = {mockShooter.fireCallCount}");
            
            // Assert
            Assert.IsTrue(mockShooter.fireCallCount > 0, 
                $"手動コルーチン実行が動作すること。実際の発射回数: {mockShooter.fireCallCount}");
        }
        
        [UnityTest]
        public IEnumerator TestSimpleAttackCoroutine()
        {
            // Arrange
            SetupEnemyForAttack();
            var mockShooter = CreateMockShooter();
            m_TestEnemy.SetEnemyShooterForTest(mockShooter);
            
            Debug.Log($"Simple test - Before: fireCallCount = {mockShooter.fireCallCount}");
            
            // Act
            m_TestEnemy.StartSimpleAttackCoroutineForTest();
            // フレームベースの短い待機
            for (int i = 0; i < 10; i++)
            {
                yield return null;
            }
            
            Debug.Log($"Simple test - After: fireCallCount = {mockShooter.fireCallCount}");
            
            // Assert
            Assert.IsTrue(mockShooter.fireCallCount > 0, 
                $"簡単な攻撃コルーチンが動作すること。実際の発射回数: {mockShooter.fireCallCount}");
        }
        
        [UnityTest]
        public IEnumerator TestDirectionalAttack()
        {
            // Arrange
            SetupEnemyForAttack();
            var mockShooter = CreateMockShooter();
            m_TestEnemy.SetEnemyShooterForTest(mockShooter);
            m_TestEnemy.IsDirectionAttack = true;
            m_TestEnemy.AttackDirection = 45f;
            
            // Act
            m_TestEnemy.PerformSingleAttackForTest();
            yield return null;
            
            // Assert
            Assert.IsTrue(mockShooter.fireCallCount > 0, "方向指定攻撃が実行されること");
            Assert.IsTrue(mockShooter.wasDirectionUsed, "方向が指定されて攻撃されること");
            Assert.AreEqual(45f, mockShooter.lastDirection, "正しい方向で攻撃されること");
        }
        
        #endregion
        
        #region パフォーマンステスト
        
        [UnityTest]
        public IEnumerator TestMultipleEnemiesPerformance()
        {
            // Arrange
            const int enemyCount = 10;
            var listEnemies = new System.Collections.Generic.List<TestEnemy>();
            
            for (int i = 0; i < enemyCount; i++)
            {
                var enemySetup = BaseEnemyTestHelper.CreateTestEnemy($"Enemy_{i}");
                BaseEnemyTestHelper.SetupEnemyForTesting(enemySetup.enemy, m_PlayerObject);
                listEnemies.Add(enemySetup.enemy);
                AddToCleanupList(enemySetup.enemyObject);
            }
            
            // Act & Assert - 複数の敵が同時に動作してもパフォーマンスに問題がないこと
            var startTime = Time.realtimeSinceStartup;
            
            foreach (var enemy in listEnemies)
            {
                enemy.TestUpdate();
            }
            
            var endTime = Time.realtimeSinceStartup;
            var processingTime = endTime - startTime;
            
            Assert.Less(processingTime, 0.1f, $"複数敵の処理時間が許容範囲内であること。実際の処理時間: {processingTime}秒");
            
            yield return null;
        }
        
        #endregion
        
        #region エラーハンドリングテスト
        
        [Test]
        public void TestNullBulletHandling()
        {
            // Arrange
            SetEnemyHp(100f);
            
            // Act & Assert - null弾でも例外が発生しないこと
            Assert.DoesNotThrow(() => m_TestEnemy.TestHit(null, m_TestEnemy.hp), 
                "null弾でも例外が発生しないこと");
        }
        
        [Test]
        public void TestNullPlayerTransformHandling()
        {
            // Arrange
            m_TestEnemy.SetPlayerTransformForTest(null);
            
            // Act & Assert - nullプレイヤーTransformでも例外が発生しないこと
            Assert.DoesNotThrow(() => m_TestEnemy.TestUpdate(), 
                "nullプレイヤーTransformでも例外が発生しないこと");
            
            Assert.IsFalse(m_TestEnemy.hasMovementBeenCalled, 
                "nullプレイヤーTransformの場合は移動処理が呼ばれないこと");
        }
        
        #endregion
    }
} 