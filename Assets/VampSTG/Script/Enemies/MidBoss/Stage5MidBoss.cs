using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage5MidBoss : BaseEnemy
{
    [Header("Appear Fade Settings")]
    [SerializeField] private bool fadeInOnSpawn = true;
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private bool switchToOpaqueAfterFade = true;
    [SerializeField] private GameObject m_bulletMlFirePoint;

    [Header("Movement Settings")]
    [SerializeField] private Vector3[] listMovePoints = new Vector3[3] 
    { 
        new Vector3(0f, 2f, 6f), 
        new Vector3(0f, 0f, 0f), 
        new Vector3(0f, 2f, 6f) 
    };
    [SerializeField] private float moveDuration = 2f;
    [SerializeField] private float stayDuration = 1.5f;
    [SerializeField] private bool loopMovement = true;

    [Header("Attack Settings")]
    [SerializeField] private GameObject m_attackPoint;

    private SpriteRenderer[] listSpriteRenderers;
    private Material[] listMaterials;
    private Coroutine fadeCoroutine;
    private BulletMlPlayer m_bulletMlPlayer;
    private Animator m_animator;
    private Rigidbody m_rigidbody;
    private Collider[] listColliders;
    
    // 移動制御用
    private Coroutine movementCoroutine;
    private int currentPointIndex = 0;
    private bool isMovementStarted = false;

    private void Awake()
    {
        m_bulletMlPlayer = m_bulletMlFirePoint.GetComponent<BulletMlPlayer>();
        listSpriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        var listMaterialTemp = new List<Material>();
        
        // 通常のRenderer（MeshRenderer等）を取得
        var listRenderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < listRenderers.Length; i++)
        {
            var materials = listRenderers[i].materials;
            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j] != null)
                {
                    listMaterialTemp.Add(materials[j]);
                }
            }
        }
        
        // SkinnedMeshRendererも明示的に取得（Rendererに含まれるが念のため）
        var listSkinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < listSkinnedMeshRenderers.Length; i++)
        {
            var materials = listSkinnedMeshRenderers[i].materials;
            for (int j = 0; j < materials.Length; j++)
            {
                if (materials[j] != null && !listMaterialTemp.Contains(materials[j]))
                {
                    listMaterialTemp.Add(materials[j]);
                }
            }
        }
        
        listMaterials = listMaterialTemp.ToArray();
    }

    private void OnEnable()
    {
        m_animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody>();
        listColliders = GetComponentsInChildren<Collider>();
        
        // Rigidbodyをkinematicにし、Colliderを無効にする（フェード中は当たり判定を停止）
        if (m_rigidbody != null)
        {
            m_rigidbody.isKinematic = true;
        }
        // 全てのColliderを無効にする
        listColliders = SetCollidersToEnable(listColliders, false);

        if (fadeInOnSpawn)
        {
            // フェード用にTransparentモードに設定
            SetMaterialsToTransparent();
            SetAlphaAll(0f);
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeInRoutine());
        }
        else
        {
            SetAlphaAll(1f);
        }
    }

    private void OnDisable()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
    }

    private IEnumerator FadeInRoutine()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeInDuration);
            SetAlphaAll(t);
            yield return null;
        }
        SetAlphaAll(1f);
        
        // フェード完了後にOpaqueモードに設定
        if (switchToOpaqueAfterFade)
        {
            SetMaterialsToOpaque();
        }
        
        // Rigidbodyを有効にし、Colliderを有効にする（不透明化完了後に当たり判定を開始）
        if (m_rigidbody != null)
        {
            m_rigidbody.isKinematic = false;
        }
        // 全てのColliderを有効にする
        listColliders = SetCollidersToEnable(listColliders, true);
        
        // BulletML開始
        m_bulletMlPlayer.StartBulletML();
        
        // 移動開始
        StartMovement();
        
        fadeCoroutine = null;
    }

    /// <summary>
    /// 全てのColliderを有効/無効にする
    /// </summary>
    /// <param name="_listColliders"></param>
    /// <param name="_enable"></param>
    /// <returns></returns>
    private Collider[] SetCollidersToEnable(Collider[] _listColliders, bool _enable)
    {
        for (int i = 0; i < _listColliders.Length; i++)
        {
            if (_listColliders[i] != null)
            {
                _listColliders[i].enabled = _enable;
            }
        }
        return _listColliders;
    }

    private void SetAlphaAll(float alpha)
    {
        if (listSpriteRenderers != null)
        {
            for (int i = 0; i < listSpriteRenderers.Length; i++)
            {
                var sr = listSpriteRenderers[i];
                if (sr == null) continue;
                var c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }

        if (listMaterials != null)
        {
            for (int i = 0; i < listMaterials.Length; i++)
            {
                var mat = listMaterials[i];
                if (mat == null) continue;

                if (mat.HasProperty("_BaseColor"))
                {
                    var c = mat.GetColor("_BaseColor");
                    c.a = alpha;
                    mat.SetColor("_BaseColor", c);
                }
                else if (mat.HasProperty("_Color"))
                {
                    var c = mat.GetColor("_Color");
                    c.a = alpha;
                    mat.SetColor("_Color", c);
                }
            }
        }
    }

    private void SetMaterialsToTransparent()
    {
        if (listMaterials == null) return;
        
        for (int i = 0; i < listMaterials.Length; i++)
        {
            var mat = listMaterials[i];
            if (mat == null || !mat.HasProperty("_Surface")) continue;
            
            // Transparentモード(1)に設定
            mat.SetFloat("_Surface", 1f);
            
            // レンダーキューも設定
            mat.renderQueue = 3000; // Transparent queue
            
            // アルファブレンディング用の設定
            if (mat.HasProperty("_SrcBlend"))
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend"))
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (mat.HasProperty("_ZWrite"))
                mat.SetFloat("_ZWrite", 0f);
        }
    }

    private void SetMaterialsToOpaque()
    {
        if (listMaterials == null) return;
        
        for (int i = 0; i < listMaterials.Length; i++)
        {
            var mat = listMaterials[i];
            if (mat == null) continue;
            
            // URP マテリアルの場合
            if (mat.HasProperty("_Surface"))
            {
                // 明確にOpaqueモード(0)に設定
                mat.SetFloat("_Surface", 0f);
                
                // シェーダーキーワードの設定（URP対応）
                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.DisableKeyword("_ALPHATEST_ON");
                
                // アルファモード設定
                if (mat.HasProperty("_AlphaClip"))
                {
                    mat.SetFloat("_AlphaClip", 0f);
                }
            }
            else
            {
                // 汎用的な透明度無効化キーワード
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }
            
            // 共通のOpaqueモード設定
            mat.renderQueue = 2000; // Geometry queue
            
            // ブレンドモード設定（可能な場合）
            if (mat.HasProperty("_SrcBlend"))
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
            if (mat.HasProperty("_DstBlend"))
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
            if (mat.HasProperty("_ZWrite"))
                mat.SetFloat("_ZWrite", 1f);
            
            // 古いシェーダー用のモード設定
            if (mat.HasProperty("_Mode"))
                mat.SetFloat("_Mode", 0f); // Opaque mode for Standard shader
        }
    }

    // BaseEnemy の抽象メソッド実装
    protected override void HandleMovement()
    {
        // 移動はコルーチンで制御するため、ここでは何もしない
    }

    /// <summary>
    /// 3点移動を開始する
    /// </summary>
    private void StartMovement()
    {
        if (listMovePoints == null || listMovePoints.Length == 0)
        {
            Debug.LogWarning("[Stage5MidBoss] Move points not set");
            return;
        }

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        
        currentPointIndex = 0;
        isMovementStarted = true;
        movementCoroutine = StartCoroutine(MovementRoutine());
    }

    /// <summary>
    /// 3点移動のメインルーチン
    /// </summary>
    private IEnumerator MovementRoutine()
    {
        Vector3 startPosition = transform.position;
        
        while (isMovementStarted)
        {
            for (int i = 0; i < listMovePoints.Length; i++)
            {
                if (!isMovementStarted) break; // 移動が停止された場合は終了
                
                currentPointIndex = i;
                Vector3 targetPosition = listMovePoints[i];
                
                // イージング移動
                yield return StartCoroutine(MoveToPositionWithEasing(startPosition, targetPosition, moveDuration));
                
                if (!isMovementStarted) break; // 移動が停止された場合は終了
                
                // 到達地点で待機
                AttackMotion(m_animator);
                yield return new WaitForSeconds(stayDuration);
                
                // 次の移動の開始位置を更新
                startPosition = targetPosition;
            }
            
            // ループが無効な場合は終了
            if (!loopMovement)
            {
                break;
            }
        }
        
        // 移動完了
        isMovementStarted = false;
        movementCoroutine = null;
    }

    /// <summary>
    /// イージングを使用した移動
    /// </summary>
    private IEnumerator MoveToPositionWithEasing(Vector3 startPos, Vector3 targetPos, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Ease-In-Out Cubic イージング
            t = t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
            
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        
        // 最終位置を確実に設定
        transform.position = targetPos;
    }

    /// <summary>
    /// 現在移動中かどうかを取得
    /// </summary>
    public bool IsMoving()
    {
        return isMovementStarted && movementCoroutine != null;
    }

    /// <summary>
    /// 現在の移動ポイントのインデックスを取得
    /// </summary>
    public int GetCurrentPointIndex()
    {
        return currentPointIndex;
    }

    /// <summary>
    /// 移動を停止する
    /// </summary>
    public void StopMovement()
    {
        isMovementStarted = false;
        
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
    }

    /// <summary>
    /// ループ設定を変更する
    /// </summary>
    public void SetLoopMovement(bool loop)
    {
        loopMovement = loop;
    }

    /// <summary>
    /// 攻撃処理
    /// </summary>
    /// <param name="animator"></param>
    private void AttackMotion(Animator _animator)
    {
        float r = Random.Range(0f, 1f);
        if (r < 0.5f)
        {
            _animator.SetTrigger("attack");
        }
        else
        {
            _animator.SetTrigger("attack2");
        }
    }

    public void Shoot()
    {
        IEnemyShooter enemyShooter = m_attackPoint.GetComponent<IEnemyShooter>();
        if (enemyShooter != null)
        {
            enemyShooter.Fire();
        }
    }

    protected override void Explosion(float maxHp)
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 pos = transform.position;
            pos.x = 1f; // 少し画面の手前に出す
            // y は 0 ～ 6 の範囲
            pos.y += (Random.value - 0.5f) * 2f + offsetExplosionY;
            // z を ±1 の範囲でランダムにずらす
            pos.z += (Random.value - 0.5f) * 2f;

            // ランダム爆発
            float r = Random.value;
            if (r < 0.3)
            {
                EffectController.Instance.PlaySmallExplosion(pos, transform.rotation, false);
            }
            else if (r < 0.6)
            {
                EffectController.Instance.PlayMiddleExplosion(pos, transform.rotation, false);
            }
            else
            {
                EffectController.Instance.PlayLargeExplosion(pos, transform.rotation, false);
            }
        }
    }
}
