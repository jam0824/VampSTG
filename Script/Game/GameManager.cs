using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public int killCount = 0;
    public int allKillCount = 0;
    public int score = 0;
    public int bulletCount = 0;
    public int allBulletCount = 0;
    public int itemCount = 0;
    public List<string> gotItems = new List<string>();
    public List<string> gotCharacters = new List<string>();
    public List<int> highScores = new List<int>();
    public int languageIndex = 0;
    public float globalBgmVol = 0.6f;
    public float globalSeVol = 1f;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("言語設定: 0英語/1日本語")]
    public int languageIndex = 0;
    [Header("選択されているキャラクター")]
    public CharacterData selectedCharacter;
    [Header("選択されているステージのシーン名")]
    public string selectedStageSceneName;

    [Header("音量")]
    public float globalBgmVol = 0.6f;
    public float globalSeVol = 1f;

    [Header("移動範囲")]
    public float maxZ = 9f;
    public float minZ = -9f;
    public float maxY = 6f;
    public float minY = -4f;

    [Header("Score倍率")]
    public float scoreMagnification = 100f;
    public int itemMagnification = 1000;

    public GameObject playerCore;

    [Header("Save系データ")]
    public int killCount = 0;
    public int allKillCount = 0;
    public int score = 0;
    public int bulletCount = 0;
    public int allBulletCount = 0;
    public int itemCount = 0;
    private List<string> gotItems = new List<string>();
    private List<string> gotCharacters = new List<string>();
    private List<int> highScores = new List<int>();

    // セーブファイルのフルパス
    private string SaveFilePath =>
        Path.Combine(Application.persistentDataPath, "saveData.json");

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGame();    // 起動時にロード
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// プレイヤーのスコアを追加
    /// </summary>
    public void AddScore(float maxHp)
    {
        score += (int)Math.Ceiling(maxHp * scoreMagnification);
    }

    /// <summary>
    /// ゲーム終了時に呼ばれる
    /// </summary>
    void OnApplicationQuit()
    {
        SaveGame();
    }

    /// <summary>
    /// アプリがバックグラウンドに回ったときにもセーブ
    /// </summary>
    /// <param name="pauseStatus"></param>
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGame();
    }

    /// <summary>
    /// 1ステージごとにリセットされるデータ
    /// </summary>
    public void ResetStageSaveData(){
        killCount = 0;
        score = 0;
        bulletCount = 0;
        itemCount = 0;
    }

    /// <summary>現在のフィールド値を JSON にしてファイルに書き込む</summary>
    public void SaveGame()
    {
        var data = new SaveData
        {
            killCount = this.killCount,
            allKillCount = this.allKillCount,
            score = this.score,
            bulletCount = this.bulletCount,
            allBulletCount = this.allBulletCount,
            itemCount = this.itemCount,
            gotItems = new List<string>(this.gotItems),
            gotCharacters = new List<string>(this.gotCharacters),
            highScores = new List<int>(this.highScores),

            // 追加分マッピング
            languageIndex = this.languageIndex,
            globalBgmVol = this.globalBgmVol,
            globalSeVol = this.globalSeVol,
        };

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SaveFilePath, json);
            Debug.Log($"Game saved: {SaveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed: {e}");
        }
    }

    /// <summary>ファイルを読み込んでフィールドに反映する</summary>
    public void LoadGame()
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.LogWarning("No save file found, initializing new data.");
            return;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null)
            {
                Debug.LogError("Failed to parse save data.");
                return;
            }

            this.killCount = data.killCount;
            this.allKillCount = data.allKillCount;
            this.score = data.score;
            this.bulletCount = data.bulletCount;
            this.allBulletCount = data.allBulletCount;
            this.itemCount = data.itemCount;

            this.gotItems.Clear();
            this.gotItems.AddRange(data.gotItems);
            this.gotCharacters.Clear();
            this.gotCharacters.AddRange(data.gotCharacters);
            this.highScores.Clear();
            this.highScores.AddRange(data.highScores);

            // 追加分反映
            this.languageIndex = data.languageIndex;
            this.globalBgmVol = data.globalBgmVol;
            this.globalSeVol = data.globalSeVol;

            Debug.Log($"Game loaded: {SaveFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Load failed: {e}");
        }
    }

    // 以下、gotItems / gotCharacters / highScores を操作するためのメソッド例
    public void AddItem(string itemId)
    {
        if (!gotItems.Contains(itemId))
            gotItems.Add(itemId);
    }

    public void AddCharacter(string charId)
    {
        if (!gotCharacters.Contains(charId))
            gotCharacters.Add(charId);
    }

}
