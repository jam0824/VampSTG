using System.Collections;
using UnityEngine;
using System.Linq;

public class MissileBattery : BaseBattery
{

    public override string itemType => "missile";
    public override int batteryLevel{get;set;} = 0;

    [SerializeField] public GameObject bullet;
    [Header("ミサイル郡を撃つまで")]
    [SerializeField, Min(0.01f)] float bulletSetInterval = 3f;
    [Header("ミサイル1発1発を撃つ間隔")]
    [SerializeField] float bulletBulletInterval = 0.1f;
    [Header("発射ポイントroot")]
    [SerializeField] GameObject firePoints;
    [Header("効果音")]
    [SerializeField] private AudioClip bulletSe;
    [SerializeField] private float bulletSeVolume = 0.5f;
    public override float powerMagnification{get;set;} = 1f;
    public override ConfigPlayerBullet configPlayerBullet{get;set;}


    public override void getItem(float magnification){
        SetMagnification(magnification);
        switch (batteryLevel)
        {
            case 0:
                level1();
                break;
            case 1:
                level2();
                break;
            case 2:
                level3();
                break;
            case 3:
                level4();
                break;
            case 4:
                level5();
                break;
            case 5:
                level6();
                break;
            case 6:
                level7();
                break;
            case 7:
                level8();
                break;
            default:
                break;
        }
    }
    void level1()
    {
        batteryLevel += 1;
        gameObject.SetActive(true);
        SetDamage();
        StartCoroutine(AutoShoot());
    }
    void level2()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoints, "Battery2");
    }
    void level3()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoints, "Battery3");
    }
    void level4()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoints, "Battery4");
    }
    void level5()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoints, "Battery5");
    }
    void level6()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoints, "Battery6");
    }
    void level7()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoints, "Battery7");
    }
    void level8()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoints, "Battery8");
    }

    bool SetActiveChildwithObject(GameObject targetObject, string childName){
        Transform child = targetObject.transform.Find(childName);
        if (child != null){
            child.gameObject.SetActive(true);
            return true;
        }
        else{
            Debug.Log("子オブジェクトは見つかりませんでした。:" + childName);
            return false;
        }
    }
    protected void SetMagnification(float magnification)
    {
        //攻撃力倍率を取得し、bullet側にセット
        powerMagnification = magnification;
        if (configPlayerBullet == null) configPlayerBullet = bullet.GetComponent<ConfigPlayerBullet>();
        configPlayerBullet.powerMagnification = powerMagnification;
    }


    private IEnumerator AutoShoot(){
        while(true){
            // 子オブジェクトの位置で一斉射撃
            foreach (Transform t in GetChildTransforms())
            {
                GameObject bulletInstance = Instantiate(bullet, t.position, t.rotation);
                bulletInstance.GetComponent<ConfigPlayerBullet>().damage = damage;
                SoundManager.Instance.PlaySE(bulletSe, bulletSeVolume);
                AddBulletCount();
                yield return new WaitForSeconds(bulletBulletInterval);
            }
            yield return new WaitForSeconds(bulletSetInterval);
        }

    }

    private Transform[] GetChildTransforms()
    {
        return firePoints.GetComponentsInChildren<Transform>()
               .Where(t => t != firePoints.transform)
               .ToArray();
    }

}
