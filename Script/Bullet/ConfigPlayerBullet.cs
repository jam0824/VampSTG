using UnityEngine;

public class ConfigPlayerBullet : MonoBehaviour
{
    [SerializeField]public float damage = 1;
    [SerializeField]public AudioClip hitSe;
    [SerializeField]public float hitSeVolume;
    [Header("衝突時のエフェクト")]
    [SerializeField] public GameObject triggerEffect;
    [Header("敵とぶつかった後に消すか")]
    [SerializeField]public bool isDestroy = true;
    public float powerMagnification = 1f;

    public float getDamage(){
        return damage * powerMagnification;
    }
    public void setDamage(float damageValue){
        damage = damageValue;
    }

    //カメラに映らなくなった瞬間に呼ばれる
    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
