using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class BossQueen : BaseBoss
{
    [Header("スポーン")]
    [SerializeField] private GameObject eggPrefab;
    [SerializeField] private Transform eggSpawnPoint;

    [Header("投げるオブジェクト")]
    [SerializeField] private List<GameObject> listThrowObjects;
    [SerializeField] private Transform leftThrowPoint;
    [SerializeField] private Transform rightThrowPoint;
    [SerializeField] private float throwSpeed = 10f;
    [SerializeField] private float throwInterval = 1f;
    [SerializeField] private float throwDuration = 1f;
    [SerializeField] private float throwDelay = 1f;
    [Header("FirePoints")]
    [SerializeField] private GameObject leftFirePoint;
    [SerializeField] private GameObject rightFirePoint;
    
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
        animator.SetTrigger("walk");
        
        // 出現演出の設定
        float entryDuration = 10f;           // 出現にかかる時間
        float targetZ = 7f;
        float addMoveSpeed = 2.5f;
        
        float elapsed = 0f;
        while (elapsed < entryDuration)
        {
            elapsed += Time.deltaTime;
            // 背景スクロールスピードに合わせた移動
            Vector3 scrollMovement = new Vector3(0f, 0f, -stageManager.scrollSpeed * Time.deltaTime);
            Vector3 addMove = new Vector3(0f, 0f, -addMoveSpeed * Time.deltaTime);
            // 両方の移動を適用
            transform.Translate(scrollMovement + addMove, Space.World);
            Vector3 pos = transform.position;
            if(pos.z <= targetZ) break;
            yield return null;
        }
        animator.SetTrigger("roar");
        
        // 独自の処理：攻撃パターン開始
        bossHpBar.StartFadeIn(2f);
        stageManager.scrollSpeed = 0f;  //ボス出現時にスクロールを止める
        yield return new WaitForSeconds(3f);
        SoundManager.Instance.PlayBGM(GetEntryBGM(), GetEntryBGMVolume());
        isStart = true;
        StartCoroutine(AttackCoroutine());
        
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

            
            if (randomValue < 0.2f)
            {
                yield return StartCoroutine(MoveAndTurn());
                yield return StartCoroutine(TurnCoroutine());
            }
            else if (randomValue < 0.5f)
            {
                yield return StartCoroutine(JumpAttackCoroutine());
                yield return StartCoroutine(TurnCoroutine());
            }
            else if (randomValue < 0.7f)
            {
                yield return StartCoroutine(HandAttackCoroutine());
            }
            else
            {
                yield return StartCoroutine(TailAttackCoroutine());
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
        
        // 最終角度を確実に設定
        transform.rotation = endRot;
    }

    /// <summary>
    /// ジャンプ攻撃
    /// </summary>
    /// <returns></returns>
    private IEnumerator JumpAttackCoroutine()
    {
        yield return StartCoroutine(RoarCoroutine(2.5f));
        animator.SetTrigger("jumpAttack");
        isMoving = true;
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
        isMoving = false;
    }

    /// <summary>
    /// 移動と回転
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveAndTurn()
    {
        yield return StartCoroutine(RoarCoroutine(2.5f));

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
        isMoving = false;
    }

    private IEnumerator HandAttackCoroutine()
    {
        animator.SetTrigger("attack");
        yield return new WaitForSeconds(0.3f);
        ThrowObject(leftThrowPoint);
        leftFirePoint.GetComponent<IEnemyShooter>().Fire();
        yield return new WaitForSeconds(0.4f);
        ThrowObject(rightThrowPoint);
        rightFirePoint.GetComponent<IEnemyShooter>().Fire();

        yield return new WaitForSeconds(1.5f);
    }
    private void ThrowObject(Transform throwPoint){
        // listThrowObjectsからランダムでオブジェクトを選択して投げる
        if (listThrowObjects != null && listThrowObjects.Count > 0 && throwPoint != null)
        {
            // コアの位置を取得
            if (GameManager.Instance.playerCore != null)
            {
                Vector3 corePosition = GameManager.Instance.playerCore.transform.position;
                
                // ランダムでオブジェクトを選択
                int randomIndex = Random.Range(0, listThrowObjects.Count);
                GameObject throwObject = listThrowObjects[randomIndex];

                Vector3 pos = throwPoint.position;
                pos.x = 0f;
                
                // 投げるオブジェクトを生成
                GameObject thrownObj = Instantiate(throwObject, pos, throwPoint.rotation);
                
                // 投げる方向を計算
                Vector3 direction = (corePosition - throwPoint.position).normalized;
                
                // オブジェクトを移動方向に向ける
                if (direction != Vector3.zero)
                {
                    thrownObj.transform.LookAt(thrownObj.transform.position + direction);
                }
                
                // Rigidbodyがある場合は完全に無効化
                Rigidbody rb = thrownObj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }
                
                // 弾丸のような直線移動を開始
                StartCoroutine(MoveBulletStraight(thrownObj, direction));
            }
        }
    }

    /// <summary>
    /// 弾丸のような直線移動
    /// </summary>
    private IEnumerator MoveBulletStraight(GameObject bullet, Vector3 direction)
    {
        float elapsed = 0f;
        float maxTime = 10f; // 最大飛行時間
        
        while (bullet != null && elapsed < maxTime)
        {
            // 弾丸のように真っすぐ移動
            bullet.transform.Translate(direction * throwSpeed * Time.deltaTime, Space.World);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 時間切れまたはオブジェクトが破壊されていない場合は削除
        if (bullet != null)
        {
            Destroy(bullet);
        }
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
        egg.transform.SetParent(stageManager.enemyPool.transform);
    }
}
