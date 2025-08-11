using UnityEngine;

public class ConfigEnemyBullet : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject _muzzlePrefab;
    [SerializeField] private GameObject _hitPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("DeleteEnemyBulletObj"))
        {
            DestroyBullet();
        }
    }

    public void MuzzleEffect()
    {
        if (_muzzlePrefab != null)
        {
            EffectController.Instance.PlayEffect(_muzzlePrefab, transform.position, transform.rotation);
        }
    }

    public void HitEffect()
    {
        if (_hitPrefab != null)
        {
            EffectController.Instance.PlayEffect(_hitPrefab, transform.position, transform.rotation);
        }
    }

    /// <summary>
    /// 弾を削除する
    /// </summary>
    public void DestroyBullet()
    {
        HitEffect();
        Destroy(gameObject);
    }
}
