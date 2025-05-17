using UnityEngine;
using System.Collections.Generic;

public class EnemySeekerShooter : MonoBehaviour
{
    [Header("ターゲット検索")]
    public string enemyTag = "Enemy";
    
    [Header("ターゲット検索のリトライ数")]
    public float retryCount = 5;

    [Header("射撃設定")]
    public GameObject laserPrefab;
    public Transform firePoint;

    [Header("効果音")]
    public AudioClip se;
    public float seVol = 1f;

    private Transform currentTarget;
    private float fireTimer = 0f;

    private Transform core;
    private BitBattery bitBattery;

    // モデルのX軸90度回転補正
    private readonly Quaternion modelOffset = Quaternion.Euler(90f, 0f, 0f);
    // ビームの向き180度反転補正
    private readonly Quaternion beamFlip = Quaternion.Euler(0f, 180f, 0f);

    void Start()
    {
        core = GameObject.FindWithTag("Core").transform;
        bitBattery = transform.parent.GetComponent<BitBattery>();
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
        if (dist > bitBattery.stopDistance)
        {
            Vector3 dir = new Vector3(
                0f,
                currentTarget.position.y - transform.position.y,
                currentTarget.position.z - transform.position.z
            ).normalized;
            transform.position += dir * bitBattery.moveSpeed * Time.deltaTime;

            // 画面範囲内にクランプ（Y・Z軸）
            float clampedY = Mathf.Clamp(
                transform.position.y,
                GameManager.Instance.minY + bitBattery.moveOffset,
                GameManager.Instance.maxY - bitBattery.moveOffset
            );
            float clampedZ = Mathf.Clamp(
                transform.position.z,
                GameManager.Instance.minZ + bitBattery.moveOffset,
                GameManager.Instance.maxZ - bitBattery.moveOffset
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
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                bitBattery.rotationSpeed * Time.deltaTime
            );
        }

        // 射程内なら発射
        if (dist <= bitBattery.stopDistance && laserPrefab != null && firePoint != null)
        {
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                Quaternion fireRot = firePoint.rotation * modelOffset * beamFlip;
                Instantiate(laserPrefab, firePoint.position, fireRot);
                SoundManager.Instance.PlaySE(se, seVol);
                fireTimer = bitBattery.fireInterval;
            }
        }
    }

    void AcquireNewTarget()
    {
        // タグで見つかった全ての敵を取得
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        // 一定範囲内の敵のみをリストに追加
        List<GameObject> inRange = new List<GameObject>();
        foreach (var enemy in enemies)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) <= bitBattery.targetRadius)
            {
                inRange.Add(enemy);
            }
        }

        // 範囲内の敵がいなければターゲットなし
        if (inRange.Count == 0)
        {
            currentTarget = null;
            return;
        }

        for (int i = 0; i < retryCount; i++)
        {
            int randomIndex = Random.Range(0, inRange.Count);
            var candidate = inRange[randomIndex];
            currentTarget = candidate.transform;
            // Contains で重複チェック
            if (!bitBattery.targetEnemyPool.Contains(candidate)) break;
            
        }

        bitBattery.AddEnemy(currentTarget.gameObject);
    }
}
