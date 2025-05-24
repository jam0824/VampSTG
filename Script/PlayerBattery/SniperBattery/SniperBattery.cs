using UnityEngine;
using System.Collections;
using System.Linq;

public class SniperBattery : BaseBattery
{
    [Header("回転設定")]
    [Tooltip("1秒あたりの回転速度（度/sec）")]
    public float rotationSpeed = 180f;

    [Tooltip("1サイクルあたりの回転角度（度）")]
    public float anglePerStep = 45f;

    [Tooltip("回転後に待機する秒数")]
    public float waitTime = 1f;
    public float WaitTimeDeltaPerLevel = 0.2f;
    public float waitForShot = 0.5f;    //撃つまでに照準さだめるなどの待ち時間

    public override string itemType => "sniper";
    public GameObject bullet;
    public override int batteryLevel { get; set; } = 0;
    [Header("効果音")]
    [SerializeField] private AudioClip bulletSe;
    [SerializeField] private float bulletSeVolume = 0.5f;
    [SerializeField] private AudioClip reloadSe;
    [SerializeField] private float reloadSeVolume = 0.5f;

    public override float powerMagnification { get; set; } = 1f;
    public override ConfigPlayerBullet configPlayerBullet { get; set; }

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
        SetActiveChild("Sniper1");
        StartCoroutine(RotateRoutine());
    }
    void level2(){
        batteryLevel += 1;
        waitTime -= WaitTimeDeltaPerLevel;
    }
    void level3(){
        batteryLevel += 1;
        SetActiveChild("Sniper2");
    }
    void level4(){
        batteryLevel += 1;
        waitTime -= WaitTimeDeltaPerLevel;
    }
    void level5(){
        batteryLevel += 1;
        SetActiveChild("Sniper3");
    }
    void level6(){
        batteryLevel += 1;
        waitTime -= WaitTimeDeltaPerLevel;
    }
    void level7(){
        batteryLevel += 1;
        SetActiveChild("Sniper4");
    }
    void level8(){
        batteryLevel += 1;
        waitTime -= WaitTimeDeltaPerLevel;
    }

    private IEnumerator RotateRoutine()
    {
        while (true)
        {
            float rotated = 0f;
            // anglePerStep 分だけ回転するループ
            while (rotated < anglePerStep)
            {
                // 1フレームでの回転量
                float step = rotationSpeed * Time.deltaTime;
                // 残りが少ない場合は残り角度までに抑える
                if (rotated + step > anglePerStep)
                {
                    step = anglePerStep - rotated;
                }

                // X軸まわりに回転
                transform.Rotate(Vector3.right * step, Space.Self);

                rotated += step;
                yield return null;
            }

            yield return new WaitForSeconds(waitForShot);
            foreach (Transform t in GetChildTransforms())
            {
                Instantiate(bullet, t.position, t.rotation);
            }
            SoundManager.Instance.PlaySE(bulletSe, bulletSeVolume);
            // 指定秒だけ待機
            yield return new WaitForSeconds(waitTime);
            SoundManager.Instance.PlaySE(reloadSe, reloadSeVolume);
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
