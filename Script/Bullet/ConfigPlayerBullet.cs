using UnityEngine;

public class ConfigPlayerBullet : MonoBehaviour
{
    [SerializeField]public int damage = 1;
    [SerializeField]public AudioClip hitSe;
    [SerializeField]public float hitSeVolume;

    public int getDamage(){
        return damage;
    }
    public void setDamage(int damageValue){
        damage = damageValue;
    }
}
