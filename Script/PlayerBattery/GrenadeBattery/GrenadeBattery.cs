using System.Collections;
using UnityEngine;
using System.Linq;

public class GrenadeBattery : BaseBattery
{

    public override string itemType => "grenade";
    public override int batteryLevel { get; set; } = 0;

    [SerializeField] public GameObject bullet;
    [Header("グレネード郡を撃つまで")]
    [SerializeField, Min(0.01f)] float bulletSetInterval = 3f;
    [Header("グレネード1発1発を撃つ間隔")]
    [SerializeField] float bulletBulletInterval = 0.1f;
    [Header("発射ポイントroot")]
    [SerializeField] GameObject firePoint;
    [Header("発射設定")]
    [SerializeField] float launchSpeed = 15f;    // 初速
    [SerializeField, Range(0, 89)] float angle = 45f; // 発射角度（度数法）
    [Header("効果音")]
    [SerializeField] private AudioClip bulletSe;
    [SerializeField] private float bulletSeVolume = 0.5f;
    public override float powerMagnification { get; set; } = 1f;
    public override ConfigPlayerBullet configPlayerBullet { get; set; }
    private PlayerManager playerManager;



    public override void getItem(float magnification)
    {
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
        playerManager = GameObject.FindWithTag("Core").GetComponent<PlayerManager>();
        SetActiveChildwithObject(firePoint, "Battery1");
        StartCoroutine(AutoShoot());
    }
    void level2()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoint, "Battery2");
    }
    void level3()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoint, "Battery3");
    }
    void level4()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoint, "Battery4");
    }
    void level5()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoint, "Battery5");
    }
    void level6()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoint, "Battery6");
    }
    void level7()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoint, "Battery7");
    }
    void level8()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoint, "Battery8");
    }

    bool SetActiveChildwithObject(GameObject targetObject, string childName)
    {
        Transform child = targetObject.transform.Find(childName);
        if (child != null)
        {
            child.gameObject.SetActive(true);
            return true;
        }
        else
        {
            Debug.Log("子オブジェクトは見つかりませんでした。:" + childName);
            return false;
        }
    }


    private IEnumerator AutoShoot()
    {
        while (true)
        {
            // 子オブジェクトの位置で一斉射撃
            foreach (Transform t in GetChildTransforms())
            {
                Fire(t);
                SoundManager.Instance.PlaySE(bulletSe, bulletSeVolume);
                AddBulletCount();
                yield return new WaitForSeconds(bulletBulletInterval);
            }
            yield return new WaitForSeconds(bulletSetInterval);
        }

    }

    void Fire(Transform childTransform)
    {
        if(playerManager == null) 
            playerManager = GameObject.FindWithTag("Core").GetComponent<PlayerManager>();
        // プレハブ生成
        var grenade = MakeBullet(bullet, childTransform);
        var rb = grenade.GetComponent<Rigidbody>();

        // Rigidbody の重力を有効化（Inspector でチェック済みなら不要）
        rb.useGravity = true;

        // 角度をラジアンに変換
        float rad = angle * Mathf.Deg2Rad;
        float vy = launchSpeed * Mathf.Sin(rad);   // ローカル Y 成分
        float vz = launchSpeed * Mathf.Cos(rad);   // ローカル Z 成分

        // ローカル空間の速度ベクトル
        Vector3 localVelocity = new Vector3(0, vy, vz);

        // ワールド空間に変換して速度をセット
        rb.linearVelocity = firePoint.transform.TransformDirection(localVelocity);

        // もし瞬間的に速度を変えたいならこちらでもOK
        // rb.AddForce(firePoint.transform.TransformDirection(localVelocity), 
        //             ForceMode.VelocityChange);
    }

    GameObject MakeBullet(GameObject bullet, Transform childTransform)
    {
        var newBullet = Instantiate(bullet, childTransform.position, childTransform.rotation);
        var configPlayerBullet = newBullet.GetComponent<ConfigPlayerBullet>();
        configPlayerBullet.powerMagnification = playerManager.powerMagnification;
        return newBullet;
    }

    private Transform[] GetChildTransforms()
    {
        return firePoint.GetComponentsInChildren<Transform>()
               .Where(t => t != firePoint.transform)
               .ToArray();
    }

}
