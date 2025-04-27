using UnityEngine;

public class PlayerItemManager : MonoBehaviour
{
    // 全ての装備をここに登録する
    [SerializeField] protected GameObject[] items;

    public void getItem(string type){
        bool isFined = false;
        type = type.ToLower();
        //とったアイテムのtypeと登録している装備（アイテム）のtypeを比較して、同じだったらその装備（アイテム）のgetItemを叩く
        foreach(GameObject item in items){
            IItem iitem = item.gameObject.GetComponent<IItem>();
            if(type == iitem.itemType){
                iitem.getItem();
                isFined = true;
            }
        }
        if(!isFined) Debug.Log("アイテムが見つかりません:" + type);

        
    }



}
