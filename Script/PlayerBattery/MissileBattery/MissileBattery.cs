using System.Collections;
using UnityEngine;
using System.Linq;

public class MissileBattery : BaseBattery
{

    public override string itemType => "missile";
    public override int batteryLevel{get;set;} = 0;

    [SerializeField] GameObject bullet = null;
    [SerializeField, Min(1)]
    int spawnCount = 1;
    [Header("ミサイル郡を撃つまで")]
    [SerializeField, Min(0.01f)] float bulletSetInterval = 3f;
    [Header("ミサイル1発1発を撃つ間隔")]
    [SerializeField] float bulletBulletInterval = 0.1f;
    [Header("発射ポイントroot")]
    [SerializeField] GameObject firePoints;
    [Header("効果音")]
    [SerializeField] private AudioClip bulletSe;
    [SerializeField] private float bulletSeVolume = 0.5f;

    bool isSpawning = false;
    Transform thisTransform;
    WaitForSeconds spawnWait;

	void Start()
	{
        thisTransform = transform;
        StartCoroutine(AutoShoot());
	}

	void Update()
    {
		
    }
    public override void getItem(float magnification){
    }

    private IEnumerator AutoShoot(){
        while(true){
            // 子オブジェクトの位置で一斉射撃
            int i = 0;
            foreach (Transform t in GetChildTransforms())
            {
                Instantiate(bullet, t.position, t.rotation);
                SoundManager.Instance.PlaySE(bulletSe, bulletSeVolume);
                i++;
                if(i == spawnCount) break;
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
