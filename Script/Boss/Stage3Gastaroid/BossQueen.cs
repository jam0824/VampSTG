using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossQueen : BaseBoss
{
    [Header("スポーン")]
    [SerializeField] private GameObject eggPrefab;
    [SerializeField] private Transform eggSpawnPoint;

    [Header("移動設定")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected float turnDuration = 1f;
    [SerializeField] protected float waitTime = 1f; // 移動後の待機時間
    [Header("BGM設定")]
    [SerializeField] private AudioClip bgm;
    [SerializeField] private float bgmVol = 0.8f;
    protected override AudioClip GetEntryBGM() => bgm;
    protected override float GetEntryBGMVolume() => bgmVol;

    private bool isMoving = false; // 移動中フラグ

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
        animator.SetTrigger("roar");
        
        // 出現演出の設定
        float entryDuration = 10f;           // 出現にかかる時間
        float targetZ = 7f;
        float roarInterval = 3f;
        
        float elapsed = 0f;
        while (elapsed < entryDuration)
        {
            elapsed += Time.deltaTime;
            // 背景スクロールスピードに合わせた移動
            Vector3 scrollMovement = new Vector3(0f, 0f, -stageManager.scrollSpeed * Time.deltaTime);
            // 両方の移動を適用
            transform.Translate(scrollMovement, Space.World);
            Vector3 pos = transform.position;
            if(elapsed >= roarInterval){
                animator.SetTrigger("roar");
                elapsed = 0f;
            }
            if(pos.z <= targetZ) break;
            yield return null;
        }
        
        // 独自の処理：攻撃パターン開始
        bossHpBar.StartFadeIn(3f);
        stageManager.scrollSpeed = 0f;  //ボス出現時にスクロールを止める
        SoundManager.Instance.PlayBGM(GetEntryBGM(), GetEntryBGMVolume());
        yield return new WaitForSeconds(attackInterval);
        StartCoroutine(AttackCoroutine());
        isStart = true;
        yield return null;
    }

    protected override void Update()
    {
        base.Update();
    }

    private IEnumerator AttackCoroutine()
    {
        while (!isDead)
        {
            // 50%の確率で行動を選択
            float randomValue = Random.Range(0f, 1f);

            
            if (randomValue < 0.25f)
            {
                yield return StartCoroutine(MoveAndTurn());
            }
            else if (randomValue < 0.5f)
            {
                yield return StartCoroutine(TailAttackCoroutine());
            }
            else if (randomValue < 0.75f)
            {
                
            }
            else
            {
                yield return StartCoroutine(JumpAttackCoroutine());
            }
            

            // 攻撃完了後の待機
            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator RoarCoroutine(float waitTime)
    {
        animator.SetTrigger("roar");
        SpawnEgg();
        yield return new WaitForSeconds(waitTime);
    }
    private IEnumerator TurnCoroutine()
    {
        animator.SetTrigger("turn");
        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 180f, 0f);
        float elapsed = 0f;
        while (elapsed < turnDuration)
        {
            if (isDead) yield break;

            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / turnDuration);
            yield return null;
        }
    }

    /// <summary>
    /// ジャンプ攻撃
    /// </summary>
    /// <returns></returns>
    private IEnumerator JumpAttackCoroutine()
    {
        yield return StartCoroutine(RoarCoroutine(2.5f));
        animator.SetTrigger("jumpAttack");
        float jumpDistance = 10f;
        float jumpSpeed = 20f;
        float currentZ = transform.position.z;
        float targetZ = currentZ > 0f ? currentZ - jumpDistance : currentZ + jumpDistance;
        Vector3 targetPos = new Vector3(transform.position.x, transform.position.y, targetZ);
        // 移動処理
        while (Mathf.Abs(transform.position.z - targetZ) > 0.01f)
        {
            if (isDead) yield break;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                jumpSpeed * Time.deltaTime
            );

            yield return null;
        }
        yield return StartCoroutine(TurnCoroutine());
    }

    /// <summary>
    /// 移動と回転
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveAndTurn()
    {
        if (isDead) yield break;
        yield return StartCoroutine(HandAttackCoroutine());

        isMoving = true;
        animator.SetTrigger("run");

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

        if (isDead) yield break;

        // 回転処理
        yield return StartCoroutine(TurnCoroutine());
        isMoving = false;
    }

    private IEnumerator HandAttackCoroutine()
    {
        animator.SetTrigger("attack");
        yield return new WaitForSeconds(1.5f);
    }

    private IEnumerator TailAttackCoroutine()
    {
        animator.SetTrigger("tailAttack");
        SpawnEgg();
        yield return new WaitForSeconds(1.5f);
        
    }
    private void SpawnEgg()
    {
        GameObject egg = Instantiate(eggPrefab, eggSpawnPoint.position, eggSpawnPoint.rotation);
    }
}
