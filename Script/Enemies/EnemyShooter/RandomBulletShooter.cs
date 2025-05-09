using UnityEngine;

public class RandomBulletShooter : MonoBehaviour
{
    [Header("弾のプレハブと発射位置")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("方向とバラツキ (度数)")]
    // 0°が+Z方向、90°が+Y方向
    [Range(-180f, 180f)] public float baseAngleDeg = 0f;
    // ±何度までばらつかせるか
    [Range(0f, 180f)]      public float angleVarianceDeg = 15f;

    [Header("速度設定")]
    public float minSpeed = 5f;
    public float maxSpeed = 10f;

    public void Fire()
    {
        // 1) ベース角度にランダムなオフセットを加える
        float randomAngle = baseAngleDeg + Random.Range(-angleVarianceDeg, angleVarianceDeg);
        // 2) 度をラジアンに変換
        float rad = randomAngle * Mathf.Deg2Rad;
        // 3) ZY 平面上の単位ベクトルを作成 (X は常に 0)
        Vector3 dir = new Vector3(0f, Mathf.Sin(rad), Mathf.Cos(rad)).normalized;

        // 4) ランダム速度を決定
        float speed = Random.Range(minSpeed, maxSpeed);

        // 5) 弾を生成して回転・速度を設定
        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.LookRotation(dir, Vector3.up)
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = dir * speed;
        }
    }
}
