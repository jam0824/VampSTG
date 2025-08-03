using UnityEngine;
using System.Collections;

public class BossMiddleGastaroid : BaseEnemy
{
    #region エネミーの行動パターン定義
    /// <summary>
    /// エネミーの行動パターン
    /// </summary>
    private enum ActionType
    {
        Attack,     // ジャンプ攻撃
        Roar,       // 咆哮攻撃
        Move,       // 移動
        Idle        // 待機
    }
    #endregion

    #region 移動・行動設定
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotateSpeed = 180f;
    [SerializeField] private float moveCheckInterval = 2f;
    
    [Header("行動確率設定（0.0〜1.0）")]
    [SerializeField] private float attackProbability = 0.4f;   // 攻撃確率
    [SerializeField] private float roarProbability = 0.2f;     // 咆哮確率
    [SerializeField] private float stopProbability = 0.3f;     // 停止確率
    
    [Header("ジャンプ攻撃設定")]
    [SerializeField] private float jumpHeight = 5f;            // ジャンプの最高点
    [SerializeField] private float jumpDistance = 3f;          // ジャンプの距離
    [SerializeField] private float jumpDuration = 1.5f;        // ジャンプにかかる時間
    
    [Header("咆哮攻撃設定")]
    [SerializeField] private float roarPreparationTime = 0.5f; // 咆哮準備時間
    [SerializeField] private float roarCooldownTime = 1.5f;    // 咆哮終了時間
    #endregion

    #region スクロール設定・状態管理
    private StageManager stageManager;
    
    [Header("状態管理")]
    private bool isMoving = false;
    private bool isRotating = false;
    
    // 定数定義
    private const float ROTATION_THRESHOLD = 1f;              // 回転中とみなす角度差
    private const float MIN_MOVE_TIME = 0.5f;                 // 最小移動時間
    private const float MAX_MOVE_TIME = 2f;                   // 最大移動時間
    private const float ATTACK_DIRECTION_RIGHT = 0f;          // 右向き攻撃角度
    private const float ATTACK_DIRECTION_LEFT = 180f;         // 左向き攻撃角度
    #endregion

    #region 初期化・基本更新
    protected override void OnStart()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
        StartCoroutine(ActionDecisionCoroutine());
    }

    protected override void Update()
    {
        base.Update();

        if (playerTransform == null) return;
        Vector3 pos = transform.position;
        if(pos.y < GameManager.Instance.minY) pos.y = GameManager.Instance.minY;
        transform.position = pos;
    }

    protected override void HandleMovement()
    {
        if (playerTransform == null) return;

        RotateTowardsPlayer();
        MoveWithScroll();
        
        if (isMoving && !isAttackAnimation)
        {
            MoveForward();
        }
        
        FixXPosition();
        CheckForDestroy();
    }
    #endregion

    #region 行動決定システム
    /// <summary>
    /// 定期的に行動を決定するメインコルーチン
    /// </summary>
    private IEnumerator ActionDecisionCoroutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(moveCheckInterval);
            
            ActionType selectedAction = DetermineNextAction();
            yield return ExecuteAction(selectedAction);
        }
    }
    
    /// <summary>
    /// 次の行動を確率に基づいて決定
    /// </summary>
    private ActionType DetermineNextAction()
    {
        float randomValue = Random.Range(0f, 1f);
        
        if (randomValue < attackProbability)
        {
            return ActionType.Attack;
        }
        else if (randomValue < attackProbability + roarProbability)
        {
            return ActionType.Roar;
        }
        else if (randomValue < GetMoveThreshold())
        {
            return ActionType.Move;
        }
        else
        {
            return ActionType.Idle;
        }
    }
    
    /// <summary>
    /// 移動確率の閾値を計算
    /// </summary>
    private float GetMoveThreshold()
    {
        float remainingProbability = 1f - attackProbability - roarProbability;
        float moveProbability = remainingProbability * (1f - stopProbability);
        return attackProbability + roarProbability + moveProbability;
    }
    
    /// <summary>
    /// 選択された行動を実行
    /// </summary>
    private IEnumerator ExecuteAction(ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Attack:
                yield return StartCoroutine(ExecuteJumpAttack());
                break;
            case ActionType.Roar:
                yield return StartCoroutine(ExecuteRoarAttack());
                break;
            case ActionType.Move:
                ExecuteMoveAction();
                break;
            case ActionType.Idle:
                ExecuteIdleAction();
                break;
        }
    }
    #endregion

    #region 行動実行メソッド
    /// <summary>
    /// ジャンプ攻撃の実行
    /// </summary>
    private IEnumerator ExecuteJumpAttack()
    {
        StopCurrentMovement();
        
        if (animator != null)
        {
            animator.SetTrigger("jump");
        }
        
        isAttackAnimation = true;
        yield return StartCoroutine(PerformJumpAttackMovement());
        yield return new WaitForSeconds(0.5f);
        isAttackAnimation = false;
    }
    
    /// <summary>
    /// 咆哮攻撃の実行
    /// </summary>
    private IEnumerator ExecuteRoarAttack()
    {
        StopCurrentMovement();
        
        if (animator != null)
        {
            animator.SetTrigger("attack");
        }
        
        isAttackAnimation = true;
        yield return new WaitForSeconds(roarPreparationTime);
        
        PerformRangedAttack();
        
        yield return new WaitForSeconds(roarCooldownTime);
        isAttackAnimation = false;
    }
    
    /// <summary>
    /// 移動行動の実行
    /// </summary>
    private void ExecuteMoveAction()
    {
        if (!isMoving)
        {
            isMoving = true;
            UpdateMovementAnimation();
        }
        
        float moveTime = Random.Range(MIN_MOVE_TIME, MAX_MOVE_TIME);
        StartCoroutine(StopMovingAfterTime(moveTime));
    }
    
    /// <summary>
    /// 待機行動の実行
    /// </summary>
    private void ExecuteIdleAction()
    {
        if (isMoving)
        {
            isMoving = false;
            UpdateMovementAnimation();
        }
    }
    #endregion

    #region 移動制御メソッド
    
    /// <summary>
    /// Y軸のみの回転でプレイヤー方向を向く
    /// </summary>
    private void RotateTowardsPlayer()
    {
        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0f; // Y成分を無視
        
        if (toPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized);
            Vector3 eulerAngles = targetRot.eulerAngles;
            targetRot = Quaternion.Euler(0f, eulerAngles.y, 0f);
            
            float angleDifference = Quaternion.Angle(transform.rotation, targetRot);
            bool wasRotating = isRotating;
            
            isRotating = angleDifference > ROTATION_THRESHOLD;
            
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );
            
            if (wasRotating != isRotating)
            {
                UpdateMovementAnimation();
            }
        }
        else
        {
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
    private void MoveWithScroll()
    {
        Vector3 scrollMovement = new Vector3(0f, 0f, -stageManager.scrollSpeed * Time.deltaTime);
        transform.Translate(scrollMovement, Space.World);
    }
    
    /// <summary>
    /// 独自の前進移動（z軸方向のみ）
    /// </summary>
    private void MoveForward()
    {
        Vector3 forward = transform.forward;
        forward.x = 0f; // x軸は0のまま
        forward.y = 0f; // Y軸は固定
        forward.Normalize();
        
        transform.Translate(forward * moveSpeed * Time.deltaTime, Space.World);
    }
    
    /// <summary>
    /// 指定時間後または位置がminZ/maxZに達したら移動を停止
    /// </summary>
    private IEnumerator StopMovingAfterTime(float time)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < time && isMoving)
        {
            float currentZ = transform.position.z;
            if (currentZ <= GameManager.Instance.minZ || currentZ >= GameManager.Instance.maxZ)
            {
                isMoving = false;
                UpdateMovementAnimation();
                yield break;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        if (isMoving)
        {
            isMoving = false;
            UpdateMovementAnimation();
        }
    }
    
    /// <summary>
    /// x軸を0に固定
    /// </summary>
    private void FixXPosition()
    {
        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.x) > 0.001f)
        {
            pos.x = 0f;
            transform.position = pos;
        }
    }
    
    /// <summary>
    /// z軸方向の削除判定
    /// </summary>
    private void CheckForDestroy()
    {
        if (transform.position.z < GameManager.Instance.minZ - 2f)
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region アニメーション制御
    /// <summary>
    /// 移動状態に応じてアニメーションを更新
    /// </summary>
    private void UpdateMovementAnimation()
    {
        if (animator == null) return;
        
        if (isMoving)
        {
            animator.SetTrigger("walk");
        }
        else if (isRotating)
        {
            animator.SetTrigger("turn");
        }
        else
        {
            animator.SetTrigger("idle");
        }
    }
    #endregion

    #region 攻撃システム
    /// <summary>
    /// ジャンプ攻撃の軌道移動
    /// </summary>
    private IEnumerator PerformJumpAttackMovement()
    {
        Vector3 startPosition = transform.position;
        Vector3 jumpDirection = CalculateJumpDirection();
        Vector3 endPosition = CalculateJumpEndPosition(startPosition, jumpDirection);
        
        yield return StartCoroutine(ExecuteJumpTrajectory(startPosition, endPosition));
    }
    
    /// <summary>
    /// ジャンプ方向を計算
    /// </summary>
    private Vector3 CalculateJumpDirection()
    {
        Vector3 jumpDirection = transform.forward;
        jumpDirection.x = 0f;
        jumpDirection.y = 0f;
        return jumpDirection.normalized;
    }
    
    /// <summary>
    /// ジャンプ着地点を計算
    /// </summary>
    private Vector3 CalculateJumpEndPosition(Vector3 startPos, Vector3 direction)
    {
        Vector3 endPosition = startPos + direction * jumpDistance;
        endPosition.x = 0f;
        return endPosition;
    }
    
    /// <summary>
    /// ジャンプ軌道を実行
    /// </summary>
    private IEnumerator ExecuteJumpTrajectory(Vector3 startPos, Vector3 endPos)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < jumpDuration)
        {
            if (isDead) yield break;
            
            float t = elapsedTime / jumpDuration;
            Vector3 currentPosition = Vector3.Lerp(startPos, endPos, t);
            
            // 放物線の計算
            float yOffset = jumpHeight * 4f * t * (1f - t);
            currentPosition.y = startPos.y + yOffset;
            currentPosition.x = 0f;
            
            transform.position = currentPosition;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 最終位置設定
        Vector3 finalPosition = endPos;
        finalPosition.y = startPos.y;
        finalPosition.x = 0f;
        transform.position = finalPosition;
    }
    
    /// <summary>
    /// 遠距離攻撃の実行
    /// </summary>
    private void PerformRangedAttack()
    {
        if (enemyShooter == null) 
            enemyShooter = GetComponent<IEnemyShooter>();
        
        if (enemyShooter != null)
        {
            float attackDirection = GetAttackDirectionBasedOnFacing();
            
            if (isDirectionAttack)
            {
                enemyShooter.Fire(attackDirection);
            }
            else
            {
                enemyShooter.Fire();
            }
        }
    }
    
    /// <summary>
    /// キャラクターの向きに基づいて攻撃方向を取得
    /// </summary>
    private float GetAttackDirectionBasedOnFacing()
    {
        return transform.forward.z > 0 ? ATTACK_DIRECTION_RIGHT : ATTACK_DIRECTION_LEFT;
    }
    
    /// <summary>
    /// 移動を停止して状態をリセット
    /// </summary>
    private void StopCurrentMovement()
    {
        if (isMoving)
        {
            isMoving = false;
            UpdateMovementAnimation();
        }
    }
    #endregion
}
