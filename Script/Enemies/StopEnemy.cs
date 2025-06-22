using UnityEngine;

/// <summary>
/// 指定のZ座標でストップしてプレイヤーの方を向く敵クラス
/// </summary>
public class StopEnemy : BaseEnemy
{
    [Header("ストップ移動設定")]
    [SerializeField] private float stopZ = 7f;          // ストップするZ座標（Inspector設定可能）
    [SerializeField] private float moveSpeed = 2f;      // 移動速度
    [SerializeField] private float rotateSpeed = 90f;   // 回転速度（度/秒）
    
    private bool hasReachedStopPosition = false;        // ストップ位置に到達したかのフラグ

    /// <summary>
    /// 移動処理の実装
    /// </summary>
    protected override void HandleMovement()
    {
        if (playerTransform == null) return;

        // ストップ位置に到達していない場合は前進
        if (!hasReachedStopPosition)
        {
            MoveToStopPosition();
        }
        
        // 常にプレイヤーの方を向く
        RotateTowardsPlayer();
    }

    /// <summary>
    /// ストップ位置まで移動
    /// </summary>
    private void MoveToStopPosition()
    {
        // 現在のZ座標がストップ位置に到達したかチェック
        if (transform.position.z <= stopZ)
        {
            // ストップ位置に固定
            Vector3 currentPos = transform.position;
            currentPos.z = stopZ;
            transform.position = currentPos;
            hasReachedStopPosition = true;
            return;
        }

        // ストップ位置まで前進
        Vector3 moveDirection = Vector3.back; // Z軸負方向に移動
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// プレイヤーの方を向く
    /// </summary>
    private void RotateTowardsPlayer()
    {
        if (playerTransform == null) return;

        // プレイヤーへの方向ベクトルを計算
        Vector3 toPlayer = playerTransform.position - transform.position;
        
        // 十分な距離がない場合は回転しない
        if (toPlayer.sqrMagnitude < 0.001f) return;

        // 目標回転を計算
        Quaternion targetRotation = Quaternion.LookRotation(toPlayer);

        // スムーズに回転
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );
    }
} 