using UnityEngine;
using System.Collections;

public class BossSandWorm : BaseBoss
{
    [Header("レーザー発射")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject laserPrefab;

    [Header("BGM設定")]
    [SerializeField] private AudioClip bgm;
    [SerializeField] private float bgmVol = 0.8f;

    [Header("向き調整")]
    [SerializeField] private float rotateSpeed = 5f;   // 回転の滑らかさ

    NWayShooter nWayShooter;
    ScatterShooter scatterShooter;

    int attackPattern = -1;
    bool canRoutate = true;
    GameObject laser;

    protected override AudioClip GetEntryBGM() => bgm;
    protected override float GetEntryBGMVolume() => bgmVol;

    protected override void Start()
    {
        base.Start();
        nWayShooter = GetComponent<NWayShooter>();
        scatterShooter = GetComponent<ScatterShooter>();

    }

    protected override void Update()
    {
        base.Update();
        UpdateFacing();
        SwitchAttackPattern();
        if ((isDead) && (laser != null))
        {
            laser.GetComponent<Vamp_Hovl_Laser2>().DisablePrepare();
        }
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
                ResetAllFlag();
                animator.SetTrigger("idle");
                StopAllCoroutines();
                StartCoroutine(Attack2Coroutine());
            }
        }
    }

    void ResetAllFlag()
    {
        canDamage = true;
        canRoutate = true;
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
                yield return StartCoroutine(ShotScatter());
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
                yield return StartCoroutine(HideUnderground(false));
            }
        }
    }

    IEnumerator Attack2Coroutine()
    {
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(HideUnderground(true));
        ResetAllFlag();
        while (true)
        {
            float r = Random.value;
            yield return StartCoroutine(ShotLaser());

            yield return new WaitForSeconds(attackInterval);
        }
    }

    IEnumerator ShotLaser()
    {
        animator.SetTrigger("attackRoar");
        yield return new WaitForSeconds(0.8f);
        laser = Instantiate(
            laserPrefab,
            firePoint.position,
            firePoint.rotation,
            firePoint
        );
        laser.transform.Rotate(0f, -90f, 0f, Space.Self);
        yield return new WaitForSeconds(4.5f);
        laser.GetComponent<Vamp_Hovl_Laser2>().DisablePrepare();
    }


    IEnumerator ShotAttack()
    {
        animator.SetTrigger("attackSpit");
        yield return new WaitForSeconds(0.8f);
        nWayShooter.Fire();
    }

    IEnumerator ShotScatter()
    {
        animator.SetTrigger("attackBite");
        yield return new WaitForSeconds(0.8f);
        scatterShooter.FireRandomScatter();
    }

    IEnumerator HideUnderground(bool isInitPos)
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
        float endZ = (!isInitPos) ? Random.Range(-8f, 8f) : 6f;
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
        yield return new WaitForSeconds(1f);
        scatterShooter.FireRandomScatter();
        yield return new WaitForSeconds(1f);
        animator.SetTrigger("appear");
        canDamage = true;  //無敵終了
        yield return new WaitForSeconds(2f);
        canRoutate = true;//回転許可

    }


}
