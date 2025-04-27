using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Enemy : MonoBehaviour
{
    [SerializeField] int hp = 10;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float rotateSpeed = 90f;   // 度/秒
    [SerializeField] float stopDistance = 0.5f;

    [SerializeField] GameObject explosion;
    [SerializeField] float offsetExplosionY = 0f;

    public GameObject item{get;set;} = null;

    Transform playerTransform;
    bool isTurning = false;

    void Start()
    {
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    void Update()
{
    if (playerTransform == null) return;

    // ─── 移動 ───
    Vector3 toPlayer = playerTransform.position - transform.position;
    //toPlayer.y = 0f;
    float dist = toPlayer.magnitude;
    if (dist > stopDistance)
    {
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, step);
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


    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerBullet")) return;
        if (!other.TryGetComponent<ConfigPlayerBullet>(out var bullet)) return;

        hp = hit(bullet, hp);
        if (hp <= 0) enemyDie();
    }

    int hit(ConfigPlayerBullet bullet, int enemyHp){
        enemyHp -= bullet.getDamage();
        AudioClip hitSe = bullet.hitSe;
        if(hitSe != null) SoundManager.Instance.PlaySE(bullet.hitSe, bullet.hitSeVolume);
        return enemyHp;
    }

    void enemyDie(){
        Vector3 pos = gameObject.transform.position;
        if(offsetExplosionY != 0) pos.y += offsetExplosionY;
        EffectController.Instance.PlaySmallExplosion(pos, gameObject.transform.rotation);
        ApearItem(item);
        Destroy(gameObject);
    }

    void ApearItem(GameObject objItem){
        if(objItem == null) return;
        Vector3 pos = gameObject.transform.position;
        Instantiate(objItem, pos, gameObject.transform.rotation);
        Debug.Log("アイテム出現");
    }
}
