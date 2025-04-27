using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vamp_HS_ProjectileMover : MonoBehaviour
{
    [SerializeField] protected float speed = 15f;
    [SerializeField] protected float hitOffset = 0f;
    [SerializeField] protected float destroyTime = 2f;
    [SerializeField] protected bool UseFirePointRotation;
    [SerializeField] protected Vector3 rotationOffset = Vector3.zero;
    [SerializeField] protected GameObject hit;
    [SerializeField] protected ParticleSystem hitPS;
    [SerializeField] protected GameObject flash;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Collider col;
    [SerializeField] protected Light lightSource;
    [SerializeField] protected GameObject[] Detached;
    [SerializeField] protected ParticleSystem projectilePS;
    private bool startChecker = false;
    [SerializeField] protected bool notDestroy = false;

    protected virtual void Start()
    {
        // トリガー判定に切り替え
        if (col != null)
            col.isTrigger = true;

        if (!startChecker)
        {
            if (flash != null)
            {
                flash.transform.parent = null;
            }
        }

        if (notDestroy)
            StartCoroutine(DisableTimer(destroyTime));
        else
            Destroy(gameObject, 5f);

        startChecker = true;
    }

    protected virtual IEnumerator DisableTimer(float time)
    {
        yield return new WaitForSeconds(time);
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    protected virtual void OnEnable()
    {
        if (startChecker)
        {
            if (flash != null)
                flash.transform.parent = null;
            if (lightSource != null)
                lightSource.enabled = true;
            col.enabled = true;
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (speed != 0f)
        {
            rb.linearVelocity = transform.forward * speed;
        }
    }

    // トリガー判定に変更
    protected virtual void OnTriggerEnter(Collider other)
    {
        // 速度＆回転をロック
        rb.constraints = RigidbodyConstraints.FreezeAll;

        if (lightSource != null)
            lightSource.enabled = false;

        col.enabled = false;

        if (projectilePS != null)
        {
            projectilePS.Stop();
            projectilePS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        // エフェクトをプロジェクトタイルの前方に生成
        Vector3 pos = transform.position + transform.forward * hitOffset;
        Quaternion rot = Quaternion.identity;

        if (hit != null)
        {
            // 回転
            if (UseFirePointRotation)
            {
                rot = transform.rotation * Quaternion.Euler(0f, 180f, 0f);
            }
            else if (rotationOffset != Vector3.zero)
            {
                rot = Quaternion.Euler(rotationOffset);
            }
            else
            {
                rot = Quaternion.LookRotation(transform.forward);
            }

            hit.transform.position = pos;
            hit.transform.rotation = rot;
            hitPS.Play();
        }

        // トレイルや付随 PS の停止
        foreach (var detachedPrefab in Detached)
        {
            if (detachedPrefab != null)
            {
                var detachedPS = detachedPrefab.GetComponent<ParticleSystem>();
                if (detachedPS != null)
                    detachedPS.Stop();
            }
        }

        // 自身の破棄 or 非アクティブ化
        float delay = (hitPS != null) ? hitPS.main.duration : 1f;
        if (notDestroy)
            StartCoroutine(DisableTimer(delay));
        else
            Destroy(gameObject, delay);
    }
}
