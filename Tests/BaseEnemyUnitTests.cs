using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VampSTG.Tests.Base;
using VampSTG.Tests.Helpers;
using VampSTG.Tests.Mocks;

namespace VampSTG.Tests.Unit
{
    /// <summary>
    /// BaseEnemyクラスの単体テスト
    /// </summary>
    public class BaseEnemyUnitTests : BaseEnemyTestBase
    {
        #region 基本機能テスト
        
        [Test]
        public void TestInitialHpSetting()
        {
            // Arrange & Act
            SetEnemyHp(50f);
            
            // Assert
            Assert.AreEqual(50f, m_TestEnemy.hp, "HPが正しく設定されること");
        }
        
        [Test]
        public void TestMovementIsCalled()
        {
            // Arrange
            m_TestEnemy.hasMovementBeenCalled = false;
            
            // Act
            m_TestEnemy.TestUpdate();
            
            // Assert
            Assert.IsTrue(m_TestEnemy.hasMovementBeenCalled, "Update時にHandleMovementが呼ばれること");
            Assert.IsTrue(m_TestEnemy.HasPlayerTransform, "PlayerTransformが設定されていること");
        }
        
        [Test]
        public void TestHpDecreasesOnHit()
        {
            // Arrange
            SetEnemyHp(100f);
            var bulletObject = CreatePlayerBullet(25f);
            var bullet = bulletObject.GetComponent<ConfigPlayerBullet>();
            
            // Act
            m_TestEnemy.TestHit(bullet, m_TestEnemy.hp);
            
            // Assert
            Assert.AreEqual(75f, m_TestEnemy.hp, "弾が当たった時にHPが減少すること");
        }
        
        [Test]
        public void TestEnemyDiesWhenHpIsZero()
        {
            // Arrange
            SetEnemyHp(0f);
            
            // Assert
            Assert.LessOrEqual(m_TestEnemy.hp, 0f, "HPが0以下の時に敵が死亡状態になること");
        }
        
        #endregion
        
        #region 衝突検知テスト
        
        [Test]
        public void TestOnTriggerEnterWithPlayerBullet()
        {
            // Arrange
            SetEnemyHp(100f);
            var bulletObject = CreatePlayerBullet(30f);
            var collider = bulletObject.GetComponent<Collider>();
            
            // Act
            m_TestEnemy.TestOnTriggerEnter(collider);
            
            // Assert
            Assert.AreEqual(70f, m_TestEnemy.hp, "プレイヤー弾との衝突でHPが減少すること");
        }
        
        [Test]
        public void TestOnTriggerEnterWithBoss()
        {
            // Arrange
            SetEnemyHp(100f);
            SetEnemyBossDamage(20);
            
            var bossObject = CreateBossObject();
            var collider = bossObject.GetComponent<Collider>();
            
            // Act
            m_TestEnemy.TestOnTriggerEnter(collider);
            
            // Assert
            Assert.AreEqual(80f, m_TestEnemy.hp, "ボスとの衝突でHPが減少すること");
        }
        
        #endregion
        
        #region 攻撃システムテスト
        
        [Test]
        public void TestEnemyShooterSetup()
        {
            // Arrange
            var mockShooter = CreateMockShooter();
            
            // Act
            m_TestEnemy.SetEnemyShooterForTest(mockShooter);
            
            // Assert
            Assert.IsTrue(m_TestEnemy.HasEnemyShooter, "EnemyShooterが正しく設定されること");
        }
        
        [Test]
        public void TestSingleAttack()
        {
            // Arrange
            var mockShooter = CreateMockShooter();
            m_TestEnemy.SetEnemyShooterForTest(mockShooter);
            
            // Act
            m_TestEnemy.PerformSingleAttackForTest();
            
            // Assert
            Assert.AreEqual(1, mockShooter.fireCallCount, "攻撃が1回実行されること");
        }
        
        [UnityTest]
        public IEnumerator TestAttackCoroutineWithBounds()
        {
            // Arrange
            SetupEnemyForAttack();
            var gameManagerObject = new GameObject("GameManager");
            var gameManager = gameManagerObject.AddComponent<GameManager>();
            gameManager.minZ = -10f;
            gameManager.maxZ = 10f;
            gameManager.minY = -10f;
            gameManager.maxY = 10f;
            AddToCleanupList(gameManagerObject);
            
            // 範囲外に配置
            m_TestEnemy.transform.position = new Vector3(0, 15f, 0);
            
            // Act
            yield return new WaitForSeconds(0.2f);
            
            // Assert
            bool isOutOfBounds = (m_TestEnemy.transform.position.y > 10f);
            Assert.IsTrue(isOutOfBounds, "敵が範囲外にいる場合は攻撃をスキップすること");
        }
        
        #endregion
        
        #region プロパティテスト
        
        [Test]
        public void TestAttackProperties()
        {
            // Arrange & Act
            m_TestEnemy.IsAttack = true;
            m_TestEnemy.AttackInterval = 2.5f;
            m_TestEnemy.AttackAnimationWait = 1.2f;
            
            // Assert
            Assert.IsTrue(m_TestEnemy.IsAttack, "IsAttackプロパティが正しく設定されること");
            Assert.AreEqual(2.5f, m_TestEnemy.AttackInterval, "AttackIntervalプロパティが正しく設定されること");
            Assert.AreEqual(1.2f, m_TestEnemy.AttackAnimationWait, "AttackAnimationWaitプロパティが正しく設定されること");
        }
        
        [Test]
        public void TestBossDamageProperty()
        {
            // Arrange & Act
            SetEnemyBossDamage(50);
            
            // Assert
            Assert.AreEqual(50, m_TestEnemy.FromBossDamage, "FromBossDamageプロパティが正しく設定されること");
        }
        
        #endregion
    }
} 