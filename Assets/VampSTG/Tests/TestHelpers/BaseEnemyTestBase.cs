using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using VampSTG.Tests.Helpers;
using VampSTG.Tests.Mocks;

namespace VampSTG.Tests.Base
{
    /// <summary>
    /// BaseEnemyテストの共通ベースクラス
    /// </summary>
    public abstract class BaseEnemyTestBase
    {
        // 共通テストオブジェクト
        protected GameObject m_PlayerObject;
        protected TestEnemySetupResult m_EnemySetup;
        protected TestEnemy m_TestEnemy;
        protected List<GameObject> m_CreatedObjects;
        
        [SetUp]
        public virtual void SetUp()
        {
            m_CreatedObjects = new List<GameObject>();
            
            // プレイヤーオブジェクト作成
            m_PlayerObject = BaseEnemyTestHelper.CreateTestPlayer();
            m_CreatedObjects.Add(m_PlayerObject);
            
            // 敵オブジェクト作成
            m_EnemySetup = BaseEnemyTestHelper.CreateTestEnemy();
            m_TestEnemy = m_EnemySetup.enemy;
            m_CreatedObjects.Add(m_EnemySetup.enemyObject);
            
            // 基本セットアップ
            BaseEnemyTestHelper.SetupEnemyForTesting(m_TestEnemy, m_PlayerObject);
            
            // 子クラス固有のセットアップ
            OnSetUp();
        }
        
        [TearDown]
        public virtual void TearDown()
        {
            // 子クラス固有のクリーンアップ
            OnTearDown();
            
            // 作成したオブジェクトを一括削除
            BaseEnemyTestHelper.CleanupGameObjects(m_CreatedObjects.ToArray());
            m_CreatedObjects.Clear();
        }
        
        /// <summary>
        /// 子クラス固有のセットアップ処理
        /// </summary>
        protected virtual void OnSetUp() { }
        
        /// <summary>
        /// 子クラス固有のクリーンアップ処理
        /// </summary>
        protected virtual void OnTearDown() { }
        
        /// <summary>
        /// テスト用オブジェクトを追跡リストに追加
        /// </summary>
        protected void AddToCleanupList(GameObject _obj)
        {
            if (_obj != null && !m_CreatedObjects.Contains(_obj))
            {
                m_CreatedObjects.Add(_obj);
            }
        }
        
        /// <summary>
        /// プレイヤー弾を作成してクリーンアップリストに追加
        /// </summary>
        protected GameObject CreatePlayerBullet(float _damage = BaseEnemyTestHelper.c_DefaultTestDamage)
        {
            var bullet = BaseEnemyTestHelper.CreatePlayerBullet(_damage);
            AddToCleanupList(bullet);
            return bullet;
        }
        
        /// <summary>
        /// ボスオブジェクトを作成してクリーンアップリストに追加
        /// </summary>
        protected GameObject CreateBossObject()
        {
            var boss = BaseEnemyTestHelper.CreateBossObject();
            AddToCleanupList(boss);
            return boss;
        }
        
        /// <summary>
        /// モックシューターを作成
        /// </summary>
        protected MockEnemyShooter CreateMockShooter()
        {
            return BaseEnemyTestHelper.CreateMockShooter();
        }
        
        /// <summary>
        /// 敵を攻撃可能状態にセットアップ
        /// </summary>
        protected void SetupEnemyForAttack()
        {
            BaseEnemyTestHelper.SetupEnemyForTesting(m_TestEnemy, m_PlayerObject, true);
        }
        
        /// <summary>
        /// 敵のHPを設定
        /// </summary>
        protected void SetEnemyHp(float _hp)
        {
            m_TestEnemy.hp = _hp;
        }
        
        /// <summary>
        /// 敵のボスダメージを設定
        /// </summary>
        protected void SetEnemyBossDamage(int _damage)
        {
            m_TestEnemy.FromBossDamage = _damage;
        }
    }
} 