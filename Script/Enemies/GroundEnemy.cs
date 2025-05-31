using UnityEngine;
using System.Collections;

public class GroundEnemy : BaseEnemy
{
    [Header("移動設定")]
    [SerializeField] float moveSpeed = 1f;          // 独自の移動速度
    [SerializeField] float rotateSpeed = 90f;       // Y軸回転速度（度/秒）
    [SerializeField] float stopProbability = 0.8f; // 止まっている確率（0.8 = 80%）
    [SerializeField] float moveCheckInterval = 2f;  // 移動判定の間隔（秒）
    
    
    [Header("スクロール設定")]
    [SerializeField] float scrollSpeed = 0.6f;      // BGスクロールスピード
    
    // BGスクロールスピード取得用
    IScrollSpeed scrollSpeedProvider;
    
    bool isMoving = false;
    bool isRotating = false; // 回転中かどうかのフラグを追加

    protected override void OnStart()
    {
        // BGスクロールスピードを取得
        GetScrollSpeed();
        
        // 移動判定のコルーチンを開始
        StartCoroutine(MoveCheckCoroutine());
    }

    protected override void HandleMovement()
    {
        if (playerTransform == null) return;

        // ─── Y軸のみの回転でプレイヤー方向を向く ───
        RotateTowardsPlayer();
        
        // ─── BGスクロールスピードに合わせてz軸方向に移動 ───
        MoveWithScroll();
        
        // ─── 独自の移動（z軸のみ） ───
        if ((isMoving) && (!isAttackAnimation))
        {
            MoveForward();
        }
        
        // ─── x軸を0に固定 ───
        FixXPosition();
        
        // ─── 削除判定 ───
        CheckForDestroy();
    }
    
    /// <summary>
    /// BGスクロールスピードを取得
    /// </summary>
    void GetScrollSpeed()
    {
        // Stage3BGオブジェクトからIScrollSpeedを取得
        GameObject BG = GameObject.FindWithTag("BG");
        if (BG != null)
        {
            scrollSpeedProvider = BG.GetComponent<IScrollSpeed>();
            if (scrollSpeedProvider != null)
            {
                scrollSpeed = scrollSpeedProvider.ScrollSpeed;
            }
        }
    }
    
    /// <summary>
    /// Y軸のみの回転でプレイヤー方向を向く
    /// </summary>
    void RotateTowardsPlayer()
    {
        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0f; // Y成分を無視
        
        if (toPlayer.sqrMagnitude > 0.001f)
        {
            // Y軸のみの回転を計算
            Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized);
            
            // Y軸のみの回転に制限
            Vector3 eulerAngles = targetRot.eulerAngles;
            targetRot = Quaternion.Euler(0f, eulerAngles.y, 0f);
            
            // 現在の回転と目標回転の角度差を計算
            float angleDifference = Quaternion.Angle(transform.rotation, targetRot);
            bool wasRotating = isRotating;
            
            // 角度差が一定以上ある場合は回転中とみなす
            isRotating = angleDifference > 1f; // 1度以上の差がある場合は回転中
            
            // スムーズに回転
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );
            
            // 回転状態が変わった場合はアニメーションを更新
            if (wasRotating != isRotating)
            {
                UpdateMovementAnimation();
            }
        }
        else
        {
            // プレイヤーが近すぎる場合は回転停止
            bool wasRotating = isRotating;
            isRotating = false;
            if (wasRotating)
            {
                UpdateMovementAnimation();
            }
        }
    }
    
    /// <summary>
    /// BGのスクロールスピードに合わせてz軸方向に移動
    /// </summary>
    void MoveWithScroll()
    {
        // x軸は0のまま、z軸方向のみ移動
        Vector3 scrollMovement = new Vector3(0f, 0f, -scrollSpeed * Time.deltaTime);
        transform.Translate(scrollMovement, Space.World);
    }
    
    /// <summary>
    /// 独自の前進移動（z軸方向のみ）
    /// </summary>
    void MoveForward()
    {
        // z軸方向のみの移動（x軸は0のまま、Y軸は固定）
        Vector3 forward = transform.forward;
        forward.x = 0f; // x軸は0のまま
        forward.y = 0f; // Y軸は固定
        forward.Normalize();
        
        transform.Translate(forward * moveSpeed * Time.deltaTime, Space.World);
    }
    
    /// <summary>
    /// 移動するかどうかを定期的に判定
    /// </summary>
    IEnumerator MoveCheckCoroutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(moveCheckInterval);
            
            // 確率で移動するかどうかを決定
            float randomValue = Random.Range(0f, 1f);
            bool newMovingState = randomValue > stopProbability; // stopProbabilityより大きい場合のみ移動
            
            // 移動状態が変わった場合のみアニメーション更新
            if (newMovingState != isMoving)
            {
                isMoving = newMovingState;
                UpdateMovementAnimation();
            }
            
            // 移動時間をランダムに設定（0.5秒〜2秒）
            if (isMoving)
            {
                float moveTime = Random.Range(0.5f, 2f);
                StartCoroutine(StopMovingAfterTime(moveTime));
            }
        }
    }
    
    /// <summary>
    /// 指定時間後に移動を停止
    /// </summary>
    IEnumerator StopMovingAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        if (isMoving) // 現在移動中の場合のみ停止
        {
            isMoving = false;
            UpdateMovementAnimation();
        }
    }
    
    /// <summary>
    /// 移動状態に応じてアニメーションを更新
    /// </summary>
    void UpdateMovementAnimation()
    {
        if (animator == null) return;
        
        if (isMoving || isRotating) // 移動中または回転中の場合
        {
            animator.SetTrigger("walk");
        }
        else
        {
            animator.SetTrigger("idle");
        }
    }
    
    /// <summary>
    /// x軸を0に固定
    /// </summary>
    void FixXPosition()
    {
        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.x) > 0.001f) // 微小な誤差を許容
        {
            pos.x = 0f;
            transform.position = pos;
        }
    }
    
    /// <summary>
    /// z軸方向の削除判定
    /// </summary>
    void CheckForDestroy()
    {
        if (transform.position.z < GameManager.Instance.minZ - 1f)
        {
            Destroy(gameObject);
        }
    }

} 