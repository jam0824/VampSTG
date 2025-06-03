using UnityEngine;
using System.Collections.Generic;
public class ConfigPlayerBullet : MonoBehaviour
{
    [SerializeField]public float damage = 1;
    [SerializeField]public AudioClip hitSe;
    [SerializeField]public float hitSeVolume;
    [Header("衝突時のエフェクト")]
    [SerializeField] public GameObject triggerEffect;
    [Header("敵とぶつかった後に消すか")]
    [SerializeField]public bool isDestroy = true;
    [Header("ぶつかった敵の登録")]
    [SerializeField]public List<GameObject> hitEnemyList = new List<GameObject>();
    public float powerMagnification = 1f;

    public float getDamage(){
        return damage * GameManager.Instance.powerMagnification;
    }
    public void setDamage(float damageValue){
        damage = damageValue;
    }
    /// <summary>
    /// 貫通系の場合複数回ダメージをウケないようにするため敵を登録しておく
    /// </summary>
    /// <param name="enemy"></param>
    public void addHitEnemy(GameObject enemy){
        hitEnemyList.Add(enemy);
    }
    /// <summary>
    /// 自分が登録されているか
    /// </summary>
    /// <param name="enemy"></param>
    public bool isHitEnemy(GameObject enemy){
        return hitEnemyList.Contains(enemy);
    }
    
    /// <summary>
    /// ヒット済み敵リストをクリアする
    /// </summary>
    public void clearHitEnemyList(){
        hitEnemyList.Clear();
    }

    //カメラに映らなくなった瞬間に呼ばれる
    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
