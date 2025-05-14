// BossSandWorm.cs
using UnityEngine;
using System.Collections;

public class BossSandWorm : BaseBoss
{
    [Header("BGM設定")]
    [SerializeField] private AudioClip bgm;
    [SerializeField] private float bgmVol = 0.8f;

    int attackPattern = -1;

    /// <summary>
    /// EntryCoroutine の BGM を上書き
    /// </summary>
    protected override AudioClip GetEntryBGM() => bgm;
    protected override float GetEntryBGMVolume() => bgmVol;

    protected override void Start()
    {
        base.Start();
        // 初期非アクティブ化など、固有初期化
        gameObject.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();
        SwitchAttackPattern();
    }

    void SwitchAttackPattern(){
        float hpPer = hp / maxHp;
        if(isStart){
            if(attackPattern == -1){
                attackPattern = 0;
                StopAllCoroutines();
                StartCoroutine(Attack0Coroutine());
            }
            else if((hpPer < 0.66f) && (attackPattern == 0)){
                attackPattern = 1;
                StopAllCoroutines();
                StartCoroutine(Attack1Coroutine());
            }
            else if((hpPer < 0.33f) && (attackPattern == 1)){
                attackPattern = 2;
                StopAllCoroutines();
                StartCoroutine(Attack2Coroutine());
            }
        }
    }

    protected override IEnumerator EntryCoroutine()
    {
        // 例：エントリー演出
        yield return new WaitForSeconds(5f);
        bossHpBar.StartFadeIn(3f);
        gameObject.SetActive(true);
        SoundManager.Instance.PlayBGM(GetEntryBGM(), GetEntryBGMVolume());
        yield return new WaitForSeconds(attackInterval);
        isStart = true;
    }

    IEnumerator Attack0Coroutine(){
        while(true){
            yield return new WaitForSeconds(attackInterval);
        }
    }

    IEnumerator Attack1Coroutine(){
        while(true){
            yield return new WaitForSeconds(attackInterval);
        }
    }

    IEnumerator Attack2Coroutine(){
        while(true){
            yield return new WaitForSeconds(attackInterval);
        }
    }


    /// <summary>
    /// 死亡時の固有演出があれば追加
    /// </summary>
    public override void Die(Transform hitPoint)
    {
        base.Die(hitPoint);
        animator.SetTrigger("Die");
        // 砂虫特有の爆発エフェクトなど
    }
}
