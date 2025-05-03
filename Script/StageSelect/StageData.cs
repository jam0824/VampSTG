using UnityEngine;

[CreateAssetMenu]
public class StageData : ScriptableObject
{
    public string[] missionTitle;
    [Header("遷移するシーン")]
    public string sceneName;

    [Header("プレビュー用の画像")]
    public Sprite previewImage;

    [Header("パラメーター")]
    public int difficulty;
    [TextArea] public string[] description; // 説明テキスト
}