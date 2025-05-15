using UnityEngine;

public class EnemySeekerShooter : MonoBehaviour
{
    [Header("ターゲット検索")]
    public string enemyTag = "Enemy";
    public float stopDistance = 2f;

    [Header("移動設定")]
    public float moveSpeed = 5f;
    public float moveOffset = 1f;
    public float rotationSpeed = 90f;

    [Header("射撃設定")]
    public GameObject laserPrefab;
    public Transform firePoint;
    public float fireInterval = 2f;

    [Header("効果音")]
    public AudioClip se;
    public float seVol = 1f;

    private Transform currentTarget;
    private float fireTimer = 0f;

    // モデルのX軸90度回転補正
    private readonly Quaternion modelOffset = Quaternion.Euler(90f, 0f, 0f);
    // ビームの向き180度反転補正
    private readonly Quaternion beamFlip = Quaternion.Euler(0f, 180f, 0f);

    void Start()
    {
        AcquireNewTarget();
    }

    void Update()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            AcquireNewTarget();
            return;
        }

        // YZ平面距離
        Vector2 me2D = new Vector2(transform.position.y, transform.position.z);
        Vector2 tgt2D = new Vector2(currentTarget.position.y, currentTarget.position.z);
        float dist = Vector2.Distance(me2D, tgt2D);

        // 移動
        if (dist > stopDistance)
        {
            Vector3 dir = new Vector3(
                0f,
                currentTarget.position.y - transform.position.y,
                currentTarget.position.z - transform.position.z
            ).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;

            // 画面範囲内にクランプ（Y・Z軸）
            float clampedY = Mathf.Clamp(
                transform.position.y,
                GameManager.Instance.minY + moveOffset,
                GameManager.Instance.maxY - moveOffset
            );
            float clampedZ = Mathf.Clamp(
                transform.position.z,
                GameManager.Instance.minZ + moveOffset,
                GameManager.Instance.maxZ - moveOffset
            );
            transform.position = new Vector3(
                transform.position.x,
                clampedY,
                clampedZ
            );
        }

        // 向き調整＋90度オフセット
        Vector3 lookDir = currentTarget.position - transform.position;
        lookDir.x = 0f;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir) * modelOffset;
            // ここをLerp/Slerpに置き換え
            transform.rotation = Quaternion.Slerp(
                transform.rotation,     // 現在の回転
                targetRot,              // 目標の回転
                rotationSpeed * Time.deltaTime  // 補間係数
            );
        }

        // 射程内なら発射
        if (dist <= stopDistance && laserPrefab != null && firePoint != null)
        {
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                // 発射時にはモデル補正＋ビーム反転補正を両方適用
                Quaternion fireRot = firePoint.rotation * modelOffset * beamFlip;
                Instantiate(laserPrefab, firePoint.position, fireRot);
                SoundManager.Instance.PlaySE(se, seVol);
                fireTimer = fireInterval;
            }
        }
    }

    void AcquireNewTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        float bestDist = Mathf.Infinity;
        Transform bestT = null;
        Vector2 me2D = new Vector2(transform.position.y, transform.position.z);

        foreach (var e in enemies)
        {
            Vector2 e2D = new Vector2(e.transform.position.y, e.transform.position.z);
            float d = Vector2.Distance(me2D, e2D);
            if (d < bestDist)
            {
                bestDist = d;
                bestT = e.transform;
            }
        }

        currentTarget = bestT;
    }
}
