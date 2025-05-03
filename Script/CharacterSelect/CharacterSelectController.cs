using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;

public class CharacterSelectController : MonoBehaviour
{
    [Header("Data")]
    public CharacterData[] characters;

    [Header("UI")]
    public Button[] iconButtons;
    public Transform previewContainer;
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    [Header("Stats UI")]
    public Transform lifeStarContainer;   // LifeStars オブジェクト
    public Transform powerStarContainer;  // PowerStars オブジェクト
    public Transform speedStarContainer;  // SpeedStars オブジェクト
    public GameObject starPrefab;         // StarIcon Prefab

    [Header("BGM")]
    [SerializeField] private AudioClip bgm;
    [SerializeField] private float bgmVol;

    [Header("SE")]
    [SerializeField] private AudioClip se;
    [SerializeField] private float seVol = 1f;

    // 現在何番が選ばれているか
    int currentIndex = -1;
    GameObject currentPreview;

    void Start()
    {
        // ボタンクリック時も OnSelect を呼ぶ
        for (int i = 0; i < iconButtons.Length; i++)
        {
            int idx = i;
            iconButtons[i].onClick.AddListener(() => OnSelect(idx));
        }
        // 最初は 0 番目を明示的に選択
        EventSystem.current.SetSelectedGameObject(iconButtons[0].gameObject);
        // 初回描画のため OnSelect を実行
        OnSelect(0);
        StartBgm();
    }
    void StartBgm(){
        SoundManager.Instance.PlayBGM(bgm, bgmVol);
    }

    void Update()
    {
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
            GameManager.Instance.selectedCharacter = characters[currentIndex];
            SceneManager.LoadScene("Stage1");
        }
    }

    public void OnSelect(int index)
    {
        currentIndex = index;

        // --- プレビューと説明 ---
        if (currentPreview) Destroy(currentPreview);
        currentPreview = Instantiate(characters[index].previewPrefab);
        /*
        currentPreview.transform.localPosition = Vector3.zero;
        currentPreview.transform.localRotation = Quaternion.identity;
        // 必要であればスケールもリセット
        currentPreview.transform.localScale = Vector3.one * 5f;
        */

        descriptionText.text = characters[index].description;
        nameText.text = characters[index].characterName;

        // --- 星アイコンの更新 ---
        UpdateStars(lifeStarContainer, characters[index].life);
        UpdateStars(powerStarContainer, characters[index].power);
        UpdateStars(speedStarContainer, characters[index].speed);

        SoundManager.Instance.PlaySE(se, seVol);
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
