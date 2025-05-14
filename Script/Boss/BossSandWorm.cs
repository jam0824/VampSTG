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

    IEnumerator Attack0Coroutine(){
        Debug.Log("Attack0Coroutine");
        while(true){
            float r = Random.value;
            if(r < 0.5f){
                animator.SetTrigger("attackBite");
            }
            else{
                animator.SetTrigger("attackSpit");
            }
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
