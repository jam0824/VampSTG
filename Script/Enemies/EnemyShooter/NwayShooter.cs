using UnityEngine;
using System.Collections;

public class NWayShooter : MonoBehaviour, IEnemyShooter
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;    // 弾のプレハブ（Rigidbody 必須）
    public Transform firePoint;        // 発射位置
    public float bulletSpeed = 10f;    // 弾速
    public float bulletLifeTime = 30f;  // 自動消滅までの時間

    [Header("Shot Settings")]
    public int numberOfBullets = 5;     // NWay の "N"
    public float totalSpreadAngle = 60f; // 全体の拡散角度（度）
    
    [Header("Continuous Fire Settings")]
    public bool isContinuousFire = false;  // 連続発射するかどうか
    public int continuousFireCount = 3;    // 連続発射回数
    public float continuousFireInterval = 0.2f; // 連続発射間隔（秒）

    [Header("Muzzle Fire")]
    public GameObject muzzleFirePrefab;
    public AudioClip muzzleFireSe;
    public float muzzleFireSeVolume = 0.8f;

    [Header("Target")]
    public Transform core;           // プレイヤー Transform

    void Start()
    {
        GetCore();
    }

    void GetCore()
    {
        if (core != null) return;
        var p = GameObject.FindGameObjectWithTag("Core");
        if (p != null) core = p.transform;

    }

    /// <summary>
    /// NWay 弾を発射する（連続発射設定に応じて）
    /// </summary>
    public void Fire()
    {
        if (isContinuousFire)
        {
            StartCoroutine(ContinuousFireCoroutine());
        }
        else
        {
            FireSingleShot();
        }
    }

    /// <summary>
    /// 連続発射コルーチン
    /// </summary>
    private IEnumerator ContinuousFireCoroutine()
    {
        GetCore();
        if (bulletPrefab == null || firePoint == null || core == null) yield break;

        // 最初に角度を計算して維持
        Vector3 toPlayer = core.position - firePoint.transform.position;
        toPlayer.x = 0f;
        float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.z) * Mathf.Rad2Deg;

        for (int i = 0; i < continuousFireCount; i++)
        {
            FireSingleShot(baseAngle);
            if (i < continuousFireCount - 1) // 最後の発射後は待機しない
            {
                yield return new WaitForSeconds(continuousFireInterval);
            }
        }
    }

    /// <summary>
    /// 1回の発射処理
    /// </summary>
    private void FireSingleShot()
    {
        GetCore();
        if (bulletPrefab == null || firePoint == null || core == null) return;

        // プレイヤーへのベクトルを取得し、X成分を無視 → ZY平面に投影
        Vector3 toPlayer = core.position - firePoint.transform.position;
        toPlayer.x = 0f;

        // ZY平面上の基準角度（度単位）
        float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.z) * Mathf.Rad2Deg;
        MuzzleFire();

        // 弾が 1 発だけなら、プレイヤー方向へ直撃
        if (numberOfBullets <= 1)
        {
            ShootBullet(baseAngle);
        }
        else
        {
            // 各弾の間隔
            float angleStep = totalSpreadAngle / (numberOfBullets - 1);
            // 中央の弾インデックス（int 型の割り算で自動切り捨て）
            int midIndex = (numberOfBullets - 1) / 2;
            // 中央の弾が baseAngle になるように、最初の弾角度を調整
            float startAngle = baseAngle - angleStep * midIndex;

            for (int i = 0; i < numberOfBullets; i++)
            {
                float angle = startAngle + angleStep * i;
                ShootBullet(angle);
            }
        }
    }

    /// <summary>
    /// 指定した方向（ZY 平面上の度数法）に向かって NWay 弾を発射する（連続発射設定に応じて）
    /// </summary>
    /// <param name="baseAngleDeg">射出方向の基準角度（度）</param>
    public void Fire(float baseAngleDeg)
    {
        if (isContinuousFire)
        {
            StartCoroutine(ContinuousFireCoroutine(baseAngleDeg));
        }
        else
        {
            FireSingleShot(baseAngleDeg);
        }
    }

    /// <summary>
    /// 方向指定の連続発射コルーチン
    /// </summary>
    private IEnumerator ContinuousFireCoroutine(float baseAngleDeg)
    {
        for (int i = 0; i < continuousFireCount; i++)
        {
            FireSingleShot(baseAngleDeg);
            if (i < continuousFireCount - 1) // 最後の発射後は待機しない
            {
                yield return new WaitForSeconds(continuousFireInterval);
            }
        }
    }

    /// <summary>
    /// 方向指定の1回発射処理
    /// </summary>
    private void FireSingleShot(float baseAngleDeg)
    {
        if (bulletPrefab == null || firePoint == null) return;
        MuzzleFire();

        // 1 発なら真ん中へ
        if (numberOfBullets <= 1)
        {
            ShootBullet(baseAngleDeg);
            return;
        }

        // 各弾の間隔
        float angleStep = totalSpreadAngle / (numberOfBullets - 1);
        // 中央の弾インデックス
        int midIndex = (numberOfBullets - 1) / 2;
        // 最初の弾角度
        float startAngle = baseAngleDeg - angleStep * midIndex;

        for (int i = 0; i < numberOfBullets; i++)
        {
            float angle = startAngle + angleStep * i;
            ShootBullet(angle);
        }
    }
    

    /// <summary>
    /// 引数なし → オブジェクトの向き（ZY平面上投影）を使って NWay 弾を発射する
    /// </summary>
    public void FireAtAngle()
    {
        // transform.forward を ZY 平面に投影
        Vector3 forward = transform.forward;
        forward.x = 0f;
        float baseAngle = Mathf.Atan2(forward.y, forward.z) * Mathf.Rad2Deg;
        Fire(baseAngle);
    }

    private void MuzzleFire()
    {
        if (muzzleFirePrefab != null)
        {
            EffectController.Instance.PlayEffect(muzzleFirePrefab, firePoint.position, firePoint.rotation);
        }
        SoundManager.Instance.PlaySE(muzzleFireSe, muzzleFireSeVolume);
    }

    /// <summary>
    /// 単発で弾を生成し、指定角度で撃つ
    /// </summary>
    void ShootBullet(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(0f, Mathf.Sin(rad), Mathf.Cos(rad)).normalized;
        Vector3 firePointPos = firePoint.position;
        firePointPos.x = 0;
        GameObject b = EffectController.Instance.PlayEnemyBullet(bulletPrefab, firePointPos, Quaternion.LookRotation(dir));
        
        // IBulletコンポーネントがあったら弾速を変更
        if (b.TryGetComponent<IBullet>(out var bullet))
        {
            bullet.Speed = bulletSpeed;
        }
        
        if (b.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.linearVelocity = dir * bulletSpeed;
        }
        Destroy(b, bulletLifeTime);
    }
}
