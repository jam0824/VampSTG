using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    [Tooltip("回転にかける時間（秒）")]
    public float turnDuration = 0.2f;

    private Animator animator;
    private bool facingRight = true;

    private bool canMove = true;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if(!canMove) return;

        float zInput = Input.GetAxis("Horizontal");
        float yInput = Input.GetAxis("Vertical");
        bool isShooting = Input.GetKey(KeyCode.Z) || Input.GetButton("Fire1");

        // “前進”／“後退”を向いている方向基準で判定
        bool movingForward  = (facingRight && zInput >= 0f)  || (!facingRight && zInput < 0f);
        bool movingBackward = (facingRight && zInput < 0f)  || (!facingRight && zInput > 0f);

        // アニメーション
        if (movingForward)
        {
            animator.SetTrigger("Front");
        }
        if (isShooting && movingBackward)
        {
            animator.SetTrigger("Back");
        }

        // 向き転換（撃っていないときのみ）
        if (!isShooting)
        {
            if (zInput > 0f && !facingRight)
                StartCoroutine(SmoothTurn(0f));
            else if (zInput < 0f && facingRight)
                StartCoroutine(SmoothTurn(180f));
        }

        // 移動
        Vector3 delta = new Vector3(0f, yInput, zInput) * speed * Time.deltaTime;
        transform.Translate(delta, Space.World);
    }

    private IEnumerator SmoothTurn(float targetYAngle)
    {
        facingRight = (targetYAngle == 0f);

        Quaternion startRot = transform.rotation;
        Quaternion endRot   = Quaternion.Euler(0f, targetYAngle, 0f);
        float elapsed = 0f;

        while (elapsed < turnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / turnDuration);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        transform.rotation = endRot;
    }

    public void SetCanMove(bool isOkMove){
        canMove = isOkMove;
    }
}
