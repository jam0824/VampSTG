using UnityEngine;

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public GameObject previewPrefab;      // 3D モデルのプレハブ
    public int life;
    public int power;
    public int speed;
    [TextArea] public string description; // 説明テキスト
}