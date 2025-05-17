using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class BitBattery : BaseBattery
{

    public override string itemType => "bit";
    public override int batteryLevel{get;set;} = 0;

    [Header("ターゲット検索")]
    // Coreからのターゲット検索半径
    public float targetRadius = 4f;
    public float maxTargetRadius = 6f;
    public float stopDistance = 2f;
    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float maxMoveSpeed = 15f;
    [Header("画面端からの移動マージン")]
    public float moveOffset = 1f;
    public float rotationSpeed = 90f;
    [Header("射撃設定")]
    public float fireInterval = 2f;

    [Header("発射ポイントroot")]
    [SerializeField] GameObject firePoints;
    public override float powerMagnification{get;set;} = 1f;
    public override ConfigPlayerBullet configPlayerBullet{get;set;}

    public List<GameObject> targetEnemyPool = new List<GameObject>();


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
        SetActiveChildwithObject(firePoints, "PotBit1");
    }
    void level2()
    {
        batteryLevel += 1;
        moveSpeed *= 2;
    }
    void level3()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoints, "PotBit2");
    }
    void level4()
    {
        batteryLevel += 1;
        moveSpeed = maxMoveSpeed;
    }
    void level5()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoints, "PotBit3");
    }
    void level6()
    {
        batteryLevel += 1;
        targetRadius = maxTargetRadius;
    }
    void level7()
    {
        batteryLevel += 1;
        SetActiveChildwithObject(firePoints, "PotBit4");
    }
    void level8()
    {
        batteryLevel += 1;
        fireInterval /= 2;
    }

    /// <summary>
    /// 敵の GameObject をプールに登録します。
    /// 呼び出し時に null エントリを一掃し、重複登録を防ぎます。
    /// </summary>
    /// <param name="enemy">追加したい敵の GameObject</param>
    public void AddEnemy(GameObject enemy)
    {
        // 1) null エントリを削除
        targetEnemyPool.RemoveAll(e => e == null);

        // 2) 渡された enemy が有効かつ未登録なら追加
        if (enemy != null && !targetEnemyPool.Contains(enemy))
        {
            targetEnemyPool.Add(enemy);
        }
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

    private Transform[] GetChildTransforms()
    {
        return firePoints.GetComponentsInChildren<Transform>()
               .Where(t => t != firePoints.transform)
               .ToArray();
    }

}
