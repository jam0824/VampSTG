using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour
{
    [SerializeField] int hp = 500;
    int maxHp = 500;

    [Tooltip("Animator コンポーネント（Inspector でセット、未設定なら同じオブジェクトを自動取得）")]
    public Animator animator;
    
    [Tooltip("攻撃トリガー名（交互に発火させる）")]
    public string[] triggerNames;
    
    [Tooltip("何秒おきに攻撃アニメを発動するか")]
    public float attackInterval = 2f;
    [Header("UI Settings")]
    [SerializeField] BossHpBar bossHpBar;

    public ScatterShooter scatterShooter;

    private int nextIndex = 0;
    private bool isDead = false;

    private string tagName = "Enemy";

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        this.maxHp = hp;
        SetTagName(tagName);
        
        StartCoroutine(AttackLoop());
    }

    void Update()
    {
        DrawHpBar();
    }

    void DrawHpBar(){
        float per = ((float)maxHp - (float)hp)/ (float)maxHp;
        bossHpBar.DrawHpBar(per);
    }

    void SetTagName(string tagName){
        foreach (Transform t in GetComponentsInChildren<Transform>(includeInactive: true))
        {
            t.gameObject.tag = tagName;
        }
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            // 現在のインデックスのトリガーを発火
            animator.SetTrigger(triggerNames[nextIndex]);

            // 次はインデックスを +1（配列長でループ）
            nextIndex = (nextIndex + 1) % triggerNames.Length;

            yield return new WaitForSeconds(1.4f);
            scatterShooter.FireScatter();

            // 指定秒数待機
            yield return new WaitForSeconds(attackInterval);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerBullet")) return;
        if (!other.TryGetComponent<ConfigPlayerBullet>(out var bullet)) return;
        if (isDead) return;

        hp = hit(bullet, hp);
        if(hp <= 0) enemyDie();
        
    }

    int hit(ConfigPlayerBullet bullet, int enemyHp){
        enemyHp -= bullet.getDamage();
        AudioClip hitSe = bullet.hitSe;
        if(hitSe != null) SoundManager.Instance.PlaySE(bullet.hitSe, bullet.hitSeVolume);
        return enemyHp;
    }

    void enemyDie(){
        isDead = true;
        Vector3 pos = gameObject.transform.position;
        EffectController.Instance.PlaySmallExplosion(pos, gameObject.transform.rotation);
        Destroy(gameObject);
    }

}
