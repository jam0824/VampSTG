using UnityEngine;

public class Smine : MonoBehaviour
{
    [Header("動作設定")]
    public float jumpForce = 5f; // 上昇力
    public float fallThreshold = 0.5f; // 落下開始判定の高さ
    public float shootHeight = 1f; // 発射する高さ
    [SerializeField]private AudioClip jumpSE;
    [SerializeField]private float jumpSEVol = 1f;
    
    [Header("子弾設定")]
    public GameObject bulletPrefab; // 子弾のプレハブ
    public float bulletSpeed = 10f; // 子弾の速度
    public int bulletCount = 3; // 前後それぞれの弾数
    public float spreadAngle = 60f; // 拡散角度

    [Header("爆発設定")]
    [SerializeField]private GameObject explosionPrefab; // 爆発のプレハブ
    
    private Rigidbody rb;
    private bool hasJumped = false;
    private bool hasFired = false;
    private float startY;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
    }

    void OnCollisionEnter(Collision collision)
    {
        if ((collision.gameObject.CompareTag("Enemy"))||(collision.gameObject.CompareTag("Boss")))
        {
            rb.useGravity = true;
            startY = transform.position.y;
            // 上方向に飛び上がる
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            hasJumped = true;
            SoundManager.Instance.PlaySE(jumpSE, jumpSEVol);
        }
    }

    void Update()
    {
        // 落下中で、指定した高さに到達したら子弾を発射
        if (hasJumped && !hasFired && rb.linearVelocity.y < 0 && 
            transform.position.y <= startY + shootHeight)
        {
            FireBullets();
            hasFired = true;
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        
    }
    
    void FireBullets()
    {
        if (bulletPrefab == null) return;
        
        Debug.Log("FireBullets");
        
        // シンプルに全方向に拡散弾を発射
        FireBulletsInAllDirections();
    }
    
    void FireBulletsInAllDirections()
    {
        if (bulletPrefab == null) return;
        
        Debug.Log($"弾発射: 弾数={bulletCount}, 拡散角={spreadAngle}");
        
        // Z軸プラス方向（前方向）に発射
        FireBulletsInDirection(1f, "前方向");
        
        // Z軸マイナス方向（後方向）に発射
        FireBulletsInDirection(-1f, "後方向");
    }
    
    void FireBulletsInDirection(float zDirection, string directionName)
    {
        for (int i = 0; i < bulletCount; i++)
        {
            // Y軸方向（上下）の拡散角度を計算
            float spreadAngleForBullet;
            if (bulletCount == 1)
            {
                spreadAngleForBullet = 0f;
            }
            else
            {
                // 全体の拡散角度を弾数-1で割って均等に配置
                spreadAngleForBullet = -spreadAngle * 0.5f + (spreadAngle / (bulletCount - 1)) * i;
            }
            
            float radians = spreadAngleForBullet * Mathf.Deg2Rad;
            
            // 方向ベクトルを計算（指定されたZ方向基準でY軸方向に拡散）
            Vector3 direction = new Vector3(0, Mathf.Sin(radians), zDirection * Mathf.Cos(radians));
            
            Debug.Log($"{directionName}弾{i}: Y軸拡散角={spreadAngleForBullet:F1}度, 方向={direction}");
            
            // 子弾を生成（方向ベクトルと同じ方向を向けて）
            Quaternion rotation = Quaternion.LookRotation(direction);
            GameObject bullet = Instantiate(bulletPrefab, transform.position, rotation);
            
            // 子弾に速度を与える
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            if (bulletRb == null)
            {
                bulletRb = bullet.AddComponent<Rigidbody>();
            }
            
            bulletRb.linearVelocity = direction * bulletSpeed;
            Debug.Log($"{directionName}弾{i}の速度: {direction * bulletSpeed}");
        }
        
    }
    

}
