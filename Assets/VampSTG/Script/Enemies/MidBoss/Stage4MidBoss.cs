using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ステージ4のミッドボス - 上下にふわふわ動きながら2パターンの攻撃を行う
/// </summary>
public class Stage4MidBoss : BaseEnemy
{
    [Header("移動設定")]
    [SerializeField] float moveSpeed = 0.5f;
    [SerializeField] float targetZ = 2f;          // 停止するZ座標
    
    [Header("上下移動設定")]
    [SerializeField] float verticalMoveSpeed = 1f;  // 上下移動の速度
    [SerializeField] float verticalRange = 2f;      // 上下移動の幅

    [Header("射撃設定")]
    [SerializeField] List<GameObject> enemyShooterObjects = new List<GameObject>();
    
    [Header("攻撃パターン設定")]
    [SerializeField] float attack2Duration = 5f;  // attack2のアニメーション再生時間
    
    [Header("逃げる設定")]
    [SerializeField] float escapeTime = 30f;     // 逃げる時間（秒）
    [SerializeField] float escapeSpeed = 3f;     // 逃げる速度
    [SerializeField] float escapeDestroyZ = -20f; // この座標で削除

    private Vector3 basePosition;                // 基準位置
    private bool isMovementEnabled = true;       // 移動が有効かどうか
    private bool isAttack2Playing = false;       // attack2アニメーション再生中かどうか
    private bool hasReachedTargetZ = false;      // 目標Z座標に到達したかどうか
    private bool isEscaping = false;             // 逃げ中かどうか
    private float verticalTime = 0f;             // 上下移動の時間管理用
    private StageManager stageManager;           // StageManagerの参照をキャッシュ

    protected override void OnStart()
    {
        
        // 基準位置を初期位置に設定
        basePosition = transform.position;
        
        // StageManagerの参照を取得してキャッシュ（GameObject.Findの方が高速）
        GameObject stageManagerObj = GameObject.Find("StageManager");
        if (stageManagerObj != null)
        {
            stageManager = stageManagerObj.GetComponent<StageManager>();
        }
        
        if (stageManager == null)
        {
            Debug.LogWarning("StageManagerが見つかりません");
        }
    }

    protected override void Update()
    {
        // 死亡済みまたは逃げ中の場合は時間チェックをスキップ
        if (!isDead && !isEscaping)
        {
            // StageManagerの経過時間をチェックして逃げる処理を開始
            if (stageManager != null && stageManager.allElapsedTime >= escapeTime)
            {
                StartEscape();
            }
        }

        // 親クラスのUpdate処理を呼び出し
        base.Update();
    }

    /// <summary>
    /// 逃げる処理を開始
    /// </summary>
    private void StartEscape()
    {
        isEscaping = true;
        isMovementEnabled = false; // 通常の移動を停止
        
        // 攻撃も停止（BaseEnemyのisAttackフラグを使用）
        isAttack = false;
        isAttackAnimation = false;
        isAttack2Playing = false;
        
        // 全てのコルーチンを停止（攻撃コルーチンも含む）
        StopAllCoroutines();
    }

    protected override void HandleMovement()
    {
        // 逃げ中の場合は逃げる処理のみ実行
        if (isEscaping)
        {
            HandleEscapeMovement();
            return;
        }

        if (!isMovementEnabled) return;

        // ─── 目標Z座標まで移動 ───
        if (!hasReachedTargetZ)
        {
            if (transform.position.z <= targetZ)
            {
                // 目標Z座標に到達
                Vector3 currentPos = transform.position;
                currentPos.z = targetZ;
                transform.position = currentPos;
                basePosition = currentPos;
                hasReachedTargetZ = true;
                return;
            }

            // 目標Z座標まで前進
            Vector3 moveDirection = Vector3.back; // Z軸負方向に移動
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
            return;
        }

        // ─── 上下移動（目標Z座標到達後のみ） ───
        // サインウェーブを使用してスムーズな上下運動を実現
        verticalTime += verticalMoveSpeed * Time.deltaTime;
        
        // サインウェーブによる滑らかな上下移動
        float sineValue = Mathf.Sin(verticalTime);
        float targetY = basePosition.y + sineValue * verticalRange;
        
        // 境界チェック
        targetY = Mathf.Clamp(targetY, GameManager.Instance.minY, GameManager.Instance.maxY);
        
        Vector3 newPosition = transform.position;
        newPosition.y = targetY;
        transform.position = newPosition;
    }

    /// <summary>
    /// 逃げる移動処理
    /// </summary>
    private void HandleEscapeMovement()
    {
        // Z軸マイナス方向に移動
        Vector3 escapeDirection = Vector3.back;
        transform.position += escapeDirection * escapeSpeed * Time.deltaTime;
        
        // 削除座標に到達したら削除
        if (transform.position.z <= escapeDestroyZ)
        {
            // アイテムドロップを無効化（逃げた場合はアイテムを落とさない）
            item = null;
            Destroy(gameObject);
        }
    }

    protected override IEnumerator AttackCoroutine()
    {
        while (!isDead && !isEscaping)
        {
            yield return new WaitForSeconds(attackInterval);
            
            // 逃げ中または死亡している場合は攻撃処理を停止
            if (isEscaping || isDead)
            {
                yield break;
            }
            
            // キャラクターが範囲外にいる場合は攻撃処理をスキップ
            if ((GameManager.Instance.minZ > transform.position.z) || 
                (GameManager.Instance.maxZ < transform.position.z) ||
                (GameManager.Instance.minY > transform.position.y) || 
                (GameManager.Instance.maxY < transform.position.y))
            {
                yield return null;
                continue;
            }

            // ランダムで攻撃パターンを選択
            bool useAttack2 = Random.value < 0.5f; // 50%の確率で attack2

            if (useAttack2)
            {
                yield return StartCoroutine(PerformAttack2());
            }
            else
            {
                yield return StartCoroutine(PerformAttack1());
            }
        }
    }

    /// <summary>
    /// 攻撃パターン1 - Stage3MidBossと同じ内容
    /// </summary>
    private IEnumerator PerformAttack1()
    {
        if (animator != null)
            animator.SetTrigger("attack");
        
        isAttackAnimation = true;
        yield return new WaitForSeconds(attackAnimationWait);
        
        // 射撃実行
        foreach (var shooterObj in enemyShooterObjects)
        {
            if (shooterObj != null && shooterObj.TryGetComponent<IEnemyShooter>(out var shooter))
            {
                shooter.Fire(attackDirection);
            }
        }
        
        yield return new WaitForSeconds(1f);
        isAttackAnimation = false;
    }

    /// <summary>
    /// 攻撃パターン2 - attack2アニメーション + 移動停止
    /// </summary>
    private IEnumerator PerformAttack2()
    {
        if (animator != null)
            animator.SetTrigger("attack2");
        
        // 移動を無効化
        isMovementEnabled = false;
        isAttack2Playing = true;
        isAttackAnimation = true;
        
        // attack2のアニメーション再生時間だけ待機
        yield return new WaitForSeconds(attack2Duration);
        
        // 移動を再有効化
        isMovementEnabled = true;
        isAttack2Playing = false;
        isAttackAnimation = false;
    }

    protected override void Explosion(float maxHp)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 pos = transform.position;
            pos.x = 1f; // 少し画面の手前に出す
            // y は 0 ～ 6 の範囲
            pos.y += (Random.value - 0.5f) * 2f;
            // z を ±1 の範囲でランダムにずらす
            pos.z += (Random.value - 0.5f) * 2f;

            // ランダム爆発
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

    /// <summary>
    /// 移動が有効かどうかを取得（デバッグ用）
    /// </summary>
    public bool IsMovementEnabled()
    {
        return isMovementEnabled;
    }

    /// <summary>
    /// attack2アニメーション再生中かどうかを取得（デバッグ用）
    /// </summary>
    public bool IsAttack2Playing()
    {
        return isAttack2Playing;
    }

    /// <summary>
    /// 外部から移動を強制停止/再開
    /// </summary>
    public void SetMovementEnabled(bool enabled)
    {
        isMovementEnabled = enabled;
    }

    /// <summary>
    /// 逃げ中かどうかを取得（デバッグ用）
    /// </summary>
    public bool IsEscaping()
    {
        return isEscaping;
    }

    /// <summary>
    /// 外部から逃げ処理を強制開始（デバッグ用）
    /// </summary>
    public void ForceEscape()
    {
        StartEscape();
    }
} 