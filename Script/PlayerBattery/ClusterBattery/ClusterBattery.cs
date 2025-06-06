using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    public List<GameObject> bulletPool = new List<GameObject>();
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
            CleanupBulletPool();
            Fire(firePoint.transform);
            SoundManager.Instance.PlaySE(bulletSe, bulletSeVolume);
            AddBulletCount();
            yield return new WaitForSeconds(bulletSetInterval);
        }

    }
    private void Fire(Transform firePoint)
    {
        // Y軸の角度を0度または180度に補正
        Vector3 eulerAngles = firePoint.rotation.eulerAngles;
        float currentYAngle = eulerAngles.y;
        float correctedYAngle = getCorrectedYAngle(currentYAngle);
        
        // 補正された回転を作成
        Quaternion correctedRotation = Quaternion.Euler(eulerAngles.x, correctedYAngle, eulerAngles.z);
        
        // X座標を0に固定した位置を作成
        Vector3 correctedPosition = new Vector3(0f, firePoint.position.y, firePoint.position.z);
        
        GameObject newBullet = Instantiate(bullet, correctedPosition, correctedRotation);
        ClusterMainBomb clusterMainBomb = newBullet.GetComponent<ClusterMainBomb>();
        clusterMainBomb.bulletRound = bulletRound;
        clusterMainBomb.damage = damage;
        clusterMainBomb.clusterBattery = this;

    }

    float getCorrectedYAngle(float currentYAngle){
        float diffTo0 = Mathf.Abs(Mathf.DeltaAngle(currentYAngle, 0f));
        float diffTo180 = Mathf.Abs(Mathf.DeltaAngle(currentYAngle, 180f));
        return (diffTo0 <= diffTo180) ? 0f : 180f;
    }

    /// <summary>
    /// bulletPoolからnullまたは破棄されたGameObjectを削除する
    /// </summary>
    private void CleanupBulletPool()
    {
        bulletPool.RemoveAll(bullet => bullet == null);
    }
    
}
