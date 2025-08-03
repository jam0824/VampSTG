using UnityEngine;

public class ThrownProjectile : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 10f;
    
    private Vector3 direction;
    private bool isInitialized = false;
    
    void Start()
    {
        // 一定時間後に削除
        Destroy(gameObject, lifeTime);
    }
    
    void Update()
    {
        if (isInitialized)
        {
            // 弾丸のように真っすぐ移動
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }
    }
    
    /// <summary>
    /// 投射方向と速度を設定
    /// </summary>
    /// <param name="dir">移動方向（正規化済み）</param>
    /// <param name="projectileSpeed">移動速度</param>
    public void Initialize(Vector3 dir, float projectileSpeed)
    {
        direction = dir.normalized;
        speed = projectileSpeed;
        isInitialized = true;
        
        // オブジェクトを移動方向に向ける（オプション）
        if (direction != Vector3.zero)
        {
            transform.LookAt(transform.position + direction);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // プレイヤーコアに当たったら削除
        if (other.CompareTag("Core"))
        {
            // ここでダメージ処理などを追加可能
            Destroy(gameObject);
        }
        
        // 地面や壁に当たったら削除
        if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
} 