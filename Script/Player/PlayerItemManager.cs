using System.Collections.Generic;
using UnityEngine;

public class PlayerItemManager : MonoBehaviour
{
    // 全ての装備をここに登録する
    [SerializeField] private GameObject itemRoot;
    public GameObject[] items;

    void Awake()
    {
        items = GetChildObjects(itemRoot);
    }

    //アイテムを取った時、初期アイテムの装備時
    public void getItem(string type, float powerMagnification)
    {
        bool isFined = false;
        type = type.ToLower();
        //とったアイテムのtypeと登録している装備（アイテム）のtypeを比較して、同じだったらその装備（アイテム）のgetItemを叩く
        foreach (GameObject item in items)
        {
            IItem iitem = item.gameObject.GetComponent<IItem>();
            if (type == iitem.itemType)
            {
                iitem.getItem(powerMagnification);
                AddItemCount();
                GetNewItem(type);
                isFined = true;
            }
        }
        if (!isFined) Debug.Log("アイテムが見つかりません:" + type);
    }

    //全ての登録されている兵器をoffにする
    public void AllBatteryActiveFalse()
    {
        foreach (GameObject item in items)
        {
            item.gameObject.GetComponent<IItem>().SetActive(false);
        }
    }

    //全ての登録されているBatteryのCoroutineをoffにする
    public void StopAllCoroutine()
    {
        foreach (GameObject item in items)
        {
            item.gameObject.GetComponent<IItem>().StopAllCoroutine();
        }
    }

    void AddItemCount()
    {
        GameManager.Instance.itemCount++;
    }

    void GetNewItem(string type)
    {
        GameManager.Instance.AddNewItemList(type);
    }
    
    /// <summary>
    /// itemRoot の直下にある全ての子オブジェクトを配列で取得する
    /// </summary>
    /// <param name="itemRoot">親となる GameObject</param>
    /// <returns>子オブジェクトの配列</returns>
    public GameObject[] GetChildObjects(GameObject itemRoot)
    {
        int count = itemRoot.transform.childCount;
        GameObject[] children = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            children[i] = itemRoot.transform.GetChild(i).gameObject;
        }
        return children;
    }

}
