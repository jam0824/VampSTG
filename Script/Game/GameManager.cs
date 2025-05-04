using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("言語設定: 0英語/1日本語")]
    public int languageIndex = 0;
    [Header("選択されているキャラクター")]
    public CharacterData selectedCharacter;

    [Header("音量")]
    public float globalBgmVol = 0.6f;
    public float globalSeVol = 1f;

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
