using UnityEngine;

public class ScatterShooter : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;    // Rigidbody を持つ弾のプレハブ
    public Transform firePoint;        // 発射位置
    public float bulletLifeTime = 5f;  // 自動消滅までの時間

    [Header("Scatter Settings")]
    public int bulletCount = 20;       // ばらまき弾の総数
    public float scatterAngle = 60f;   // 全体の散布角度（度）
    public float minBulletSpeed = 5f;  // 弾速の最小値
    public float maxBulletSpeed = 15f; // 弾速の最大値

    [Header("Target")]
    public Transform player;           // プレイヤー Transform

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Core");
            if (p != null) player = p.transform;
        }
    }

    /// <summary>
    /// プレイヤー方向を中心にランダム角度・ランダム速度で弾をばらまく
    /// </summary>
    public void FireScatter()
    {
        if (bulletPrefab == null || firePoint == null || player == null) return;

        // 発射位置 → プレイヤーへのベクトルを取得し、X成分を無視して ZY 平面に投影
        Vector3 toPlayer = player.position - firePoint.position;
        toPlayer.x = 0f;

        // ZY 平面上の基準角度（プレイヤー方向）
        float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.z) * Mathf.Rad2Deg;

        // bulletCount 発、scatterAngle の中でランダムに角度オフセット＆速度決定
        for (int i = 0; i < bulletCount; i++)
        {
            // 角度オフセット
            float randomOffset = Random.Range(-scatterAngle / 2f, scatterAngle / 2f);
            float angle = baseAngle + randomOffset;

            // 速度もランダム
            float speed = Random.Range(minBulletSpeed, maxBulletSpeed);

            ShootBullet(angle, speed);
        }
    }

    /// <summary>
    /// 指定角度・指定速度で単発の弾を生成して飛ばす
    /// </summary>
    void ShootBullet(float angleDeg, float speed)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(0f, Mathf.Sin(rad), Mathf.Cos(rad)).normalized;
        Vector3 firePointPos = firePoint.position;
        firePointPos.x = 0;
        // 弾を生成し、向きを dir に合わせる
        GameObject b = Instantiate(bulletPrefab, firePointPos, Quaternion.LookRotation(dir));
        if (b.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = dir * speed;
        }

        Destroy(b, bulletLifeTime);
    }
}
