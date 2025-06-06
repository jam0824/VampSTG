using UnityEngine;
using System.Collections;

public class ClusterBattery : BaseBattery
{
    public override string itemType => "cluster";
    public override int batteryLevel { get; set; } = 0;

    [Header("クラスター爆弾")]
    [SerializeField] public GameObject bullet;
    [Header("クラスター爆弾を撃つまで")]
    [SerializeField, Min(0.01f)] float bulletSetInterval = 3f;
    [Header("クラスター爆弾の1回あたりの発射数")]
    [SerializeField] int bulletRound = 1;

    [Header("発射ポイントroot")]
    [SerializeField] GameObject firePoint;
    [Header("効果音")]
    [SerializeField] private AudioClip bulletSe;
    [SerializeField] private float bulletSeVolume = 0.5f;
    public override ConfigPlayerBullet configPlayerBullet { get; set; }
    private PlayerManager playerManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

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
        gameObject.SetActive(true);
        SetDamage();
        playerManager = GameObject.FindWithTag("Core").GetComponent<PlayerManager>();
        StartCoroutine(AutoShoot());
    }
    void level2()
    {
        batteryLevel += 1;
        bulletRound += 1;
    }
    void level3()
    {
        batteryLevel += 1;
        bulletRound += 1;
    }
    void level4()
    {
        batteryLevel += 1;
        bulletRound += 1;
    }
    void level5()
    {
        batteryLevel += 1;
        bulletRound += 1;
    }
    void level6()
    {
        batteryLevel += 1;
        bulletRound += 1;
    }
    void level7()
    {
        batteryLevel += 1;
        bulletRound += 1;
    }
    void level8()
    {
        batteryLevel += 1;
        bulletRound += 1;
    }

    private IEnumerator AutoShoot()
    {
        while (true)
        {
            Fire(firePoint.transform);
            SoundManager.Instance.PlaySE(bulletSe, bulletSeVolume);
            AddBulletCount();
            yield return new WaitForSeconds(bulletSetInterval);
        }

    }
    private void Fire(Transform firePoint)
    {
        // Y軸を軸にして90度回転させた方向で発射
        /*
        Quaternion rotationWithOffset = firePoint.rotation * Quaternion.Euler(0, -90, 0);
        GameObject newBullet = Instantiate(bullet, firePoint.position, rotationWithOffset);
        */
        GameObject newBullet = Instantiate(bullet, firePoint.position, firePoint.rotation);
        ClusterMainBomb clusterMainBomb = newBullet.GetComponent<ClusterMainBomb>();
        clusterMainBomb.bulletRound = bulletRound;
        clusterMainBomb.damage = damage;

    }
}
