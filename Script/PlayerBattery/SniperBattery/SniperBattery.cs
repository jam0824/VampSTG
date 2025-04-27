using UnityEngine;
using System.Collections;
using System.Linq;

public class RotateWithPause : MonoBehaviour,IItem
{
    [Header("回転設定")]
    [Tooltip("1秒あたりの回転速度（度/sec）")]
    public float rotationSpeed = 180f;

    [Tooltip("1サイクルあたりの回転角度（度）")]
    public float anglePerStep = 45f;

    [Tooltip("回転後に待機する秒数")]
    public float waitTime = 1f;
    public float WaitTimeDeltaPerLevel = 0.2f;

    public string itemType{get;} = "sniper";
    public GameObject bullet;
    public int batteryLevel{get;set;} = 0;


    public void getItem(){
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

            foreach (Transform t in GetChildTransforms())
            {
                Instantiate(bullet, t.position, t.rotation);
            }
            // 指定秒だけ待機
            yield return new WaitForSeconds(waitTime);
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
    private bool SetActiveChild(string childName){
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
}
