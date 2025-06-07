using UnityEngine;

public abstract class BaseBattery : MonoBehaviour, IItem
{
    [Header("ItemData")]
    [SerializeField] private ItemData itemData;
    public abstract string itemType{get;}
    public abstract int batteryLevel{get; set;}
    public abstract void getItem();
    public abstract ConfigPlayerBullet configPlayerBullet{get;set;}
    public float damage{get;set;}


    protected bool SetActiveChild(string childName){
        Transform child = transform.Find(childName);
        if (child != null){
            child.gameObject.SetActive(true);
            return true;
        }
        else{
            Debug.Log("子オブジェクトは見つかりませんでした。:" + childName);
            return false;
        }
    }

    public bool SetActive(bool isActive){
        gameObject.SetActive(isActive);
        return gameObject.activeSelf;
    }

    public bool StopAllCoroutine(){
        StopAllCoroutines();
        return true;
    }

    public void AddBulletCount(){
        GameManager.Instance.bulletCount++;
        GameManager.Instance.allBulletCount++;
    }
    protected void SetDamage(){
        damage = itemData.damage;
    }
}
