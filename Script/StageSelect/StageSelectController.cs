using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;

public class StageSelectController : MonoBehaviour
{
    [Header("Data")]
    public StageData[] stages;

    [Header("UI")]
    public Button[] iconButtons;
    public Image missionImage;
    public TMP_Text missionText;
    public TMP_Text descriptionText;

    [Header("Stats UI")]
    public Transform difficultyStarContainer;   // LifeStars オブジェクト
    public GameObject starPrefab;         // StarIcon Prefab

    [Header("BGM")]
    [SerializeField] private AudioClip bgm;
    [SerializeField] private float bgmVol;

    [Header("SE")]
    [SerializeField] private AudioClip se;
    [SerializeField] private AudioClip decisionSe;
    [SerializeField] private float seVol = 1f;

    // 現在何番が選ばれているか
    int currentIndex = -1;
    GameManager gm;
    bool isFirstTime = true;

    void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        // 決定時はConfirmSelectionを呼ぶ
        for (int i = 0; i < iconButtons.Length; i++)
        {
            int idx = i;
            iconButtons[i].onClick.AddListener(() => ConfirmSelection(idx));
        }
        // 最初は 0 番目を明示的に選択
        EventSystem.current.SetSelectedGameObject(iconButtons[0].gameObject);
        // 初回描画のため OnSelect を実行
        OnSelect(0);
    }
    void StartBgm(){
        SoundManager.Instance.PlayBGM(bgm, bgmVol);
    }

    void Update()
    {
        //GameManagerが見つからなかったら探す。それでも見つからなかったら何もしない
        if(gm == null) gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        if(gm == null) return;

        // 1) フォーカス変更を検知して自動的に OnSelect
        var selected = EventSystem.current.currentSelectedGameObject;
        int newIndex = Array.FindIndex(iconButtons, b => b.gameObject == selected);
        if (newIndex >= 0 && newIndex != currentIndex)
        {
            OnSelect(newIndex);
        }

        // 2) 決定ボタンで次のシーンへ
        if (Input.GetButtonDown("Submit"))
        {
            ConfirmSelection(currentIndex);
        }
    }

    public void OnSelect(int index)
    {
        currentIndex = index;

        // --- プレビューと説明 ---

        descriptionText.text = stages[index].description[gm.languageIndex];
        missionText.text = stages[index].missionTitle[gm.languageIndex];
        if(stages[index].previewImage != null) missionImage.sprite = stages[index].previewImage;

        // --- 星アイコンの更新 ---
        UpdateStars(difficultyStarContainer, stages[index].difficulty);
        if(!isFirstTime) SoundManager.Instance.PlaySE(se, seVol);
        else isFirstTime = false;
    }

    void ConfirmSelection(int index)
    {
        SoundManager.Instance.PlaySE(decisionSe, seVol);
        currentIndex = index;
        string sceneName = stages[currentIndex].sceneName;
        GameManager.Instance.selectedStageSceneName = sceneName;
        SoundManager.Instance.StopBGMWithFadeOut(0.5f);
        FadeManager.FadeToScene("ChangeToStage", FadeColor.Black);
    }

    // 指定したコンテナに starPrefab を count 個並べる
    void UpdateStars(Transform container, int count)
    {
        // 1) 既存の星を全削除
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        // 2) count 個の星を生成
        for (int i = 0; i < count; i++)
        {
            Instantiate(starPrefab, container);
        }
    }
}
