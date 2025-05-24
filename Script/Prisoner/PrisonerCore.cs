using UnityEngine;
using UnityEngine.Rendering;  // BlendMode を使うために追加

public class PrisonerCore : MonoBehaviour
{
    [SerializeField] public float hp = 10;
    private float maxHp = 10;
    [SerializeField] float offsetExplosionY = 0f;

    [Header("ItemCharcterPrefab")]
    [SerializeField] GameObject itemCharacterPrefab;

    Transform playerTransform;
    bool isDead = false;


    void Start()
    {
        var playerObj = GameObject.FindWithTag("Core");
        if (playerObj != null)
            playerTransform = playerObj.transform;
        maxHp = hp;
    }

    void Update()
    {
        if (playerTransform == null) return;

        if ((!isDead) && (hp <= 0)) enemyDie();

    }


    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("PlayerBullet"))
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
        enemyHp -= bullet.getDamage();
        AudioClip hitSe = bullet.hitSe;
        if (hitSe != null) SoundManager.Instance.PlaySE(bullet.hitSe, bullet.hitSeVolume);
        return enemyHp;
    }

    void enemyDie()
    {
        isDead = true;
        
        Vector3 pos = transform.position;
        if (offsetExplosionY != 0) pos.y += offsetExplosionY;
        GameObject itemCharacter = Instantiate(itemCharacterPrefab, pos, Quaternion.identity);
        EffectController.Instance.PlayLargeExplosion(pos, gameObject.transform.rotation);
        AddScore(maxHp);
        Destroy(gameObject);
    }

    void AddScore(float maxHp)
    {
        GameManager.Instance.AddScore(maxHp);
    }

}
