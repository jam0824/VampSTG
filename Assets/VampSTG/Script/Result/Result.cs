using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;    // ← 追加
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Result : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text textScore;
    public TMP_Text textKill;
    public TMP_Text textShots;
    public TMP_Text textItem;
    public TMP_Text textTotalScore;
    public GameObject noMissGameObject;
    [Header("アンロックアイテム")]
    public Transform unlockContainer;
    public GameObject unlockItemImagePrefab;
    [Header("アンロックステージ")]
    public UnlockManager unlockManager;

    [Header("次のスコアを表示するまでの時間")]
    public float displayInterval = 0.5f;

    [Header("効果音")]
    [SerializeField] private AudioClip scoreDisplaySe;
    [SerializeField] private AudioClip noMissSe;
    [SerializeField] private float SeVolume = 1f;
    [Header("BGM")]
    [SerializeField] private AudioClip bgm;
    [SerializeField] private float bgmVol = 0.5f;

    private int totalScore = 0;

    // コルーチン制御用
    private Coroutine displayCoroutine;
    private bool isDisplaying = false;

    void Start()
    {
        // コルーチンを開始して参照を保持
        displayCoroutine = StartCoroutine(DisplayResult());
        SoundManager.Instance.PlayBGM(bgm, bgmVol);
        UnlockItems();  //新しく取得したアイテムをアンロックしてしまう
        UnlockCharacter(); //新しく取得したキャラクターをアンロックしてしまう   
        unlockManager.UnlockStage(GameManager.Instance.selectedStage.sceneName); //シーンネームを送ることで次のステージをUnlock
        DisplayUnlockItem(unlockContainer); //アンロックアイテムを並べる
        DisplayUnlockCharacter(unlockContainer); //アンロックキャラクターを並べる
    }

    void UnlockItems(){
        GameManager.Instance.AddNewItemListToGotItems();
    }
    void UnlockCharacter(){
        GameManager.Instance.AddNewCharacterListToGotCharacters();
    }

    void Update()
    {
        // 決定（Submit）または Z キーが押されたら……
        if (Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.Z))
        {
            if (isDisplaying)
            {
                // 1) まだ演出中 → コルーチン停止して一気に表示
                StopCoroutine(displayCoroutine);
                ShowAllResultsImmediately();
                isDisplaying = false;
            }
            else
            {
                GameManager.Instance.SaveGame();
                // 2) 演出完了後 → シーン切り替え
                FadeManager.FadeToScene("CharacterSelect", FadeColor.Black);
            }
        }
    }

    /// <summary>
    /// リザルト表示演出
    /// </summary>
    private IEnumerator DisplayResult()
    {
        isDisplaying = true;
        totalScore = 0;

        yield return new WaitForSeconds(1f);

        // Score
        textScore.gameObject.SetActive(true);
        textScore.text = DisplayScore(GameManager.Instance.score);
        SoundManager.Instance.PlaySE(scoreDisplaySe, SeVolume);
        yield return new WaitForSeconds(displayInterval);

        // Kill
        textKill.gameObject.SetActive(true);
        textKill.text = DisplayCalc(GameManager.Instance.killCount, (int)GameManager.Instance.scoreMagnification);
        SoundManager.Instance.PlaySE(scoreDisplaySe, SeVolume);
        yield return new WaitForSeconds(displayInterval);

        // Shots
        textShots.gameObject.SetActive(true);
        textShots.text = DisplayCalc(GameManager.Instance.bulletCount, GameManager.Instance.bulletMagnification);
        SoundManager.Instance.PlaySE(scoreDisplaySe, SeVolume);
        yield return new WaitForSeconds(displayInterval);

        // Item
        textItem.gameObject.SetActive(true);
        textItem.text = DisplayCalc(GameManager.Instance.itemCount, GameManager.Instance.itemMagnification);
        SoundManager.Instance.PlaySE(scoreDisplaySe, SeVolume);
        yield return new WaitForSeconds(displayInterval);

        // NoMissボーナス
        if (GameManager.Instance.stageDeadCount == 0)
        {
            noMissGameObject.SetActive(true);
            DisplayTotalScore(GameManager.Instance.noMissBonus);
            SoundManager.Instance.PlaySE(noMissSe, SeVolume);
        }

        isDisplaying = false;
    }

    /// <summary>
    /// 中断されたときや一気に表示するときに呼び出す
    /// </summary>
    private void ShowAllResultsImmediately()
    {
        totalScore = 0;

        // Score
        textScore.gameObject.SetActive(true);
        textScore.text = DisplayScore(GameManager.Instance.score);

        // Kill
        textKill.gameObject.SetActive(true);
        textKill.text = DisplayCalc(GameManager.Instance.killCount, (int)GameManager.Instance.scoreMagnification);

        // Shots
        textShots.gameObject.SetActive(true);
        textShots.text = DisplayCalc(GameManager.Instance.bulletCount, GameManager.Instance.bulletMagnification);

        // Item
        textItem.gameObject.SetActive(true);
        textItem.text = DisplayCalc(GameManager.Instance.itemCount, GameManager.Instance.itemMagnification);

        // NoMissボーナス
        if (GameManager.Instance.stageDeadCount == 0)
        {
            noMissGameObject.SetActive(true);
            DisplayTotalScore(GameManager.Instance.noMissBonus);
            SoundManager.Instance.PlaySE(noMissSe, SeVolume);
        }
        else{
            SoundManager.Instance.PlaySE(scoreDisplaySe, SeVolume);
        }
    }

    private string DisplayScore(int score)
    {
        string formattedScore = score.ToString("N0");
        DisplayTotalScore(score);
        return formattedScore;
    }

    private string DisplayCalc(int count, int magnification)
    {
        int total = count * magnification;
        string formattedCount = count.ToString("N0");
        string formattedMag = magnification.ToString("N0");
        string formattedTotal = total.ToString("N0");
        DisplayTotalScore(total);
        return $"{formattedCount} x {formattedMag} = $ {formattedTotal}";
    }

    private void DisplayTotalScore(int addScore)
    {
        totalScore += addScore;
        string formattedTotalScore = totalScore.ToString("N0");
        textTotalScore.text = $"$ {formattedTotalScore}";
    }

    // 指定したコンテナに itemPrefab を並べる
    void DisplayUnlockItem(Transform container)
    {
        List<string> items = GameManager.Instance.GetStageGetNewItems();
        foreach (string itemType in items)
        {
            ItemData itemData = GameManager.Instance.itemDataDB.GetItemData(itemType);
            if(itemData == null){
                Debug.Log("ItemTypeに合うItemDataがありません : " + itemType);
                continue;
            }
            GameObject item = Instantiate(unlockItemImagePrefab, container);
            item.GetComponent<Image>().sprite = itemData.itemSprite;
        }
    }

    void DisplayUnlockCharacter(Transform container)
    {
        List<string> characters = GameManager.Instance.GetStageGetNewCharacters();
        foreach (string characterId in characters)
        {
            CharacterData characterData = GameManager.Instance.itemDataDB.GetCharacterData(characterId);
            if(characterData == null){
                Debug.Log("CharacterIdに合うCharacterDataがありません : " + characterId);
                continue;
            }
            GameObject character = Instantiate(unlockItemImagePrefab, container);
            character.GetComponent<Image>().sprite = characterData.characterSprite;
        }
    }
}
