using UnityEngine;

public class ConfigPlayerBullet : MonoBehaviour
{
    [SerializeField]public float damage = 1;
    [SerializeField]public AudioClip hitSe;
    [SerializeField]public float hitSeVolume;

    public float getDamage(){
        return damage;
    }
    public void setDamage(float damageValue){
        damage = damageValue;
    }
}
