using UnityEngine;

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    public string characterName;
    [Header("プレビュー用のモデル")]
    public GameObject previewPrefab;      // 3D モデルのプレハブ
    [Header("ゲーム中で使うモデル")]
    public GameObject playModel;
    [Header("パラメーター")]
    public int life;
    public int power;
    public int speed;
    [TextArea] public string description; // 説明テキスト
}