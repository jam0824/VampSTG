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

    private SpriteRenderer[] listSpriteRenderers;
    private Material[] listMaterials;
    private Coroutine fadeCoroutine;
    private BulletMlPlayer m_bulletMlPlayer;

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
        m_bulletMlPlayer.StartBulletML();
        fadeCoroutine = null;
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
        // ステージ5中ボスの移動パターン未定のため、現状は静止。
        // 必要に応じて後で移動ロジックを追加してください。
    }
}
