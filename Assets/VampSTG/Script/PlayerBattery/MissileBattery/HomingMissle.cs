using UnityEngine;

public sealed class HomingMissle : BaseProjectile
{
    [SerializeField, Min(0.01f), Tooltip("ターゲットに到達するまでの時間")]
    float period = 1f;
    [SerializeField, Tooltip("加速度を制限するか？（Falseなら必中）")]
    bool limitAcceleration = true;
    [SerializeField, Min(0.01f), Tooltip("加速度の上限")]
    float maxAcceleration = 100f;

    protected override void AddForce()
    {
		if(target == null){
            GameObject targetGameObj = GetTargetObj(targetTags);
            if (targetGameObj != null) target = targetGameObj.transform;
		}
        if(target == null) return;

        // ターゲットとの差分を計算し、X成分は無視（0固定）
        Vector3 diff = target.position - position;
        diff.x = 0f;

        // 期中の速度補正も X 成分を無視
        Vector3 velYZ = new Vector3(0f, velocity.y, velocity.z);

        // Z–Y平面のみの加速度を計算
        Vector3 acc = (diff - velYZ * period) * 2f / (period * period);
        acc.x = 0f;  // 念のため X を0固定

        // 加速度制限（Y–Z成分のみで判定）
        if (limitAcceleration)
        {
            float sqrMagYZ = acc.y * acc.y + acc.z * acc.z;
            if (sqrMagYZ > maxAcceleration * maxAcceleration)
            {
                var norm = new Vector2(acc.y, acc.z).normalized * maxAcceleration;
                acc = new Vector3(0f, norm.x, norm.y);
            }
        }

        acceleration = acc;

        // 残り時間を減少
        period -= Time.deltaTime;
        if (period < 0f)
        {
            isHit = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        
        if((other.CompareTag("Enemy")) || (other.CompareTag("Boss"))){
            EffectController.Instance.PlayMiddleExplosion(transform.position, transform.rotation);
        }
        
    }
}
