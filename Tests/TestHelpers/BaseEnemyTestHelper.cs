using System.Collections;
using UnityEngine;
using NUnit.Framework;
using VampSTG.Tests.Mocks;

namespace VampSTG.Tests.Helpers
{
    /// <summary>
    /// BaseEnemyテスト用の共通ヘルパークラス
    /// </summary>
    public static class BaseEnemyTestHelper
    {
        // テスト用定数
        public const float c_DefaultTestHp = 100f;
        public const float c_DefaultTestDamage = 25f;
        public const float c_DefaultAttackInterval = 0.1f;
        public const float c_DefaultAttackAnimationWait = 0.02f;
        
        /// <summary>
        /// テスト用敵オブジェクトを作成
        /// </summary>
        public static TestEnemySetupResult CreateTestEnemy(string _name = "TestEnemy")
        {
            var enemyObject = new GameObject(_name);
            var enemy = enemyObject.AddComponent<TestEnemy>();
            enemy.hp = c_DefaultTestHp;
            
            // コライダー追加
            var collider = enemyObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            
            return new TestEnemySetupResult
            {
                enemyObject = enemyObject,
                enemy = enemy,
                collider = collider
            };
        }
        
        /// <summary>
        /// テスト用プレイヤーオブジェクトを作成
        /// </summary>
        public static GameObject CreateTestPlayer(string _name = "TestPlayer")
        {
            var playerObject = new GameObject(_name);
            playerObject.tag = "Core";
            return playerObject;
        }
        
        /// <summary>
        /// テスト用プレイヤー弾を作成
        /// </summary>
        public static GameObject CreatePlayerBullet(float _damage, string _name = "PlayerBullet")
        {
            var bulletObject = new GameObject(_name);
            bulletObject.tag = "PlayerBullet";
            
            // 実際のConfigPlayerBulletを使用
            var bullet = bulletObject.AddComponent<ConfigPlayerBullet>();
            bullet.damage = _damage;
            bullet.isDestroy = true;
            
            var collider = bulletObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            
            return bulletObject;
        }
        
        /// <summary>
        /// テスト用ボスオブジェクトを作成
        /// </summary>
        public static GameObject CreateBossObject(string _name = "TestBoss")
        {
            var bossObject = new GameObject(_name);
            bossObject.tag = "Boss";
            
            var collider = bossObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            
            return bossObject;
        }
        
        /// <summary>
        /// テスト用モックシューターを作成
        /// </summary>
        public static MockEnemyShooter CreateMockShooter(GameObject _targetObject = null)
        {
            return new MockEnemyShooter();
        }
        
        /// <summary>
        /// 敵の基本セットアップを実行
        /// </summary>
        public static void SetupEnemyForTesting(TestEnemy _enemy, GameObject _player, bool _enableAttack = false)
        {
            // playerTransformを設定
            _enemy.SetPlayerTransformForTest(_player.transform);
            
            if (_enableAttack)
            {
                _enemy.IsAttack = true;
                _enemy.AttackInterval = c_DefaultAttackInterval;
                _enemy.AttackAnimationWait = c_DefaultAttackAnimationWait;
            }
            
            // テスト環境ではStart()が自動的に呼ばれない場合があるため、明示的に初期化
            _enemy.InitializeForTest();
        }
        
        /// <summary>
        /// 複数のGameObjectを一括削除
        /// </summary>
        public static void CleanupGameObjects(params GameObject[] _objects)
        {
            foreach (var obj in _objects)
            {
                if (obj != null)
                    Object.DestroyImmediate(obj);
            }
        }
    }
} 