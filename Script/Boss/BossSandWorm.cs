using UnityEngine;
using System.Collections;

public class BossSandWorm : BaseBoss
{
    [Header("BGM設定")]
    [SerializeField] private AudioClip bgm;
    [SerializeField] private float bgmVol = 0.8f;

    [Header("向き調整")]
    [SerializeField] private float rotateSpeed = 5f;   // 回転の滑らかさ

    NWayShooter nWayShooter;

    int attackPattern = -1;
    bool canRoutate = true;

    protected override AudioClip GetEntryBGM() => bgm;
    protected override float GetEntryBGMVolume() => bgmVol;

    protected override void Start()
    {
        base.Start();
        nWayShooter = GetComponent<NWayShooter>();

    }

    protected override void Update()
    {
        base.Update();

        // ─── 毎フレーム向き更新 ───
        UpdateFacing();

        // ─── 攻撃パターン切り替え ───
        SwitchAttackPattern();
    }

    /// <summary>
    /// プレイヤーより Z 軸プラス側なら左向き（Y 180°）、マイナス側なら右向き（Y 0°）に滑らかに回転
    /// </summary>
    private void UpdateFacing()
    {
        if (core == null) return;
        if (!canRoutate) return;

        // プレイヤーとの差分
        float diffZ = transform.position.z - core.transform.position.z;

        // 目標回転（Y 180° = 左向き、Y 0° = 右向き）
        Quaternion targetRot = diffZ > 0
            ? Quaternion.Euler(0f, 180f, 0f)
            : Quaternion.Euler(0f, 0f, 0f);

        // 滑らかに補間
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * rotateSpeed
        );
    }

    void SwitchAttackPattern()
    {
        float hpPer = hp / maxHp;
        if (isStart)
        {
            if (attackPattern == -1)
            {
                attackPattern = 0;
                StopAllCoroutines();
                StartCoroutine(Attack0Coroutine());
            }
            else if (hpPer < 0.66f && attackPattern == 0)
            {
                attackPattern = 1;
                StopAllCoroutines();
                StartCoroutine(Attack1Coroutine());
            }
            else if (hpPer < 0.33f && attackPattern == 1)
            {
                attackPattern = 2;
                StopAllCoroutines();
                StartCoroutine(Attack2Coroutine());
            }
        }
    }

    IEnumerator Attack0Coroutine()
    {
        Debug.Log("Attack0Coroutine");
        while (true)
        {
            float r = Random.value;
            if (r < 0.5f)
            {
                yield return StartCoroutine(ShotAttack());
            }
            else
            {
                animator.SetTrigger("attackBite");
            }
            
            yield return new WaitForSeconds(attackInterval);
        }
    }

    IEnumerator Attack1Coroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackInterval);
            if (Random.value < 0.2f)
            {
                // 弾攻撃
                yield return StartCoroutine(ShotAttack());
            }
            else
            {
                // 地中隠れ攻撃
                yield return StartCoroutine(HideUnderground());
            }
        }
    }

    IEnumerator ShotAttack()
    {
        animator.SetTrigger("attackSpit");
        yield return new WaitForSeconds(0.5f);
        nWayShooter.Fire();
    }

    IEnumerator HideUnderground()
    {
        // 元の位置を保持
        Vector3 originPos = transform.position;

        canRoutate = false; //回転を止める
        // 1) 隠れるアニメーション
        animator.SetTrigger("hide");
        yield return new WaitForSeconds(3f);
        canDamage = false;  //無敵にする
        yield return new WaitForSeconds(2f);
        // 2) 地面下に移動
        transform.position = new Vector3(originPos.x, -20f, originPos.z);
        canRoutate = true;  //回転許可

        // 3) ランダム Z 移動
        float startZ = transform.position.z;
        float endZ = Random.Range(-8f, 8f);
        float moveDuration = 1f;
        float elapsed = 0f;
        Vector3 pos = transform.position;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            pos.z = Mathf.Lerp(startZ, endZ, t);
            transform.position = pos;
            yield return null;
        }

        // 4) 元の Y に戻す
        pos = transform.position;
        pos.y = originPos.y;
        transform.position = pos;

        canRoutate = false;//回転を止める
        // 5) 噛みつき＆再出現
        animator.SetTrigger("underBite");
        yield return new WaitForSeconds(2f);
        animator.SetTrigger("appear");
        canDamage = true;  //無敵終了
        yield return new WaitForSeconds(2f);
        canRoutate = true;//回転許可

    }


    IEnumerator Attack2Coroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(attackInterval);
        }
    }



    public override void Die(Transform hitPoint)
    {
        base.Die(hitPoint);
        animator.SetTrigger("Die");
        // 砂虫特有の爆発エフェクトなど
    }
}
