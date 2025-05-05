using UnityEngine;

public class PlayerItemManager : MonoBehaviour
{
    // 全ての装備をここに登録する
    [SerializeField] protected GameObject[] items;

    //アイテムを取った時、初期アイテムの装備時
    public void getItem(string type, float powerMagnification){
        bool isFined = false;
        type = type.ToLower();
        //とったアイテムのtypeと登録している装備（アイテム）のtypeを比較して、同じだったらその装備（アイテム）のgetItemを叩く
        foreach(GameObject item in items){
            IItem iitem = item.gameObject.GetComponent<IItem>();
            if(type == iitem.itemType){
                iitem.getItem(powerMagnification);
                AddItemCount();
                isFined = true;
            }
        }
        if(!isFined) Debug.Log("アイテムが見つかりません:" + type);
    }

    //全ての登録されている兵器をoffにする
    public void AllBatteryActiveFalse(){
        foreach(GameObject item in items){
            item.gameObject.GetComponent<IItem>().SetActive(false);
        }
    }

    //全ての登録されているBatteryのCoroutineをoffにする
    public void StopAllCoroutine(){
        foreach(GameObject item in items){
            item.gameObject.GetComponent<IItem>().StopAllCoroutine();
        }
    }

    void AddItemCount(){
        GameManager.Instance.itemCount++;
    }

}
