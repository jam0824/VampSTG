using System.Collections.Generic;
using UnityEngine;

public class ItemDataDB : MonoBehaviour
{
    [Header("ItemDataのリスト")]
    public List<ItemData> listItemData = new List<ItemData>();

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
        return returnData;
    }
}
