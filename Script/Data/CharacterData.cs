using UnityEngine;

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    [Header("名前:0英語1日本語")]
    public string[] characterName;
    [Header("プレビュー用のモデル")]
    public GameObject previewPrefab;      // 3D モデルのプレハブ
    [Header("ゲーム中で使うモデル")]
    public GameObject playModel;
    [Header("モデルにCore配置のオフセット")]
    public Vector3 playerModelOffset;
    [Header("パラメーター")]
    public int life;
    public int power;
    public int speed;
    public int pickupRange;
    [Header("初期装備")]
    public ItemData initialItemData;
    [Header("説明:0英語1日本語")]
    [TextArea] public string[] description; // 説明テキスト
}