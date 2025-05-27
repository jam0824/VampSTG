using UnityEngine;
using System.Collections;

public class Enemy : BaseEnemy
{
    [Header("移動設定")]
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float rotateSpeed = 90f;   // 度/秒
    [SerializeField] float stopDistance = 0.1f;

    protected override void HandleMovement()
    {
        if (playerTransform == null) return;

        // ─── 移動 ───
        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.x = 0f;
        float dist = toPlayer.magnitude;
        if (dist > stopDistance)
        {
            float step = moveSpeed * Time.deltaTime;
            Vector3 stepPos = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
            stepPos.x = 0f;
            transform.position = stepPos;
        }

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

    protected override IEnumerator AttackCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackInterval);
            animator.SetTrigger("Attack");
            yield return new WaitForSeconds(attackAnimationWait);
            enemyShooter.Fire();
        }
    }
} 