using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
public class ConfigPlayerBullet : MonoBehaviour
{
    [SerializeField]public float damage = 1;
    [SerializeField]public AudioClip hitSe;
    [SerializeField]public float hitSeVolume;
    [Header("衝突時のエフェクト")]
    [SerializeField] public GameObject triggerEffect;
    [Header("敵とぶつかった後に消すか")]
    [SerializeField]public bool isDestroy = true;
    [Header("ヒット時に既定の爆発エフェクトを使うか")]
    [SerializeField]public bool isUseDefaultExplosionEffect = false;
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

    private void OnTriggerEnter(Collider other)
    {
        if(isUseDefaultExplosionEffect){
            Explosion(damage);
        }
    }

    private void Explosion(float damage)
    {

        
        if (damage < 20)
        {
            EffectController.Instance.PlaySmallExplosion(transform.position, transform.rotation);
            return;
        }
        if (damage < 50)
        {
            EffectController.Instance.PlayMiddleExplosion(transform.position, transform.rotation);
            return;
        }
        EffectController.Instance.PlayLargeExplosion(transform.position, transform.rotation);
    }

    //カメラに映らなくなった瞬間に呼ばれる
    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
