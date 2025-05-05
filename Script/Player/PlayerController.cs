using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    [Tooltip("回転にかける時間（秒）")]
    public float turnDuration = 0.2f;

    [Header("CoreのPlayerModelセットのオフセット")]
    public Vector3 playerModelOffset;

    [Header("Idle時の浮遊設定")]
    [Tooltip("浮遊の振幅")]
    public float floatAmplitude = 0.05f;
    [Tooltip("浮遊の速さ（Hz）")]
    public float floatFrequency = 0.5f;

    private Animator animator;
    private bool facingRight = true;
    private bool canMove = true;
    private GameObject playerModel;

    // 現在のアニメーション状態を記録
    private string lastAnim = string.Empty;

    void Update()
    {
        // PlayerModelを取得できるまでループ
        if (playerModel == null)
            playerModel = GameObject.FindGameObjectWithTag("PlayerModel");
        if (playerModel != null && animator == null)
            animator = playerModel.GetComponent<Animator>();
        if (!canMove || playerModel == null) return;

        float zInput = Input.GetAxis("Horizontal");
        float yInput = Input.GetAxis("Vertical");
        bool isShooting = Input.GetKey(KeyCode.Z) || Input.GetButton("Fire1");

        // 向き基準の前進／後退判定
        bool movingForward  = (facingRight && zInput > 0f)  || (!facingRight && zInput < 0f);
        bool movingBackward = (facingRight && zInput < 0f)  || (!facingRight && zInput > 0f);

        // 移動判定
        bool isMoving = Mathf.Abs(zInput) > 0f || Mathf.Abs(yInput) > 0f;

        // 新しいアニメーション状態を決定
        string newAnim = string.Empty;
        if (isMoving)
        {
            if (movingForward)
                newAnim = "Front";
            else if (isShooting && movingBackward)
                newAnim = "Back";
            // 他の方向アニメーションがあればここで設定
        }
        else
        {
            newAnim = "Idle";
        }

        // 状態が変わったときだけトリガーを叩く
        if (!string.IsNullOrEmpty(newAnim) && newAnim != lastAnim)
        {
            animator.SetTrigger(newAnim);
            lastAnim = newAnim;
        }

        // 向き転換（撃っていないときのみ）
        if (!isShooting)
        {
            if (zInput > 0f && !facingRight)
                StartCoroutine(SmoothTurn(0f));
            else if (zInput < 0f && facingRight)
                StartCoroutine(SmoothTurn(180f));
        }

        // 実際の移動
        Vector3 delta = new Vector3(0f, yInput, zInput) * speed * Time.deltaTime;
        transform.Translate(delta, Space.World);

        // 移動範囲制限
        Vector3 clampedPos = transform.position;
        clampedPos.z = Mathf.Clamp(clampedPos.z, GameManager.Instance.minZ, GameManager.Instance.maxZ);
        clampedPos.y = Mathf.Clamp(clampedPos.y, GameManager.Instance.minY, GameManager.Instance.maxY);
        transform.position = clampedPos;

        // モデル配置＋停止時の浮遊
        Vector3 basePos = transform.position + playerModelOffset;
        if (!isMoving)
        {
            float yOffset = Mathf.Sin(Time.time * Mathf.PI * 2f * floatFrequency) * floatAmplitude;
            basePos.y += yOffset;
        }
        playerModel.transform.position = basePos;
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
