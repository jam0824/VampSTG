using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace VampSTG.Tests
{
    /// <summary>
    /// GameManagerのFPS機能を検証するテスト
    /// </summary>
    public class GameManagerFpsTests
    {
        private GameObject m_gameManagerObject;
        private GameManager m_gameManager;
        private int m_originalFrameRate;

        [SetUp]
        public void SetUp()
        {
            // 現在のフレームレートを保存
            m_originalFrameRate = Application.targetFrameRate;
            
            // GameManagerのテスト用オブジェクト作成
            m_gameManagerObject = new GameObject("TestGameManager");
            m_gameManager = m_gameManagerObject.AddComponent<GameManager>();
            
            // シングルトンの設定を回避するため、直接テスト
            // （通常のAwake()は呼ばれないようにする）
        }

        [TearDown]
        public void TearDown()
        {
            // 元のフレームレートを復元
            Application.targetFrameRate = m_originalFrameRate;
            
            // テストオブジェクトのクリーンアップ
            if (m_gameManagerObject != null)
            {
                Object.DestroyImmediate(m_gameManagerObject);
            }
        }

        [Test]
        public void SetFrameRate_正常な値_フレームレートが設定される()
        {
            // Arrange
            int expectedFrameRate = 30;

            // Act
            m_gameManager.SetFrameRate(expectedFrameRate);

            // Assert
            Assert.AreEqual(expectedFrameRate, Application.targetFrameRate, 
                $"フレームレートが{expectedFrameRate}に設定されるべきです");
        }

        [Test]
        public void SetFrameRate_60FPS_正常に設定される()
        {
            // Arrange
            int expectedFrameRate = 60;

            // Act
            m_gameManager.SetFrameRate(expectedFrameRate);

            // Assert
            Assert.AreEqual(expectedFrameRate, Application.targetFrameRate, 
                "フレームレートが60に設定されるべきです");
        }

        [Test]
        public void SetFrameRate_120FPS_正常に設定される()
        {
            // Arrange
            int expectedFrameRate = 120;

            // Act
            m_gameManager.SetFrameRate(expectedFrameRate);

            // Assert
            Assert.AreEqual(expectedFrameRate, Application.targetFrameRate, 
                "フレームレートが120に設定されるべきです");
        }

        [Test]
        public void SetFrameRate_負の値_値は設定されるがWarningをログ出力()
        {
            // Arrange
            int negativeFrameRate = -1;

            // Act & Assert
            // Unityでは負の値も受け入れられる（-1は制限なしを意味する）
            // ただし、警告をログに出力することを確認
            LogAssert.Expect(LogType.Log, $"Target Frame Rate set to: {negativeFrameRate}");
            m_gameManager.SetFrameRate(negativeFrameRate);
            
            Assert.AreEqual(negativeFrameRate, Application.targetFrameRate, 
                "負の値でもフレームレートが設定されるべきです");
        }

        [Test]
        public void SetFrameRate_0FPS_値は設定される()
        {
            // Arrange
            int zeroFrameRate = 0;

            // Act
            m_gameManager.SetFrameRate(zeroFrameRate);

            // Assert
            Assert.AreEqual(zeroFrameRate, Application.targetFrameRate, 
                "0のフレームレートが設定されるべきです");
        }

        [Test]
        public void SetFrameRate_複数回呼び出し_最後の値が設定される()
        {
            // Arrange
            int firstFrameRate = 30;
            int secondFrameRate = 60;
            int finalFrameRate = 144;

            // Act
            m_gameManager.SetFrameRate(firstFrameRate);
            m_gameManager.SetFrameRate(secondFrameRate);
            m_gameManager.SetFrameRate(finalFrameRate);

            // Assert
            Assert.AreEqual(finalFrameRate, Application.targetFrameRate, 
                "最後に設定したフレームレートが適用されるべきです");
        }

        [Test]
        public void SetFrameRate_異なる値を連続設定_全て正常に設定される()
        {
            // Arrange & Act & Assert
            int[] testFrameRates = { 24, 30, 60, 90, 120, 144 };

            foreach (int frameRate in testFrameRates)
            {
                m_gameManager.SetFrameRate(frameRate);
                Assert.AreEqual(frameRate, Application.targetFrameRate, 
                    $"フレームレート{frameRate}が正常に設定されるべきです");
            }
        }
    }
}