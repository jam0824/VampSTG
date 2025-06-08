using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Stage3MidBoss : BaseEnemy
{
    [Header("移動設定")]
    [SerializeField] float moveSpeed = 0.5f;
    [SerializeField] float rotateSpeed = 30f;   // 度/秒
    [SerializeField] float stopDistance = 0.1f;


    [Header("アイテム設定")]
    [SerializeField] GameObject specificItem;   // 特定のアイテムをセット

    [Header("射撃設定")]
    [SerializeField] List<GameObject> enemyShooterObjects = new List<GameObject>();

    protected override void OnStart()
    {
        // 特定のアイテムが設定されている場合は、それを親クラスのitemプロパティに設定
        if (specificItem != null)
        {
            item = specificItem;
        }
    }

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
            foreach (var shooterObj in enemyShooterObjects)
            {
                if (shooterObj != null && shooterObj.TryGetComponent<IEnemyShooter>(out var shooter))
                {
                    shooter.Fire();
                }
            }
        }
    }

    protected override void Explosion(float maxHp)
    {

        for (int i = 0; i < 10; i++)
        {
            Vector3 pos = transform.position;
            pos.x = 1f; //少し画面の手前に出す
                        // y は 0 ～ 6 の範囲
            pos.y += (Random.value - 0.5f) * 2f;
            // z を ±1 の範囲でランダムにずらす
            pos.z += (Random.value - 0.5f) * 2f;

            //ランダム爆発
            float r = Random.value;
            if (r < 0.3)
            {
                EffectController.Instance.PlaySmallExplosion(pos, transform.rotation, false);

            }
            else if (r < 0.6)
            {
                EffectController.Instance.PlayMiddleExplosion(pos, transform.rotation, false);
            }
            else
            {
                EffectController.Instance.PlayLargeExplosion(pos, transform.rotation, false);
            }
        }
    }

}