using UnityEngine;

public class ConfigEnemyBullet : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject _muzzlePrefab;
    [SerializeField] private GameObject _hitPrefab;

    private bool isDestroyed = false;
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
        if (_hitPrefab != null && GameManager.Instance != null && 
            GameManager.Instance.IsWithinScreenBounds(transform.position, 1f)&&
            !isDestroyed)
        {
            EffectController.Instance.PlayEffect(_hitPrefab, transform.position, transform.rotation);
            isDestroyed = true;
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

    /// <summary>
    /// オブジェクトが非アクティブになったときにエフェクトを出す
    /// </summary>
    void OnDisable()
    {
        HitEffect();
    }

    /// <summary>
    /// オブジェクトが破棄されたときにエフェクトを出す
    /// </summary>
    void OnDestroy()
    {
        if (gameObject.activeSelf)
        {
            HitEffect();
        }
    }
}
