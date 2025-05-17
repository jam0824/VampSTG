using UnityEngine;

public class ExplosionDamage : MonoBehaviour
{
    [Header("ダメージを与える時間")]
    [SerializeField] private float damageTerm = 0.1f;
    private ConfigPlayerBullet configPlayerBullet;
    private float elapsedTime = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        configPlayerBullet = GetComponent<ConfigPlayerBullet>();
    }
    void Start()
    {
        //作成時に音を出す
        EffectController.Instance.PlayMiddleExplosionSeOnly();
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if ((elapsedTime > damageTerm) && (configPlayerBullet.damage != 0))
        {
            configPlayerBullet.damage = 0;
        }
    }
}
