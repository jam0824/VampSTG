using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// BaseEnemyの統合テスト
/// </summary>
public class BaseEnemyIntegrationTests
{
    private GameObject testSceneRoot;
    private GameObject playerObject;
    private GameObject enemyObject;
    private TestEnemy enemy;

    [SetUp]
    public void SetUp()
    {
        // テストシーンのルートオブジェクト作成
        testSceneRoot = new GameObject("TestSceneRoot");
        
        // プレイヤーオブジェクト作成
        playerObject = new GameObject("Player");
        playerObject.tag = "Core";
        playerObject.transform.SetParent(testSceneRoot.transform);
        
        // 敵オブジェクト作成
        enemyObject = new GameObject("Enemy");
        enemyObject.transform.SetParent(testSceneRoot.transform);
        enemy = enemyObject.AddComponent<TestEnemy>();
        enemy.hp = 100f;
        
        // playerTransformを設定
        enemy.SetPlayerTransformForTest(playerObject.transform);
        
        // コライダー追加
        var enemyCollider = enemyObject.AddComponent<BoxCollider>();
        enemyCollider.isTrigger = true;
    }

    [TearDown]
    public void TearDown()
    {
        if (testSceneRoot != null)
            Object.DestroyImmediate(testSceneRoot);
    }

    [UnityTest]
    public IEnumerator TestEnemyLifecycle()
    {
        // Arrange
        enemy.hp = 10f;
        Assert.AreEqual(10f, enemy.hp, "Initial HP should be set correctly");
        
        // Act - プレイヤー弾との衝突をシミュレート
        var bulletObject = CreatePlayerBullet(15f);
        var bulletCollider = bulletObject.GetComponent<Collider>();
        
        // 弾との衝突をシミュレート
        enemy.TestOnTriggerEnter(bulletCollider);
        
        // Assert
        Assert.LessOrEqual(enemy.hp, 0f, "Enemy HP should be zero or below after fatal hit");
        
        // Clean up
        Object.DestroyImmediate(bulletObject);
        yield return null;
    }

    [UnityTest]
    public IEnumerator TestMultipleHits()
    {
        // Arrange
        enemy.hp = 100f;
        
        // Act - 複数回のダメージ
        for (int i = 0; i < 3; i++)
        {
            var bulletObject = CreatePlayerBullet(25f);
            var bulletCollider = bulletObject.GetComponent<Collider>();
            enemy.TestOnTriggerEnter(bulletCollider);
            Object.DestroyImmediate(bulletObject);
            yield return new WaitForFixedUpdate();
        }
        
        // Assert
        Assert.AreEqual(25f, enemy.hp, "HP should decrease correctly after multiple hits");
    }

    [Test]
    public void TestBossCollision()
    {
        // Arrange
        enemy.hp = 100f;
        enemy.FromBossDamage = 30;
        
        var bossObject = new GameObject("Boss");
        bossObject.tag = "Boss";
        var bossCollider = bossObject.AddComponent<BoxCollider>();
        bossCollider.isTrigger = true;
        
        // Act
        enemy.TestOnTriggerEnter(bossCollider);
        
        // Assert
        Assert.AreEqual(70f, enemy.hp, "HP should decrease by boss damage amount");
        
        // Clean up
        Object.DestroyImmediate(bossObject);
    }

    [Test]
    public void TestItemPropertySetting()
    {
        // Arrange
        var itemPrefab = new GameObject("ItemPrefab");
        
        // Act
        enemy.item = itemPrefab;
        
        // Assert
        Assert.AreEqual(itemPrefab, enemy.item, "Item property should be set correctly");
        
        // Clean up
        Object.DestroyImmediate(itemPrefab);
    }

    [UnityTest]
    public IEnumerator TestAttackBehavior()
    {
        // Arrange
        enemy.IsAttack = true;
        enemy.AttackInterval = 0.1f;
        
        // Mock IEnemyShooter
        var mockShooter = enemyObject.AddComponent<MockEnemyShooter>();
        enemy.SetEnemyShooterForTest(mockShooter);
        
        // Debug: enemyShooterが正しく設定されているか確認
        Assert.IsTrue(enemy.HasEnemyShooter, "EnemyShooter should be set before testing attack");
        
        // Act - 直接攻撃を実行してテストする
        Debug.Log($"Before attack: fireCallCount = {mockShooter.fireCallCount}");
        enemy.PerformSingleAttackForTest();
        Debug.Log($"After attack: fireCallCount = {mockShooter.fireCallCount}");
        
        yield return null; // 1フレーム待機
        
        // Assert
        Assert.IsTrue(mockShooter.fireCallCount > 0, $"Enemy should attempt to fire when attack is enabled. Actual fireCallCount: {mockShooter.fireCallCount}");
    }

    [UnityTest]
    public IEnumerator TestAttackCoroutineBehavior()
    {
        // Arrange
        enemy.IsAttack = true;
        enemy.AttackInterval = 0.05f;
        enemy.AttackAnimationWait = 0.02f; // アニメーション待機時間を短く設定
        
        // Mock IEnemyShooter
        var mockShooter = enemyObject.AddComponent<MockEnemyShooter>();
        enemy.SetEnemyShooterForTest(mockShooter);
        
        // Debug: enemyShooterが正しく設定されているか確認
        Assert.IsTrue(enemy.HasEnemyShooter, "EnemyShooter should be set before testing attack");
        
        Debug.Log($"Test settings: AttackInterval={enemy.AttackInterval}, AttackAnimationWait={enemy.AttackAnimationWait}");
        
        // Act - 攻撃コルーチンを手動で開始
        enemy.StartAttackCoroutineForTest();
        
        // 十分に長い時間待機（0.05 + 0.02 + α）
        yield return new WaitForSeconds(0.15f);
        
        // Assert
        Assert.IsTrue(mockShooter.fireCallCount > 0, $"Enemy should attempt to fire when attack coroutine is running. fireCallCount: {mockShooter.fireCallCount}");
    }
    
    [UnityTest]
    public IEnumerator TestSimpleAttackCoroutine()
    {
        // Arrange
        // Mock IEnemyShooter
        var mockShooter = enemyObject.AddComponent<MockEnemyShooter>();
        enemy.SetEnemyShooterForTest(mockShooter);
        
        // Debug: enemyShooterが正しく設定されているか確認
        Assert.IsTrue(enemy.HasEnemyShooter, "EnemyShooter should be set before testing attack");
        
        // Act - 簡単な攻撃コルーチンを開始
        Debug.Log($"Before simple attack: fireCallCount = {mockShooter.fireCallCount}");
        enemy.StartSimpleAttackCoroutineForTest();
        
        // 短い時間待機
        yield return new WaitForSeconds(0.2f);
        
        Debug.Log($"After simple attack: fireCallCount = {mockShooter.fireCallCount}");
        
        // Assert
        Assert.IsTrue(mockShooter.fireCallCount > 0, $"Enemy should fire in simple attack coroutine. Actual fireCallCount: {mockShooter.fireCallCount}");
    }
    
    [UnityTest]
    public IEnumerator TestAttackCoroutineWithLongerWait()
    {
        // Arrange
        enemy.IsAttack = true;
        enemy.AttackInterval = 0.05f;
        enemy.AttackAnimationWait = 0.02f;
        
        // Mock IEnemyShooter
        var mockShooter = enemyObject.AddComponent<MockEnemyShooter>();
        enemy.SetEnemyShooterForTest(mockShooter);
        
        Debug.Log($"Test settings: AttackInterval={enemy.AttackInterval}, AttackAnimationWait={enemy.AttackAnimationWait}");
        Debug.Log($"Expected total time: {enemy.AttackInterval + enemy.AttackAnimationWait} seconds");
        
        // Act - 攻撃コルーチンを手動で開始
        enemy.StartAttackCoroutineForTest();
        
        // 十分な時間待機
        yield return new WaitForSeconds(0.5f);
        
        Debug.Log($"After wait: fireCallCount = {mockShooter.fireCallCount}");
        
        // Assert
        Assert.IsTrue(mockShooter.fireCallCount > 0, $"Enemy should fire after sufficient wait. fireCallCount: {mockShooter.fireCallCount}");
    }

    private GameObject CreatePlayerBullet(float damage)
    {
        var bulletObject = new GameObject("PlayerBullet");
        bulletObject.tag = "PlayerBullet";
        var bullet = bulletObject.AddComponent<TestConfigPlayerBullet>();
        bullet.damage = damage;
        bullet.isDestroy = true;
        
        var collider = bulletObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        
        return bulletObject;
    }
}

/// <summary>
/// テスト用のモックEnemyShooter
/// </summary>
public class MockEnemyShooter : MonoBehaviour, IEnemyShooter
{
    public int fireCallCount = 0;
    
    public void Fire()
    {
        fireCallCount++;
        Debug.Log($"MockEnemyShooter.Fire() called! fireCallCount is now {fireCallCount}");
    }
    
    public void Fire(float direction)
    {
        fireCallCount++;
        Debug.Log($"MockEnemyShooter.Fire(direction={direction}) called! fireCallCount is now {fireCallCount}");
    }
} 