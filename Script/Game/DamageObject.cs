using UnityEngine;

public class DamageObject : MonoBehaviour
{
    [SerializeField] public float hp = 3;
    [SerializeField] private float offsetExplosionY = 0;
    private float maxHp = 10;
    bool isDead = false;
    int fromBossDamage = 5; //敵キャラがボスにあたった時のダメージ

    

    void Start()
    {
        maxHp = hp;
    }

    void Update()
    {
        if ((!isDead) && (hp <= 0)) enemyDie();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        if (other.CompareTag("Boss"))
        {
            hp -= fromBossDamage;
        }
        else if (other.CompareTag("PlayerBullet"))
        {
            if (!other.TryGetComponent<ConfigPlayerBullet>(out var bullet)) return;
            hp = hit(bullet, hp);
            // 近似的に当たり位置を計算
            Vector3 hitPoint = other.ClosestPoint(transform.position);

            if (bullet.triggerEffect != null)
            {
                Instantiate(bullet.triggerEffect, hitPoint, other.gameObject.transform.rotation);
            }
            if (bullet.isDestroy) Destroy(other.gameObject);
        }
        if (hp <= 0) enemyDie();
    }

    float hit(ConfigPlayerBullet bullet, float enemyHp)
    {
        float damage = bullet.getDamage();
        Debug.Log("ダメージ：" + damage);
        enemyHp -= damage;
        AudioClip hitSe = bullet.hitSe;
        if (hitSe != null) SoundManager.Instance.PlaySE(bullet.hitSe, bullet.hitSeVolume);
        return enemyHp;
    }

    void enemyDie()
    {
        isDead = true;
        Explosion(maxHp);
        Destroy(gameObject);
    }

    void Explosion(float maxHp)
    {
        Vector3 pos = gameObject.transform.position;
        if (offsetExplosionY != 0) pos.y += offsetExplosionY;
        if(maxHp < 50){
            EffectController.Instance.PlaySmallExplosion(pos, gameObject.transform.rotation);
            return;
        }
        if(maxHp < 100){
            EffectController.Instance.PlayMiddleExplosion(pos, gameObject.transform.rotation);
            return;
        }
        EffectController.Instance.PlayLargeExplosion(pos, gameObject.transform.rotation);
    }

}
