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
        var playerObj = GameObject.Find("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    void Update()
    {
        if (playerTransform == null) return;

        // ─── 移動（省略） ───
        Vector3 toPlayer = playerTransform.position - transform.position;
        //toPlayer.y = 0;
        float dist = toPlayer.magnitude;
        if (dist > stopDistance)
        {
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, step);
        }

        // ─── 回転 ───
        Vector3 dir = toPlayer.normalized;
        dir.y = 0;

        // 1) プレイヤーが「完全に後ろ（180°）」に回ったら回転開始
        float dot = Vector3.Dot(transform.forward, dir);
        if (!isTurning && dot < 0f)
        {
            isTurning = true;
        }

        // 2) 回転中はずっとQuaternion.RotateTowardsでターゲット方向へ
        if (isTurning)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );

            // 3) 十分に向き直せたらフラグオフ
            float angleLeft = Quaternion.Angle(transform.rotation, targetRot);
            if (angleLeft < 0.5f)  // 0.5°未満になったら完了とみなす
            {
                isTurning = false;
            }
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
        ExplosionController.Instance.PlaySmallExplosion(pos, gameObject.transform.rotation);
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
