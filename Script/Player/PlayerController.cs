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
    private bool flippedThisFrame;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        flippedThisFrame = false;

        float zInput = Input.GetAxis("Horizontal");
        float yInput = Input.GetAxis("Vertical");
        bool isShooting = Input.GetKey(KeyCode.Z) || Input.GetButton("Fire1");

        if (!isShooting)
        {
            if (zInput > 0f && !facingRight)
                StartCoroutine(SmoothTurn(0f));
            else if (zInput < 0f && facingRight)
                StartCoroutine(SmoothTurn(180f));
        }

        Vector3 delta = new Vector3(0f, yInput, zInput) * speed * Time.deltaTime;
        transform.Translate(delta, Space.World);

        if (!flippedThisFrame)
        {
            animator.SetFloat("Vertical", zInput);
            animator.SetFloat("Horizontal", yInput);
        }
    }

    private IEnumerator SmoothTurn(float targetYAngle)
    {
        facingRight = (targetYAngle == 0f);
        flippedThisFrame = true;

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
}
