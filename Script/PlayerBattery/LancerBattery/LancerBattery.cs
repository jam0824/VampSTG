using System.Collections;
using UnityEngine;
using System.Linq;

public class LancerBattery : BaseBattery
{

    public override string itemType => "lance";
    public override int batteryLevel { get; set; } = 0;

    [Header("子オブジェクトroot")]
    [SerializeField] private GameObject firePoint;

    [Header("回転設定")]
    [Tooltip("X 軸周りの回転速度（度/秒）")]
    public float rotationSpeed = 360f;

    [Tooltip("1 周回転後に待機する時間（秒）")]
    public float restTime = 1.0f;

    [Header("効果音")]
    [SerializeField] private AudioClip bulletSe;
    [SerializeField] private float bulletSeVolume = 0.5f;
    public override float powerMagnification { get; set; } = 1f;
    public override ConfigPlayerBullet configPlayerBullet { get; set; }
    private PlayerManager playerManager;

    private Transform[] lances; //それぞれのランスのTransformが入っている


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
        SetDamage();
        playerManager = GameObject.FindWithTag("Core").GetComponent<PlayerManager>();
        lances = GetChildTransforms(firePoint);
        DisplayLance(firePoint, "Battery1");
        StartCoroutine(RotateLoop());
    }
    void level2()
    {
        batteryLevel += 1;
        DisplayLance(firePoint, "Battery2");
    }
    void level3()
    {
        batteryLevel += 1;
        ChangeScale(1.1f);
        
    }
    void level4()
    {
        batteryLevel += 1;
        ChangeScale(1.2f);
    }
    void level5()
    {
        batteryLevel += 1;
        ChangeScale(1.3f);
    }
    void level6()
    {
        batteryLevel += 1;
        ChangeScale(1.4f);
    }
    void level7()
    {
        batteryLevel += 1;
        ChangeScale(1.5f);
    }
    void level8()
    {
        batteryLevel += 1;
        ChangeScale(1.6f);
    }

    Transform SetActiveChildwithObject(GameObject targetObject, string childName)
    {
        Transform child = targetObject.transform.Find(childName);
        if (child != null)
        {
            child.gameObject.SetActive(true);
            return child;
        }
        else
        {
            Debug.Log("子オブジェクトは見つかりませんでした。:" + childName);
            return null;
        }
    }
    /// <summary>
    /// ランスを表示し、攻撃倍率を設定する
    /// </summary>
    /// <param name="firePoint"></param>
    /// <param name="objectName"></param>
     void DisplayLance(GameObject firePoint, string objectName)
    {
        Transform lance = SetActiveChildwithObject(firePoint, objectName);
        var configPlayerBullet = lance.gameObject.GetComponent<ConfigPlayerBullet>();
        configPlayerBullet.damage = damage;
        configPlayerBullet.powerMagnification = playerManager.powerMagnification;
    }

    /// <summary>
    /// 回転と休止を繰り返すコルーチン
    /// </summary>
    private IEnumerator RotateLoop()
    {
        while (true)
        {
            ToggleLanceColliders(lances, true); //当たり判定ON
            SoundManager.Instance.PlaySE(bulletSe, bulletSeVolume);
            yield return StartCoroutine(RotateOneFullTurn());
            ToggleLanceColliders(lances, true); //当たり判定OFF
            yield return new WaitForSeconds(restTime);
        }
    }

    /// <summary>
    /// X 軸周りに正確に 360 度回転させるコルーチン
    /// </summary>
    private IEnumerator RotateOneFullTurn()
    {
        float rotated = 0f;
        while (rotated < 360f)
        {
            float step = rotationSpeed * Time.deltaTime;
            if (rotated + step > 360f)
                step = 360f - rotated;

            // ローカル X 軸周りに回転を適用
            transform.Rotate(step, 0f, 0f, Space.Self);
            rotated += step;

            yield return null; // 次のフレームまで待機
        }
    }

    /// <summary>
    /// サイズを変更する
    /// </summary>
    /// <param name="magnification"></param>
    private void ChangeScale(float magnification)
    {
        transform.localScale = Vector3.one * magnification;
    }

    /// <summary>
    /// lances 配列に含まれる各 GameObject の Collider を
    /// 引数で指定した状態に切り替えます。
    /// </summary>
    /// <param name="isEnabled">true: Collider を有効にする ／ false: 無効にする</param>
    private void ToggleLanceColliders(Transform[] lances, bool isEnabled)
    {
        foreach (var t in lances)
        {
            // 3D 用 Collider を取得
            var col = t.gameObject.GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = isEnabled;
            }
            else
            {
                Debug.LogWarning($"{t.name} に Collider がアタッチされていません");
            }
        }
    }

    private Transform[] GetChildTransforms(GameObject firePoint)
    {
        return firePoint.GetComponentsInChildren<Transform>()
               .Where(t => t != firePoint.transform)
               .ToArray();
    }

}
