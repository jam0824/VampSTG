using System.Collections;
using UnityEngine;

public class AngleShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float shootingInterval = 0.2f; // 弾と弾の間隔
    [SerializeField] private float restDuration = 3f; // 休憩時間
    [SerializeField] private float burstCount = 5f; // 連続撃つ弾の数
    [SerializeField] private float size = 5f;
    
    [Header("Angle Settings")]
    [SerializeField] private float shootingAngles = 45f; // 射撃角度（度）
    [SerializeField] private bool randomizeAngles = false; // ランダム角度を使用するか
    [SerializeField] private float minRandomAngle = -45f;
    [SerializeField] private float maxRandomAngle = 45f;
    
    [Header("Direction Settings")]
    [SerializeField] private bool isFacingRight = false; // キャラクターの向き
    
    private bool isShooting = false;
    
    void Start()
    {
        // 射撃パターンを開始
        StartCoroutine(ShootingPattern());
    }
    
    /// <summary>
    /// メインの射撃パターン（設定回数撃って3秒休む）
    /// </summary>
    private IEnumerator ShootingPattern()
    {
        while (true)
        {
            // 設定回数弾を撃つ
            yield return StartCoroutine(ShootBurstFire());
            
            // 設定時間休む
            yield return new WaitForSeconds(restDuration);
        }
    }
    
    /// <summary>
    /// 設定回数連続で弾を撃つ
    /// </summary>
    private IEnumerator ShootBurstFire()
    {
        isShooting = true;
        
        for (int i = 0; i < burstCount; i++)
        {
            float angle = GetShootingAngle(i);
            ShootBullet(angle);
            
            // 最後の弾以外は間隔を空ける
            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(shootingInterval);
            }
        }
        
        isShooting = false;
    }
    
    /// <summary>
    /// 射撃角度を取得（キャラクターの向きを考慮）
    /// </summary>
    private float GetShootingAngle(int shotIndex)
    {
        float baseAngle;
        
        if (randomizeAngles)
        {
            // ランダム角度
            baseAngle = Random.Range(minRandomAngle, maxRandomAngle);
        }
        else
        {
            baseAngle = shootingAngles;
        }
        
        // キャラクターの向きを考慮して角度を調整
        if (!isFacingRight)
        {
            // 左向きの場合、角度を反転
            baseAngle = 180f - baseAngle;
        }
        
        return baseAngle;
    }
    
    /// <summary>
    /// 指定された角度で弾を撃つ（YZ面での3D横シューティング対応）
    /// </summary>
    private void ShootBullet(float angleDegrees)
    {
        if (bulletPrefab == null || firePoint == null) return;
        
        // 角度をラジアンに変換
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        
        // 弾を生成
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.transform.localScale = new Vector3(size, size, size);
        
        // YZ面での回転を設定（X軸周りの回転 + 90度調整）
        bullet.transform.rotation = Quaternion.Euler(angleDegrees + 90f, 0, 0);
        
        // YZ面での方向ベクトルを計算
        Vector3 direction = new Vector3(0, Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
        
        // 弾の速度を設定（Rigidbody3Dを使用）
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = direction * bulletSpeed;
        }
        else
        {
            // Rigidbodyがない場合はTransformで移動
            StartCoroutine(MoveBulletWithTransform(bullet, direction));
        }
    }
    
    /// <summary>
    /// Rigidbodyがない弾をTransformで移動させる
    /// </summary>
    private IEnumerator MoveBulletWithTransform(GameObject bullet, Vector3 direction)
    {
        while (bullet != null)
        {
            bullet.transform.position += direction * bulletSpeed * Time.deltaTime;
            yield return null;
        }
    }
    
    /// <summary>
    /// キャラクターの向きを変更
    /// </summary>
    public void SetFacingDirection(bool facingRight)
    {
        isFacingRight = facingRight;
    }
    
    /// <summary>
    /// 射撃角度を動的に変更
    /// </summary>
    public void SetShootingAngles(float newAngles)
    {
        shootingAngles = newAngles;
    }
    
    /// <summary>
    /// 射撃を停止
    /// </summary>
    public void StopShooting()
    {
        StopAllCoroutines();
        isShooting = false;
    }
    
    /// <summary>
    /// 射撃を再開
    /// </summary>
    public void ResumeShooting()
    {
        if (!isShooting)
        {
            StartCoroutine(ShootingPattern());
        }
    }
    
    /// <summary>
    /// 現在射撃中かどうか
    /// </summary>
    public bool IsShooting()
    {
        return isShooting;
    }
    
    
} 