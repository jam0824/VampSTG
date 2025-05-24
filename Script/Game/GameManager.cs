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
    public List<string> gotStages = new List<string>();
    public int[] highScores = new int[20];  // 配列に変更

    public int languageIndex = 0;
    public float globalBgmVol = 0.6f;
    public float globalSeVol = 1f;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("言語設定: 0英語/1日本語")] public int languageIndex = 0;
    [Header("選択されているキャラクター")] public CharacterData selectedCharacter;
    [Header("選択されているステージのシーン名")] public string selectedStageSceneName;

    [Header("音量")] public float globalBgmVol = 0.6f;
    public float globalSeVol = 1f;

    [Header("移動範囲")] public float maxZ = 9f;
    public float minZ = -9f;
    public float maxY = 6f;
    public float minY = -4f;

    [Header("Score倍率")]
    public int bulletMagnification = 10;
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
    public List<string> gotItems = new List<string>();  //ロック解除済みのアイテムリスト
    public List<string> gotCharacters = new List<string>(); //ロック解除済みのキャラクターリスト
    public List<string> gotStages = new List<string>(); //ロック解除済みのステージリスト
    public int[] highScores = new int[20];  // 初期サイズは空配列
    [Header("Stageで死んだ回数")]
    public int stageDeadCount = 0;
    [Header("ノーミスボーナス")]
    public int noMissBonus = 100000;
    [Header("死んだときに戻るシーン")]
    public string whenDeathToSceneName = "CharacterSelect";
    [Header("データ系")]
    public ItemDataDB itemDataDB;

    public List<string> stageGetNewItems = new List<string>();   //そのステージでとった新しいアイテムリスト
    public List<string> stageGetNewCharacters = new List<string>();   //そのステージでとった新しいキャラクターリスト

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "saveData.json");
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGame();  // 起動時にロード
        }
        else Destroy(gameObject);
    }

    public void AddScore(float maxHp)
    {
        score += (int)Math.Ceiling(maxHp * scoreMagnification);
    }

    /// <summary>
    /// 1ステージごとにリセットされるデータ
    /// </summary>
    public void ResetStageSaveData()
    {
        killCount = 0;
        score = 0;
        bulletCount = 0;
        itemCount = 0;
        stageDeadCount = 0;
        stageGetNewItems.Clear();
        stageGetNewCharacters.Clear();
        Debug.Log("ステージデータをリセットしました");
    }

    void OnApplicationQuit() { SaveGame(); }
    void OnApplicationPause(bool pauseStatus) { if (pauseStatus) SaveGame(); }

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
            gotStages = new List<string>(this.gotStages),
            highScores = (int[])this.highScores.Clone(),  // 配列を複製して渡す
            languageIndex = this.languageIndex,
            globalBgmVol = this.globalBgmVol,
            globalSeVol = this.globalSeVol
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
            this.gotStages.Clear();
            this.gotStages.AddRange(data.gotStages);

            this.highScores = data.highScores;  // 配列をそのまま代入

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

    /// <summary>
    /// アンロックアイテムに正式追加
    /// </summary>
    /// <param name="itemId"></param>
    public void AddItem(string itemId)
    {
        if (!gotItems.Contains(itemId))
        {
            gotItems.Add(itemId);
            Debug.Log("正式にアイテムリストに追加しました : " + itemId);
        }

    }

    /// <summary>
    /// 一時的にアンロックアイテムに追加する。クリア後に正式に追加される。
    /// </summary>
    /// <param name="type"></param>
    public void AddNewItemList(string type)
    {
        type = type.ToLower();
        if ((!gotItems.Contains(type)) && (!stageGetNewItems.Contains(type)))
        {
            stageGetNewItems.Add(type);
            Debug.Log("Unlockのアイテムリストに追加しました : " + type);
        }
    }

    /// <summary>
    /// 一時的にアンロックキャラクターに追加する。クリア後に正式に追加される。
    /// </summary>
    /// <param name="charId"></param>
    public void AddNewCharacterList(string charId)
    {
        charId = charId.ToLower();
        if ((!gotCharacters.Contains(charId)) && (!stageGetNewCharacters.Contains(charId)))
        {
            stageGetNewCharacters.Add(charId);
            Debug.Log("Unlockのキャラクターリストに追加しました : " + charId);
        }
    }

    /// <summary>
    /// stageGetNewItemsに含まれるアイテムを全てgotItemsに反映させる
    /// </summary>
    public void AddNewItemListToGotItems()
    {
        foreach (string unlockedItem in stageGetNewItems)
        {
            AddItem(unlockedItem);
        }
    }

    /// <summary>
    /// stageGetNewCharactersに含まれるキャラクターを全てgotCharactersに反映させる
    /// </summary>
    public void AddNewCharacterListToGotCharacters()
    {
        foreach (string unlockedCharacter in stageGetNewCharacters)
        {
            AddCharacter(unlockedCharacter);
        }
    }

    public List<string> GetStageGetNewItems()
    {
        return stageGetNewItems;
    }

    public void ClearStageGetNewItems()
    {
        stageGetNewItems.Clear();
    }

    public List<string> GetStageGetNewCharacters()
    {
        return stageGetNewCharacters;
    }

    public void ClearStageGetNewCharacters()
    {
        stageGetNewCharacters.Clear();
    }

    private void AddCharacter(string charId)
    {
        if (!gotCharacters.Contains(charId))
            gotCharacters.Add(charId);
    }

    public void AddStage(string stageId)
    {
        if (!gotStages.Contains(stageId))
            gotStages.Add(stageId);
    }

    /// <summary>
    /// キャラクターがアンロックされているかどうかを返す
    /// </summary>
    /// <param name="characterId"></param>
    /// <returns></returns>
    public bool IsCharacterUnlocked(string characterId)
    {
        bool isUnlocked = false;
        characterId = characterId.ToLower();
        foreach (string character in gotCharacters)
        {
            string tmpCharacter = character.ToLower();
            if (tmpCharacter == characterId)
            {
                isUnlocked = true;
                break;
            }
        }
        return isUnlocked;
    }

    /// <summary>
    /// ステージがアンロックされているかどうかを返す
    /// </summary>
    /// <param name="stageId"></param>
    /// <returns></returns>
    public bool IsStageUnlocked(string stageId)
    {
        bool isUnlocked = false;
        stageId = stageId.ToLower();
        foreach (string stage in gotStages)
        {
            string tmpStage = stage.ToLower();
            if (tmpStage == stageId)
            {
                isUnlocked = true;
                break;
            }
        }
        return isUnlocked;
    }

}
