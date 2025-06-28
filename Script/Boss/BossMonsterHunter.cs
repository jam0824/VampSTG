using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class BossMonsterHunter : BaseBoss
{
    [Header("モンスター設定")]
    [SerializeField] private float waitTime = 4f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private float stopDistance = 1f;

    [Header("攻撃設定")]
    [SerializeField] private GameObject FirePointLeftHand;
    [SerializeField] private GameObject FirePointRightHand;
    [SerializeField] private GameObject FirePointHead;

    [Header("フェーズ2設定")]
    [SerializeField] private float phase2HpRate = 0.75f;
    [SerializeField] private float groundY = -2f;

    [Header("BGM設定")]
    [SerializeField] private AudioClip bgm;
    [SerializeField] private float bgmVol = 0.8f;
    protected override AudioClip GetEntryBGM() => bgm;
    protected override float GetEntryBGMVolume() => bgmVol;

    private bool isMoving = false; // 移動中フラグ
    private Transform playerTransform;
    private bool isSwim = false;
    private bool isPhase2 = false; // 第2フェーズフラグ
    private Coroutine swimAttackCoroutine; // SwimAttackCoroutineの参照
    

    protected override void Start()
    {
        base.Start();

        // animatorを初期化
        if (animator == null)
            animator = GetComponent<Animator>();

        PlayEntry();
    }

    /// <summary>
    /// ボス出現の初期化処理
    /// </summary>
    public override void PlayEntry()
    {
        base.PlayEntry(); // 親クラスの処理を実行（EntryCoroutineを開始）
    }

    /// <summary>
    /// ボス出現の演出
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator EntryCoroutine()
    {
        Debug.Log("BossQueen 出現演出開始");
        gameObject.SetActive(true);
        
        
        // 独自の処理：攻撃パターン開始
        bossHpBar.StartFadeIn(2f);
        stageManager.scrollSpeed = 0f;  //ボス出現時にスクロールを止める
        yield return new WaitForSeconds(3f);
        SoundManager.Instance.PlayBGM(GetEntryBGM(), GetEntryBGMVolume());
        animator.SetTrigger("startSwim");
        yield return new WaitForSeconds(2f);
        isStart = true;
        isSwim = true;
        swimAttackCoroutine = StartCoroutine(SwimAttackCoroutine());
        yield return null;
    }

    protected override void Update()
    {
        base.Update();
        if(isSwim) Phase1Movement();
        float hpRate = hp / maxHp;
        PhaseSwitcher(hpRate);
    }



    /// <summary>
    /// フェーズ切り替え
    /// </summary>
    /// <param name="hpRate"></param>
    private void PhaseSwitcher(float hpRate)
    {
        if (hpRate <= phase2HpRate && !isPhase2)
        {
            isPhase2 = true;
            StopCoroutine(swimAttackCoroutine);
            isSwim = false;
            StartCoroutine(MoveToYZero());
        }
    }

    /// <summary>
    /// Phase1の攻撃パターン
    /// </summary>
    /// <returns></returns>
    private IEnumerator SwimAttackCoroutine()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(waitTime);
            float randomValue = Random.Range(0f, 1f);
            if(randomValue < 0.5f)
            {
                animator.SetTrigger("attack1");
            }
            else if(randomValue < 0.75f)
            {
                animator.SetTrigger("attack2");
            }
            else
            {
                animator.SetTrigger("attack3");
            }
        }
    }

    private IEnumerator AttackCoroutine()
    {
        while (!isDead)
        {
            // 50%の確率で行動を選択
            float randomValue = Random.Range(0f, 1f);

            
            if (randomValue < 0.2f)
            {
                yield return StartCoroutine(HandAttackCoroutine());
            }
            else if (randomValue < 0.5f)
            {
                yield return StartCoroutine(HandAttackCoroutine());
            }
            else if (randomValue < 0.7f)
            {
                yield return StartCoroutine(HandAttackCoroutine());
            }
            else
            {
                yield return StartCoroutine(HandAttackCoroutine());
            }
            

            // 攻撃完了後の待機
            yield return new WaitForSeconds(waitTime);
        }
    }

    void Phase1Movement()
    {
        if (playerTransform == null) {
            playerTransform = core.transform;
        }

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

    /// <summary>
    /// 移動と回転
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveAndTurn()
    {
        isMoving = true;

        float currentZ = transform.position.z;
        float targetZ = currentZ > 0f ? -6f : 6f;
        Vector3 targetPos = new Vector3(transform.position.x, transform.position.y, targetZ);

        // 移動処理
        while (Mathf.Abs(transform.position.z - targetZ) > 0.01f)
        {
            if (isDead) yield break;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }
        isMoving = false;
    }

    private IEnumerator HandAttackCoroutine()
    {
        //animator.SetTrigger("attack1");
        yield return null;
    }

    /// <summary>
    /// 左腕の攻撃(アニメーションから呼ばれる)
    /// </summary>
    public void FireLeftHand()
    {
        FirePointLeftHand.GetComponent<IEnemyShooter>().Fire();
    }

    /// <summary>
    /// 右腕の攻撃(アニメーションから呼ばれる)
    /// </summary>
    public void FireRightHand()
    {
        FirePointRightHand.GetComponent<IEnemyShooter>().Fire();
    }

    /// <summary>
    /// 頭の攻撃(アニメーションから呼ばれる)
    /// </summary>
    public void FireHead()
    {
        FirePointHead.GetComponent<IEnemyShooter>().Fire();
    }

    /// <summary>
    /// 地面レベルまで移動し、向きを調整してフェーズ2を開始する
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveToYZero()
    {
        Debug.Log("Phase2開始：地面レベルへ移動開始");
        
        // 1. 地面レベルまで移動
        yield return StartCoroutine(MoveToGroundLevel());
        
        // 2. 向きを調整
        yield return StartCoroutine(AdjustDirectionBasedOnZ());
        
        // 3. フェーズ2開始処理
        StartPhase2();
    }

    /// <summary>
    /// 地面レベル（groundY）まで移動する
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveToGroundLevel()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(startPos.x, groundY, startPos.z);
        
        while (Mathf.Abs(transform.position.y - targetPos.y) > 0.01f)
        {
            if (isDead) yield break;
            
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );
            
            yield return null;
        }
    }

    /// <summary>
    /// z座標に基づいて向きを調整する
    /// </summary>
    /// <returns></returns>
    private IEnumerator AdjustDirectionBasedOnZ()
    {
        Vector3 targetDirection = GetDirectionBasedOnZ();
        Quaternion targetRotation = CalculateTargetRotation(targetDirection);
        
        yield return StartCoroutine(RotateToDirection(targetRotation));
    }

    /// <summary>
    /// z座標に基づいて目標方向を取得する
    /// </summary>
    /// <returns></returns>
    private Vector3 GetDirectionBasedOnZ()
    {
        if (transform.position.z < 0f)
        {
            // zがマイナス：前向き
            return Vector3.forward;
        }
        else
        {
            // zがプラス：後ろ向き
            return Vector3.back;
        }
    }

    /// <summary>
    /// 目標回転を計算する（x軸角度は0に固定）
    /// </summary>
    /// <param name="_targetDirection">目標方向</param>
    /// <returns></returns>
    private Quaternion CalculateTargetRotation(Vector3 _targetDirection)
    {
        Quaternion targetRotation = Quaternion.LookRotation(_targetDirection);
        return Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
    }

    /// <summary>
    /// 指定された方向にスムーズに回転する
    /// </summary>
    /// <param name="_targetRotation">目標回転</param>
    /// <returns></returns>
    private IEnumerator RotateToDirection(Quaternion _targetRotation)
    {
        while (Quaternion.Angle(transform.rotation, _targetRotation) > 1f)
        {
            if (isDead) yield break;
            
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                _targetRotation,
                rotateSpeed * Time.deltaTime
            );
            
            yield return null;
        }
        
        // 最終的な回転を設定
        transform.rotation = _targetRotation;
    }

    /// <summary>
    /// フェーズ2開始処理
    /// </summary>
    private void StartPhase2()
    {
        animator.SetTrigger("phase2");
        Debug.Log("Phase2移動完了");
        
        // 第2フェーズの攻撃パターン開始
        StartCoroutine(AttackCoroutine());
    }
}
