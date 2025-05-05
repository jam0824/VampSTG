using System.Collections;
using UnityEngine;

public class BaseProjectile : MonoBehaviour
{
    [Header("基本設定")]
    [SerializeField, Tooltip("ターゲットのタグ")]
    protected string[] targetTags;
    [SerializeField, Min(0.01f), Tooltip("弾の生存時間")]
    float lifeTime = 3f;

    [Header("初速設定")]
    [SerializeField, Tooltip("初速度")]
    Vector3 initialVelocity = Vector3.zero;
    [SerializeField, Tooltip("初速度の振れ幅の最小値")]
    Vector3 minInitialVelocity = Vector3.zero;
    [SerializeField, Tooltip("初速度の振れ幅の最大値")]
    Vector3 maxInitialVelocity = Vector3.zero;

    protected bool isHit = false;
    protected Vector3 velocity;
    protected Vector3 position;
    protected Vector3 acceleration;
    Transform thisTransform;
    protected Transform target = null;

    float startX;

    GameObject player;

    void Start()
    {
        player = GameObject.FindWithTag("Core");
        thisTransform = transform;
        position = thisTransform.position;
        startX = position.x;
        GameObject targetObj = GetTargetObj(targetTags);
        if (targetObj != null) target = targetObj.transform;

        AddInitialVelocity();
        StartCoroutine(DestroyTimer(lifeTime));
    }

    protected GameObject GetTargetObj(string[] targetTags)
    {
        GameObject targetObj = null;
        // 配列を順番にループ
        foreach (var tag in targetTags)
        {
            // タグで検索（該当するオブジェクトがなければ null が返る）
            var obj = GameObject.FindGameObjectWithTag(tag);
            if (obj != null)
            {
                targetObj = obj;
                break;    // 見つかったらループを抜ける
            }
        }

        // 最後に見つからなかった場合の処理
        if (targetObj == null)
        {
            Debug.LogWarning("どのタグにも一致するオブジェクトが見つかりませんでした。");
        }
        return targetObj;
    }

    void Update()
    {
        acceleration = Vector3.zero;
        MoveMissile();
    }

    protected void AddInitialVelocity()
    {
        // Y・Z の基本速度を取得
        float vY = initialVelocity.y + Random.Range(minInitialVelocity.y, maxInitialVelocity.y);
        float vZ = initialVelocity.z + Random.Range(minInitialVelocity.z, maxInitialVelocity.z);

        // 親（キャラクター）の向きをチェックして Z を反転
        if (player != null)
        {
            // parent.forward.z > 0 → 親は「正Z（右向き）」→ 左（負Z）へ撃つ
            //              < 0 → 親は「負Z（左向き）」→ 右（正Z）へ撃つ
            float sign = player.transform.forward.z > 0f ? -1f : 1f;
            vZ = Mathf.Abs(vZ) * sign;
        }

        velocity = new Vector3(0f, vY, vZ);
    }

    protected virtual void AddForce() { }

    void MoveMissile()
    {
        AddForce();
        if (isHit)
        {
            //Destroy(gameObject);
            return;
        }

        velocity += acceleration * Time.deltaTime;
        position += velocity * Time.deltaTime;
        position.x = startX;
        thisTransform.position = position;
        thisTransform.rotation = Quaternion.LookRotation(velocity, Vector3.up);
    }

    IEnumerator DestroyTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Destroy(gameObject);
    }
}
