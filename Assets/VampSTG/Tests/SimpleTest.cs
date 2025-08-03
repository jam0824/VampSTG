using NUnit.Framework;
using UnityEngine;
using VampSTG.Tests.Mocks;
using VampSTG.Tests.Helpers;

namespace VampSTG.Tests
{
    /// <summary>
    /// 基本的な動作を検証するための簡単なテスト
    /// </summary>
    public class SimpleTest
    {
        private GameObject m_TestObject;
        private TestEnemy m_TestEnemy;
        
        [SetUp]
        public void SetUp()
        {
            // テスト用オブジェクト作成
            m_TestObject = new GameObject("SimpleTestEnemy");
            m_TestEnemy = m_TestObject.AddComponent<TestEnemy>();
            m_TestEnemy.hp = 100f;
            
            // プレイヤーオブジェクト作成
            var playerObject = new GameObject("TestPlayer");
            playerObject.tag = "Core";
            m_TestEnemy.SetPlayerTransformForTest(playerObject.transform);
            
            // テスト環境でStart()を明示的に呼び出し
            m_TestEnemy.InitializeForTest();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (m_TestObject != null)
                Object.DestroyImmediate(m_TestObject);
                
            var playerObject = GameObject.FindWithTag("Core");
            if (playerObject != null)
                Object.DestroyImmediate(playerObject);
        }
        
        [Test]
        public void TestMockShooterCreation()
        {
            // Arrange & Act
            var mockShooter = new MockEnemyShooter();
            
            // Assert
            Assert.IsNotNull(mockShooter, "MockEnemyShooterが作成されること");
            Assert.AreEqual(0, mockShooter.fireCallCount, "初期状態でfireCallCountが0であること");
        }
        
        [Test]
        public void TestMockShooterFire()
        {
            // Arrange
            var mockShooter = new MockEnemyShooter();
            
            // Act
            mockShooter.Fire();
            
            // Assert
            Assert.AreEqual(1, mockShooter.fireCallCount, "Fire()呼び出し後にfireCallCountが1になること");
        }
        
        [Test]
        public void TestEnemyShooterSetting()
        {
            // Arrange
            var mockShooter = new MockEnemyShooter();
            
            // Act
            m_TestEnemy.SetEnemyShooterForTest(mockShooter);
            
            // Assert
            Assert.IsTrue(m_TestEnemy.HasEnemyShooter, "EnemyShooterが正しく設定されること");
        }
        
        [Test]
        public void TestEnemyAttack()
        {
            // Arrange
            var mockShooter = new MockEnemyShooter();
            m_TestEnemy.SetEnemyShooterForTest(mockShooter);
            
            // Act
            m_TestEnemy.PerformSingleAttackForTest();
            
            // Assert
            Assert.IsTrue(mockShooter.fireCallCount > 0, "攻撃実行後にfireCallCountが増加すること");
        }
    }
} 