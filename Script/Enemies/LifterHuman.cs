using UnityEngine;
using System.Collections;

/// <summary>
/// リフターに乗った人間の敵
/// </summary>
public class LifterHuman : BaseEnemy
{
    #region 移動・浮遊設定
    [Header("浮遊設定")]
    [SerializeField] private float floatAmplitude = 0.5f;  // 上下の振幅
    [SerializeField] private float floatSpeed = 2f;        // 上下の速度
    [SerializeField] private float sideAmplitude = 0.3f;   // 左右の振幅
    [SerializeField] private float sideSpeed = 1.5f;       // 左右の速度
    
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 2f;         // プレイヤーに近づく速度
    [SerializeField] private float stopDistance = 3f;     // 停止する距離
    
    [Header("回転設定")]
    [SerializeField] private float yRotateSpeed = 90f;     // Y軸回転速度（度/秒）
    [SerializeField] private float zRotateSpeed = 90f;     // Z軸回転速度（度/秒）
    [SerializeField] private float xRotateSpeed = 30f;     // X軸回転速度（度/秒）- ゆっくり
    [SerializeField] private float maxXRotation = 45f;     // X軸回転の制限角度
    
    [Header("ラグドール設定")]
    [SerializeField] private GameObject ragdollPrefab;     // ラグドールのプレファブ
    [SerializeField] private float ragdollForce = 500f;    // 吹っ飛ばす力
    // [SerializeField] private Vector3 ragdollForceDirection = Vector3.up; // 不要：動的に計算
    #endregion

    #region 内部変数
    private Vector3 basePosition;     // 移動の基準位置
    private float floatTimer = 0f;    // 浮遊アニメーション用タイマー
    private float sideTimer = 0f;     // 左右揺らし用タイマー
    private Vector3 currentRotation;  // 現在の回転値
    private bool isNearPlayer = false; // プレイヤーに近い状態かどうか
    #endregion

    #region 初期化・基本更新
    protected override void OnStart()
    {
        // 基準位置を初期位置に設定
        basePosition = transform.position;
        
        // 現在の回転値を記録
        currentRotation = transform.eulerAngles;
        
        // ランダムなオフセットで浮遊開始
        floatTimer = Random.Range(0f, Mathf.PI * 2f);
        sideTimer = Random.Range(0f, Mathf.PI * 2f);
    }

    protected override void HandleMovement()
    {
        if (playerTransform == null) return;

        // ─── プレイヤーに近づく移動 ───
        HandlePlayerApproach();
        
        // ─── 上下の浮遊動作 ───
        HandleFloatingMovement();
        
        // ─── プレイヤー方向への回転 ───
        HandlePlayerLookingRotation();
    }
    #endregion

    #region 移動・回転処理
    /// <summary>
    /// プレイヤーに近づく移動処理
    /// </summary>
    private void HandlePlayerApproach()
    {
        // プレイヤーまでの距離を計算（Y軸は無視）
        Vector3 toPlayer = playerTransform.position - basePosition;
        float distanceToPlayer = toPlayer.magnitude;
        
        // Y座標の上限チェック - 上限を超えている場合は徐々に下がる
        float maxYLimit = GameManager.Instance.maxY - 1f;
        if (basePosition.y > maxYLimit)
        {
            // 徐々に下がる
            float downwardSpeed = moveSpeed * 0.5f; // 移動速度の半分で下がる
            basePosition.y -= downwardSpeed * Time.deltaTime;
            
            // 下限を設定（上限以下まで）
            if (basePosition.y < maxYLimit)
            {
                basePosition.y = maxYLimit;
            }
        }
        
        // 停止距離より遠い場合のみ移動
        if (distanceToPlayer > stopDistance)
        {
            // プレイヤー方向への移動ベクトル
            Vector3 moveDirection = toPlayer.normalized;
            
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
            basePosition += movement;
            
            isNearPlayer = false;
        }
        else
        {
            isNearPlayer = true;
        }
    }
    
    /// <summary>
    /// 上下の浮遊動作
    /// </summary>
    private void HandleFloatingMovement()
    {
        floatTimer += Time.deltaTime * floatSpeed;
        sideTimer += Time.deltaTime * sideSpeed;
        
        // サイン波で上下運動
        float yOffset = Mathf.Sin(floatTimer) * floatAmplitude;
        Vector3 newPosition = basePosition;
        newPosition.y += yOffset;
        
        // プレイヤーに近い時は左右にも揺らす
        if (isNearPlayer)
        {
            // プレイヤーからの右方向ベクトルを計算
            Vector3 toPlayer = (playerTransform.position - basePosition).normalized;
            Vector3 rightDirection = Vector3.Cross(toPlayer, Vector3.up).normalized;
            
            // 左右の揺らし
            float sideOffset = Mathf.Sin(sideTimer) * sideAmplitude;
            newPosition += rightDirection * sideOffset;
        }
        
        transform.position = newPosition;
    }

    /// <summary>
    /// プレイヤー方向への回転処理（x軸制限あり）
    /// </summary>
    private void HandlePlayerLookingRotation()
    {
        Vector3 toPlayer = playerTransform.position - transform.position;
        
        // 目標回転角度を計算
        Quaternion targetRotation = Quaternion.LookRotation(toPlayer);
        Vector3 targetEuler = targetRotation.eulerAngles;
        
        // X軸回転を制限
        float targetX = Mathf.DeltaAngle(0f, targetEuler.x);
        targetX = Mathf.Clamp(targetX, -maxXRotation, maxXRotation);
        
        // 各軸の回転速度を個別に適用
        float deltaTime = Time.deltaTime;
        
        // X軸：ゆっくり回転
        currentRotation.x = Mathf.MoveTowardsAngle(
            currentRotation.x,
            targetX,
            xRotateSpeed * deltaTime
        );
        
        // Y軸：通常速度
        currentRotation.y = Mathf.MoveTowardsAngle(
            currentRotation.y,
            targetEuler.y,
            yRotateSpeed * deltaTime
        );
        
        // Z軸：通常速度
        currentRotation.z = Mathf.MoveTowardsAngle(
            currentRotation.z,
            targetEuler.z,
            zRotateSpeed * deltaTime
        );
        
        // 回転を適用
        transform.rotation = Quaternion.Euler(currentRotation);
    }
    #endregion

    #region 死亡処理オーバーライド
    /// <summary>
    /// 死亡処理をオーバーライドしてラグドール化
    /// </summary>
    protected override void enemyDie()
    {
        isDead = true;
        
        // ラグドールを生成して吹っ飛ばす
        if (ragdollPrefab != null)
        {
            SpawnRagdoll();
        }
        
        // 通常の死亡処理（エフェクト、スコア、アイテム）
        Explosion(maxHp);
        AddKillCount();
        AddScore(maxHp);
        ApearItem(item);
        
        // オブジェクトを削除
        Destroy(gameObject);
    }

    /// <summary>
    /// ラグドールを生成して吹っ飛ばす
    /// </summary>
    private void SpawnRagdoll()
    {
        // ラグドールを生成
        GameObject ragdoll = Instantiate(ragdollPrefab, transform.position, transform.rotation);
        
        // キャラが向いている方向の逆方向＋少し上の方向を計算
        Vector3 backwardDirection = -transform.forward; // 向いている方向の逆
        Vector3 upwardOffset = Vector3.up * 0.8f;       // 少し上方向を追加
        Vector3 baseForceDirection = (backwardDirection + upwardOffset).normalized;
        
        // Rigidbodyを取得して力を加える
        Rigidbody[] ragdollRigidbodies = ragdoll.GetComponentsInChildren<Rigidbody>();
        
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            if (rb != null)
            {
                // 各部位に力を加える（ランダム要素を追加）
                Vector3 forceDirection = baseForceDirection + Random.insideUnitSphere * 0.3f;
                rb.AddForce(forceDirection.normalized * ragdollForce, ForceMode.Impulse);
                
                // 回転も追加
                rb.AddTorque(Random.insideUnitSphere * ragdollForce * 0.5f, ForceMode.Impulse);
            }
        }
        
    }
    #endregion
} 