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
    
    [Header("回転アニメーション設定")]
    public float rotationAnimationDuration = 0.5f; // 回転アニメーションの時間
    
    private Rigidbody rb;
    private bool hasJumped = false;
    private bool hasFired = false;
    private float startY;
    private bool isRotating = false; // 回転アニメーション中かどうか

    public float damage{get;set;}   //子弾のダメージ
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
    }

    // 共通の衝突処理関数
    void HandleHit(GameObject hitObject)
    {
        if ((hitObject.CompareTag("Enemy"))||
        (hitObject.CompareTag("Boss"))||
        (hitObject.CompareTag("EnemyBullet")))
        {
            if (!isRotating && !hasJumped)
            {
                // 回転アニメーションを開始
                StartCoroutine(RotateToStraightAndJump());
            }
        }
    }

    // OnCollisionEnter（通常のコライダー衝突）
    void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.gameObject);
    }

    // OnTriggerEnter（トリガーコライダー衝突）
    void OnTriggerEnter(Collider other)
    {
        HandleHit(other.gameObject);
    }
    
    System.Collections.IEnumerator RotateToStraightAndJump()
    {
        isRotating = true;
        
        // 現在の回転と目標の回転（真っすぐな位置）
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, 0); // 真っすぐな位置
        
        float elapsedTime = 0f;
        
        // 回転アニメーション
        while (elapsedTime < rotationAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / rotationAnimationDuration;
            
            // スムーズな補間
            t = Mathf.SmoothStep(0f, 1f, t);
            
            // 回転を補間
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            
            yield return null;
        }
        
        // 最終的な回転を確実に設定
        transform.rotation = targetRotation;
        
        isRotating = false;
        yield return new WaitForSeconds(0.2f);
        
        // 飛び上がり処理
        rb.useGravity = true;
        startY = transform.position.y;
        
        // コライダーをオフにする
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // 上方向に飛び上がる
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        hasJumped = true;
        SoundManager.Instance.PlaySE(jumpSE, jumpSEVol);
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
        // x軸からずれていたら補正する
        if((transform.position.x >=0.1f)||(transform.position.x <= -0.1f)){
            Vector3 pos = transform.position;
            pos.x = 0;
            transform.position = pos;
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
            GameObject bullet = MakeBullet(direction, Quaternion.identity, bulletPrefab, bulletSpeed, damage);

        }
        
    }

    GameObject MakeBullet(
        Vector3 direction, 
        Quaternion rotation, 
        GameObject bulletPrefab, 
        float bulletSpeed, 
        float damage)
    {
        
        // 子弾を生成（方向ベクトルと同じ方向を向けて）
        Quaternion rot = Quaternion.LookRotation(direction);
        GameObject bullet = Instantiate(bulletPrefab, transform.position, rot);

        bullet.GetComponent<ConfigPlayerBullet>().damage = damage; 
        // 子弾に速度を与える
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb == null)
        {
            bulletRb = bullet.AddComponent<Rigidbody>();
        }
        
        bulletRb.linearVelocity = direction * bulletSpeed;
        return bullet;
    }
    

}
