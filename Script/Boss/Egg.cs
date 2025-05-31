using UnityEngine;
using System.Collections;

public class Egg : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.1f;
    [SerializeField] private float groundY = -4f;
    [SerializeField] private float hachingDelay = 3f; // 孵化までの待機時間
    public bool isGround = false;
    private bool isHatched = false; // 孵化済みフラグ
    private Animator animator;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGround) Move();
    }

    void Move()
    {
        Vector3 pos = transform.position;
        if (pos.y <= groundY)
        {
            OnGroundLanded();
            return;
        }
        else
        {
            pos.y -= moveSpeed * Time.deltaTime;
            transform.position = pos;
        }
    }
    
    /// <summary>
    /// 地面に着地した時の処理
    /// </summary>
    private void OnGroundLanded()
    {
        if (!isGround)
        {
            isGround = true;
            StartCoroutine(HachingCoroutine());
        }
    }
    
    /// <summary>
    /// 孵化処理のコルーチン
    /// </summary>
    private IEnumerator HachingCoroutine()
    {
        // 指定時間待機
        yield return new WaitForSeconds(hachingDelay);
        
        // 孵化処理
        if (!isHatched && animator != null)
        {
            isHatched = true;
            animator.SetTrigger("hatching");
            Debug.Log("卵が孵化しました！");
        }
        yield return new WaitForSeconds(hachingDelay);
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 地面との衝突判定（Trigger使用の場合）
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            OnGroundLanded();
            // 必要に応じて地面に着地した時のエフェクトや音を再生
            // EffectController.Instance.PlayExplosion(transform.position);
        }
    }
    
    /// <summary>
    /// 地面との衝突判定（Collision使用の場合）
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            OnGroundLanded();
            // 必要に応じて地面に着地した時のエフェクトや音を再生
            // EffectController.Instance.PlayExplosion(transform.position);
        }
    }
}
