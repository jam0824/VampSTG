using UnityEngine;

public class FixedEnemy : BaseEnemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("移動設定")]
    [SerializeField] float rotateSpeed = 90f;   // 度/秒
    protected override void HandleMovement()
    {
        if (playerTransform == null) return;

        Vector3 toPlayer = playerTransform.position - transform.position;
        // ─── 常にプレイヤー方向を向く ───
        if (toPlayer.sqrMagnitude > 0.001f) // プレイヤーと同位置だとQuaternion.LookRotationでエラーになるので念のため
        {
            // y成分を無視して水平方向だけで向きを計算
            Vector3 dir = toPlayer.normalized;

            // 目標回転
            Quaternion targetRot = Quaternion.LookRotation(dir);

            // スムーズに回転
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );
        }
    }
}
