using UnityEngine;

[CreateAssetMenu]
public class ItemData : ScriptableObject
{
    [Header("アイテム名")]
    [SerializeField] public string[] itemName;
    [Header("判定用type")]
    [SerializeField] public string type;
    [Header("アイテムのゲームオブジェクト")]
    [SerializeField] public GameObject itemObj;
    [Header("アイテムのイメージ")]
    [SerializeField] public Sprite itemSprite;
    [Header("アイテムのパ欄メーター")]
    
    [SerializeField] public float damage = 1f;
    
    [Header("説明:0英語1日本語")]
    [TextArea] public string[] description; // 説明テキスト
}
