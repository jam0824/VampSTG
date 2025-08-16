using UnityEngine;
using System.Collections;

public class Stage5Boss : BaseBoss
{
    [Header("Phase1設定")]
    [SerializeField] private Vector3 phase1TargetPosition;
    [SerializeField] private float phase1MoveSpeed = 0.5f;
    [SerializeField] private float phase1WaitTime = 10f;

    [Header("Phase2設定")]
    [SerializeField] private Vector3 phase2FirstTargetPosition;
    [SerializeField] private float phase2FirstMoveSpeed = 0.5f;
    [SerializeField] private Vector3 phase2SecondTargetPosition;
    [SerializeField] private float phase2SecondMoveSpeed = 0.5f;

    [Header("Phase3設定")]
    [SerializeField] private Vector3 phase3FirstTargetPosition;
    [SerializeField] private float phase3FirstMoveSpeed = 5f;
    [SerializeField] private Vector3 phase3SecondTargetPosition;
    [SerializeField] private float phase3SecondMoveSpeed = 0.5f;
    [Header("Phase4設定")]
    [SerializeField] private Vector3 phase4TargetPosition;
    [SerializeField] private float phase4MoveSpeed = 1f;
    [SerializeField] private float phase4WaitTime = 10f;


    [Header("Phase5設定")]
    [SerializeField] private Vector3 phase5FirstTargetPosition;
    [SerializeField] private float phase5FirstMoveSpeed = 1f;
    [SerializeField] private Vector3 phase5SecondTargetPosition;
    [SerializeField] private float phase5SecondMoveSpeed = 1f;

    [Header("Phase6設定")]
    [SerializeField] private Vector3 phase6FirstTargetPosition;
    [SerializeField] private float phase6FirstMoveSpeed = 5f;
    [SerializeField] private Vector3 phase6SecondTargetPosition;
    [SerializeField] private float phase6SecondMoveSpeed = 1f;

    [Header("移動イージング設定")]
    [SerializeField] private EaseType defaultEaseType = EaseType.EaseInOutQuad;  // デフォルトのイージングタイプ


    
    [Header("フェーズ管理")]
    [SerializeField] private int currentPhase = 2;  // 現在のフェーズ（1-6）
    [SerializeField] private float phaseTransitionWait = 0.0f;  // フェーズ間の待機時間
    private bool isPhaseRunning = false;  // フェーズが実行中かどうか
    
    /// <summary>
    /// イージングの種類
    /// </summary>
    public enum EaseType
    {
        Linear,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
        PlayEntry();
        
    }

    // Update is called once per frame
    void Update()
    {
        base.Update(); // 親クラスのUpdate処理を呼び出す
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
        Debug.Log("Stage5Boss 出現演出開始");
        gameObject.SetActive(true);
        
        // y座標をGroundYまで上昇させる演出
        yield return StartCoroutine(
            MoveToPositionWithEasing(transform.position, phase1TargetPosition, phase1MoveSpeed, EaseType.EaseOutSine)
        );
        
        // 独自の処理：攻撃パターン開始
        bossHpBar.StartFadeIn(2f);
        stageManager.scrollSpeed = 0f;  //ボス出現時にスクロールを止める
        yield return new WaitForSeconds(3f);
        SoundManager.Instance.PlayBGM(GetEntryBGM(), GetEntryBGMVolume());
        yield return new WaitForSeconds(2f);
        
        // フェーズ管理システムを開始
        isStart = true;
        StartCoroutine(PhaseManagerCoroutine());
        yield return null;
    }

    /// <summary>
    /// 中央制御型フェーズ管理システム
    /// </summary>
    /// <returns></returns>
    private IEnumerator PhaseManagerCoroutine()
    {
        Debug.Log("Stage5Boss フェーズ管理システム開始");
        
        while (!isDead && isStart)
        {
            isPhaseRunning = true;
            Debug.Log($"Phase {currentPhase} 開始");
            
            // 現在のフェーズに応じた処理を実行
            switch (currentPhase)
            {
                case 1:
                    yield return StartCoroutine(Phase1Coroutine());
                    break;
                case 2:
                    yield return StartCoroutine(Phase2Coroutine());
                    break;
                case 3:
                    yield return StartCoroutine(Phase3Coroutine());
                    break;
                case 4:
                    yield return StartCoroutine(Phase4Coroutine());
                    break;
                case 5:
                    yield return StartCoroutine(Phase5Coroutine());
                    break;
                case 6:
                    yield return StartCoroutine(Phase6Coroutine());
                    break;
                default:
                    Debug.LogWarning($"不正なフェーズ番号: {currentPhase}");
                    currentPhase = 1; // リセット
                    break;
            }
            
            isPhaseRunning = false;
            Debug.Log($"Phase {currentPhase} 完了");
            
            // 次のフェーズに進む（1-6の循環）
            currentPhase = (currentPhase % 6) + 1;
            
            // フェーズ間の待機時間
            yield return new WaitForSeconds(phaseTransitionWait);
        }
        
        Debug.Log("Stage5Boss フェーズ管理システム終了");
    }

    /// <summary>
    /// フェーズ1：指定位置に移動して待機
    /// </summary>
    protected IEnumerator Phase1Coroutine()
    {
        yield return StartCoroutine(
            MoveToPositionWithEasing(transform.position, phase1TargetPosition, phase1MoveSpeed, EaseType.EaseOutSine)
        );
        yield return new WaitForSeconds(phase1WaitTime);
    }

    /// <summary>
    /// フェーズ2：2つの位置を順番に移動
    /// </summary>
    protected IEnumerator Phase2Coroutine()
    {
        yield return StartCoroutine(
            MoveToPositionWithEasing(transform.position, phase2FirstTargetPosition, phase2FirstMoveSpeed, EaseType.EaseInOutSine)
        );
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(
            MoveToPositionWithEasing(transform.position, phase2SecondTargetPosition, phase2SecondMoveSpeed, EaseType.EaseInSine)
        );
    }

    /// <summary>
    /// フェーズ3：2つの位置を順番に移動
    /// </summary>
    protected IEnumerator Phase3Coroutine()
    {
        yield return StartCoroutine(
            MoveToPositionWithEasing(transform.position, phase3FirstTargetPosition, phase3FirstMoveSpeed, EaseType.Linear)
        );
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(
            MoveToPositionWithEasing(transform.position, phase3SecondTargetPosition, phase3SecondMoveSpeed, EaseType.EaseOutSine)
        );
    }

    /// <summary>
    /// フェーズ4：指定位置に移動して待機
    /// </summary>
    protected IEnumerator Phase4Coroutine()
    {
        yield return StartCoroutine(
            MoveToPositionWithEasing(transform.position, phase4TargetPosition, phase4MoveSpeed, EaseType.EaseOutSine)
        );
        yield return new WaitForSeconds(phase4WaitTime);
    }

    /// <summary>
    /// フェーズ5：2つの位置を順番に移動
    /// </summary>
    protected IEnumerator Phase5Coroutine()
    {
        yield return StartCoroutine(
            MoveToPositionWithEasing(transform.position, phase5FirstTargetPosition, phase5FirstMoveSpeed, EaseType.EaseInOutSine)
        );
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(
            MoveToPositionWithEasing(transform.position, phase5SecondTargetPosition, phase5SecondMoveSpeed, EaseType.EaseInSine)
        );
    }

    /// <summary>
    /// フェーズ6：2つの位置を順番に移動
    /// </summary>
    protected IEnumerator Phase6Coroutine()
    {
        yield return StartCoroutine(
            MoveToPositionWithEasing(transform.position, phase6FirstTargetPosition, phase6FirstMoveSpeed, EaseType.Linear)
        );
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(
            MoveToPositionWithEasing(transform.position, phase6SecondTargetPosition, phase6SecondMoveSpeed, EaseType.EaseOutSine)
        );
    }


    /// <summary>
    /// 指定した開始位置から目標位置まで指定速度で移動するメソッド（線形補間）
    /// </summary>
    /// <param name="startPosition">開始位置</param>
    /// <param name="targetPosition">目標位置</param>
    /// <param name="speed">移動速度</param>
    /// <returns>移動完了までのコルーチン</returns>
    public IEnumerator MoveToPosition(Vector3 startPosition, Vector3 targetPosition, float speed)
    {
        return MoveToPositionWithEasing(startPosition, targetPosition, speed, defaultEaseType);
    }

    /// <summary>
    /// 指定した開始位置から目標位置まで指定速度とイージングで移動するメソッド
    /// </summary>
    /// <param name="startPosition">開始位置</param>
    /// <param name="targetPosition">目標位置</param>
    /// <param name="speed">移動速度</param>
    /// <param name="easeType">イージングタイプ</param>
    /// <returns>移動完了までのコルーチン</returns>
    public IEnumerator MoveToPositionWithEasing(Vector3 startPosition, Vector3 targetPosition, float speed, EaseType easeType)
    {
        // 現在位置を開始位置に設定
        transform.position = startPosition;
        
        // 移動距離を計算
        float distance = Vector3.Distance(startPosition, targetPosition);
        
        // 移動にかかる時間を計算
        float moveDuration = distance / speed;
        
        float elapsedTime = 0f;
        
        // 移動処理
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / moveDuration;
            
            // イージング関数を適用
            float easedProgress = ApplyEasing(normalizedTime, easeType);
            
            // イージングされた進行度で移動
            transform.position = Vector3.Lerp(startPosition, targetPosition, easedProgress);
            
            yield return null;
        }
        
        // 最終位置を確実に設定
        transform.position = targetPosition;
    }

    /// <summary>
    /// イージング関数を適用する
    /// </summary>
    /// <param name="t">正規化された時間（0-1）</param>
    /// <param name="easeType">イージングタイプ</param>
    /// <returns>イージングが適用された値</returns>
    private float ApplyEasing(float t, EaseType easeType)
    {
        // tを0-1の範囲にクランプ
        t = Mathf.Clamp01(t);
        
        switch (easeType)
        {
            case EaseType.Linear:
                return t;
                
            case EaseType.EaseInQuad:
                return t * t;
                
            case EaseType.EaseOutQuad:
                return t * (2f - t);
                
            case EaseType.EaseInOutQuad:
                return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
                
            case EaseType.EaseInCubic:
                return t * t * t;
                
            case EaseType.EaseOutCubic:
                float f = t - 1f;
                return f * f * f + 1f;
                
            case EaseType.EaseInOutCubic:
                return t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;
                
            case EaseType.EaseInSine:
                return 1f - Mathf.Cos(t * Mathf.PI / 2f);
                
            case EaseType.EaseOutSine:
                return Mathf.Sin(t * Mathf.PI / 2f);
                
            case EaseType.EaseInOutSine:
                return -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
                
            default:
                return t; // Linear fallback
        }
    }



    /// <summary>
    /// 現在のフェーズ番号を取得
    /// </summary>
    /// <returns>現在のフェーズ番号（1-6）</returns>
    public int GetCurrentPhase()
    {
        return currentPhase;
    }

    /// <summary>
    /// フェーズが実行中かどうかを取得
    /// </summary>
    /// <returns>フェーズが実行中の場合true</returns>
    public bool IsPhaseRunning()
    {
        return isPhaseRunning;
    }

    /// <summary>
    /// 指定したフェーズに強制的に移行する（デバッグ用）
    /// </summary>
    /// <param name="phaseNumber">移行先のフェーズ番号（1-6）</param>
    public void ForceSetPhase(int phaseNumber)
    {
        if (phaseNumber >= 1 && phaseNumber <= 6)
        {
            currentPhase = phaseNumber;
            Debug.Log($"フェーズを強制的に {phaseNumber} に設定しました");
        }
        else
        {
            Debug.LogWarning($"不正なフェーズ番号: {phaseNumber}。1-6の範囲で指定してください。");
        }
    }
}
