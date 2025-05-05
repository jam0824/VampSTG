using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    [Tooltip("回転にかける時間（秒）")]
    public float turnDuration = 0.2f;
    
    [Header("CoreのPlayerModelセットのオフセット")]
    public Vector3 playerModelOffset;

    private Animator animator;
    private bool facingRight = true;

    private bool canMove = true;
    private GameObject playerModel;

    void Update()
    {
        // PlayerModelを取得できるまでループ
        if (playerModel == null)
        {
            playerModel = GameObject.FindGameObjectWithTag("PlayerModel");
        }
        if (playerModel != null && animator == null)
        {
            animator = playerModel.GetComponent<Animator>();
        }
        if (!canMove || playerModel == null) return;

        float zInput = Input.GetAxis("Horizontal");
        float yInput = Input.GetAxis("Vertical");
        bool isShooting = Input.GetKey(KeyCode.Z) || Input.GetButton("Fire1");

        // “前進”／“後退”を向いている方向基準で判定
        bool movingForward  = (facingRight && zInput > 0f)  || (!facingRight && zInput < 0f);
        bool movingBackward = (facingRight && zInput < 0f)  || (!facingRight && zInput > 0f);

        // 移動判定
        bool isMoving = Mathf.Abs(zInput) > 0f || Mathf.Abs(yInput) > 0f;

        // アニメーション
        if (isMoving)
        {
            if (movingForward)
            {
                animator.SetTrigger("Front");
            }
            else if (isShooting && movingBackward)
            {
                animator.SetTrigger("Back");
            }
            // 他の移動方向アニメーションが必要ならここに追加
        }
        else
        {
            // 移動していないときは Idle
            animator.SetTrigger("Idle");
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
        Vector3 pos = transform.position + playerModelOffset;
        playerModel.transform.position = pos;
    }

    private IEnumerator SmoothTurn(float targetYAngle)
    {
        facingRight = (targetYAngle == 0f);

        Quaternion startRot = playerModel.transform.rotation;
        Quaternion endRot   = Quaternion.Euler(0f, targetYAngle, 0f);
        float elapsed = 0f;

        while (elapsed < turnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / turnDuration);
            playerModel.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            transform.rotation = playerModel.transform.rotation;
            yield return null;
        }

        playerModel.transform.rotation = endRot;
        transform.rotation = endRot;
    }

    public void SetCanMove(bool isOkMove)
    {
        canMove = isOkMove;
    }
}
