using UnityEngine;
using System.Collections;
using System.Linq;

public class VulcanCanon : BaseBattery
{
    public override string itemType => "vulcan";
    public GameObject bullet;
    public override int batteryLevel{get;set;} = 0;
    public float bulletInterval = 0.1f;   // 通常の連射間隔

    public int pauseAfterShots = 50;      // 何発撃ったらポーズ
    public float pauseDuration = 1f;      // ポーズ時間（秒）
    public float powerMagnification = 1f;
    [Header("効果音")]
    [SerializeField] private AudioClip bulletSe;
    [SerializeField] private float bulletSeVolume = 0.5f;
    [SerializeField] private AudioClip reloadSe;
    [SerializeField] private float reloadSeVolume = 0.5f;

    private ConfigPlayerBullet configPlayerBullet;

    void Start()
    {
        configPlayerBullet = bullet.GetComponent<ConfigPlayerBullet>();
    }

    void SetMagnification(float magnification){
        //攻撃力倍率を取得し、bullet側にセット
        powerMagnification = magnification;
        if(configPlayerBullet == null) configPlayerBullet = bullet.GetComponent<ConfigPlayerBullet>();
        configPlayerBullet.powerMagnification = powerMagnification;
    }

    public override void getItem(float magnification){
        SetMagnification(magnification);

        switch(batteryLevel){
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
    void level1(){
        batteryLevel += 1;
        StartCoroutine(AutoShoot());
    }
    void level2(){
        batteryLevel += 1;
        SetActiveChild("Battery2");
    }
    void level3(){
        batteryLevel += 1;
        pauseAfterShots += 50;
    }
    void level4(){
        batteryLevel += 1;
        SetActiveChild("Battery3");
    }
    void level5(){
        batteryLevel += 1;
        SetActiveChild("Battery4");
    }
    void level6(){
        batteryLevel += 1;
        pauseAfterShots += 50;
    }
    void level7(){
        batteryLevel += 1;
        SetActiveChild("Battery5");
    }
    void level8(){
        batteryLevel += 1;
        SetActiveChild("Battery6");
        SetActiveChild("Battery7");
    }

    private IEnumerator AutoShoot()
    {
        int bulletCount = 0;

        while (true)
        {
            // 子オブジェクトの位置で一斉射撃
            foreach (Transform t in GetChildTransforms())
            {
                Instantiate(bullet, t.position, t.rotation);
            }
            SoundManager.Instance.PlaySE(bulletSe, bulletSeVolume);
            bulletCount++;

            // 50発撃ったら「1秒ポーズ」、それ以外は通常インターバル
            if (bulletCount >= pauseAfterShots)
            {
                SoundManager.Instance.PlaySE(reloadSe, reloadSeVolume);
                yield return new WaitForSeconds(pauseDuration);
                bulletCount = 0;  // カウントリセット
            }
            else
            {
                yield return new WaitForSeconds(bulletInterval);
            }
        }
    }

    /// <summary>
    /// このオブジェクト配下のすべての子 Transform（自分自身を除く）を返します
    /// </summary>
    private Transform[] GetChildTransforms()
    {
        return GetComponentsInChildren<Transform>()
               .Where(t => t != transform)
               .ToArray();
    }
}
