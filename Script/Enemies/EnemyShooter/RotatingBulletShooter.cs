using UnityEngine;

public class RotatingBulletShooter : NWayShooter
{
    [Header("回転設定")]
    [SerializeField] private float initialAngle = 0f; // 初期角度（度）
    [SerializeField] private float angleChangeRate = 5f; // 角度変化量（度）
    [SerializeField] private float angleChangeInterval = 0.1f; // 角度変化間隔（秒）
    [SerializeField] private RotationDirection rotationDirection = RotationDirection.Right;
    
    [Header("自動発射設定")]
    [SerializeField] private bool startAutoFireOnStart = true; // 開始時に自動発射を開始するか
    
    // 回転方向の列挙型
    public enum RotationDirection
    {
        Left,   // 左回り
        Right   // 右回り
    }
    
    private float currentAngle;
    private float lastAngleChangeTime;
    
    void Start()
    {
        currentAngle = initialAngle;
        lastAngleChangeTime = Time.time;
        
        // NWayShooterの設定を回転弾用に調整
        numberOfBullets = 1; // 1発ずつ撃つ
        totalSpreadAngle = 0f; // 拡散なし
        isContinuousFire = false; // 連続発射オフ
        
        // 自動発射を開始
        if (startAutoFireOnStart)
        {
            StartAutoFire();
        }
    }
    
    void Update()
    {
        UpdateAngle();
    }
    
    private void UpdateAngle()
    {
        if (Time.time - lastAngleChangeTime >= angleChangeInterval)
        {
            float angleChange = angleChangeRate;
            
            switch (rotationDirection)
            {
                case RotationDirection.Left:
                    currentAngle += angleChange;
                    break;
                    
                case RotationDirection.Right:
                    currentAngle -= angleChange;
                    break;
            }
            
            // 角度を0-360度の範囲に正規化
            currentAngle = currentAngle % 360f;
            if (currentAngle < 0f)
            {
                currentAngle += 360f;
            }
            
            lastAngleChangeTime = Time.time;
        }
    }
    
    /// <summary>
    /// 現在の角度で回転弾を発射
    /// </summary>
    public void FireRotatingBullet()
    {
        Fire(currentAngle);
    }
    
    /// <summary>
    /// 指定した発射レートで自動的に回転弾を発射
    /// </summary>
    public void StartAutoFire()
    {
        InvokeRepeating(nameof(FireRotatingBullet), 0f, angleChangeInterval);
    }
    
    /// <summary>
    /// 自動発射を停止
    /// </summary>
    public void StopAutoFire()
    {
        CancelInvoke(nameof(FireRotatingBullet));
    }
    
    /// <summary>
    /// 自動発射レートを変更（実行中に変更可能）
    /// </summary>
    public void ChangeAutoFireRate(float newFireRate)
    {
        angleChangeInterval = newFireRate;
        StopAutoFire();
        StartAutoFire();
    }
    
    // デバッグ用：現在の角度を表示
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            float angleInRadians = currentAngle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(0f, Mathf.Sin(angleInRadians), Mathf.Cos(angleInRadians));
            Gizmos.DrawRay(transform.position, direction * 3f);
        }
    }
} 