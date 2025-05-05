using UnityEngine;
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

    public GameObject playerCore;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
}
