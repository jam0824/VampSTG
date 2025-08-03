using UnityEngine;
using System.Collections;

public class SmineBattery : BaseBattery
{

    public override string itemType => "smine";
    public override int batteryLevel { get; set; } = 0;

    [Header("地雷")]
    [SerializeField] public GameObject sminePrefab;
    [SerializeField] public GameObject smineMuzzlePrefab;
    [Header("地雷郡を撃つまで")]
    [SerializeField, Min(0.01f)] float bulletSetInterval = 5f;
    [Header("地雷1発1発を撃つ間隔")]
    [SerializeField] float bulletBulletInterval = 1f;
    [Header("発射ポイントroot")]
    [SerializeField] GameObject firePoint;
    
    [Header("効果音")]
    [SerializeField] private AudioClip bulletSe;
    [SerializeField] private float bulletSeVolume = 0.5f;

    public override ConfigPlayerBullet configPlayerBullet { get; set; }

    public int smineMaxSetCount = 0;   //地雷の現在設置できる数



    public override void getItem()
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
        smineMaxSetCount += 1;
        gameObject.SetActive(true);
        SetDamage();
        StartCoroutine(AutoShoot());
    }
    void level2()
    {
        batteryLevel += 1;
        smineMaxSetCount += 1;
    }
    void level3()
    {
        batteryLevel += 1;
        smineMaxSetCount += 1;
    }
    void level4()
    {
        batteryLevel += 1;
        smineMaxSetCount += 1;
    }
    void level5()
    {
        batteryLevel += 1;
        smineMaxSetCount += 1;
    }
    void level6()
    {
        batteryLevel += 1;
        smineMaxSetCount += 1;
    }
    void level7()
    {
        batteryLevel += 1;
        smineMaxSetCount += 1;
    }
    void level8()
    {
        batteryLevel += 1;
        smineMaxSetCount += 1;
    }


    private IEnumerator AutoShoot()
    {
        while (true)
        {
            // 一斉地雷設置
            for(int i = 0; i < smineMaxSetCount; i++)
            {
                Fire(firePoint.transform);
                MakeMuzzle(smineMuzzlePrefab, firePoint.transform);
                SoundManager.Instance.PlaySE(bulletSe, bulletSeVolume);
                AddBulletCount();
                yield return new WaitForSeconds(bulletBulletInterval);
            }
            yield return new WaitForSeconds(bulletSetInterval);
        }

    }

    void Fire(Transform firePointTransform)
    {
        // プレハブ生成
        MakeBullet(sminePrefab, firePointTransform, damage);
    }

    GameObject MakeBullet(GameObject sminePrefab, Transform firePointTransform, float damage)
    {
        var newBullet = EffectController.Instance.PlayPlayerBullet(sminePrefab, firePointTransform.position, Quaternion.identity);
        var smine = newBullet.GetComponent<Smine>();
        smine.damage = damage;
        return newBullet;
    }

    GameObject MakeMuzzle(GameObject smineMuzzlePrefab, Transform firePointTransform)
    {
        // 現在の回転を取得してX軸を逆にする
        Vector3 euler = firePointTransform.rotation.eulerAngles;
        euler.x = -euler.x + 180f; // X軸を逆回転
        Quaternion modifiedRotation = Quaternion.Euler(euler);

        var newMuzzle = EffectController.Instance.PlayEffect(smineMuzzlePrefab, firePointTransform.position, modifiedRotation);
        return newMuzzle;
    }

}
