using System.Collections.Generic;
using UnityEngine;

public class ItemDataDB : MonoBehaviour
{
    [Header("ItemDataのリスト")]
    public List<ItemData> listItemData = new List<ItemData>();
    
    [Header("CharacterDataのリスト")]
    public List<CharacterData> listCharacterData = new List<CharacterData>();

    #region ItemData関連
    /// <summary>
    /// typeからItemDataを返す。
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public ItemData GetItemData(string type){
        type = type.ToLower();
        ItemData returnData = null;
        foreach(ItemData item in listItemData){
            string itemType = item.type.ToLower();
            if(itemType == type){
                returnData = item;
                break;
            }
        }
        if(returnData == null){
            Debug.LogError("ItemDataが見つかりません: " + type);
        }
        return returnData;
    }

    /// <summary>
    /// ItemDataが存在するかチェック
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool HasItemData(string type){
        return GetItemData(type) != null;
    }
    #endregion

    #region CharacterData関連
    /// <summary>
    /// characterIdからCharacterDataを返す。
    /// </summary>
    /// <param name="characterId"></param>
    /// <returns></returns>
    public CharacterData GetCharacterData(string characterId){
        characterId = characterId.ToLower();
        CharacterData returnData = null;
        foreach(CharacterData character in listCharacterData){
            string charId = character.characterId.ToLower();
            if(charId == characterId){
                returnData = character;
                break;
            }
        }
        return returnData;
    }

    /// <summary>
    /// 全てのCharacterDataを取得
    /// </summary>
    /// <returns></returns>
    public List<CharacterData> GetAllCharacterData(){
        return listCharacterData;
    }

    /// <summary>
    /// CharacterDataが存在するかチェック
    /// </summary>
    /// <param name="characterId"></param>
    /// <returns></returns>
    public bool HasCharacterData(string characterId){
        return GetCharacterData(characterId) != null;
    }
    #endregion
}
