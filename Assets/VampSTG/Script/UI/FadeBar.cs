using UnityEngine;
using System.Collections;

/// <summary>
/// フェード処理を共通化した抽象ベースクラス。
/// StartFadeIn／StartFadeOut でコルーチンを呼び出し、
/// 子オブジェクトの SpriteRenderer アルファ値を操作します。
/// DrawBar メソッドは派生クラスで実装してください。
/// </summary>
public abstract class FadeBar : MonoBehaviour
{
    [Header("Fade Settings")]
    [Tooltip("フェードにかける時間（秒）")]
    [SerializeField] protected float fadeDuration = 1f;

    private Coroutine fadeCoroutine;

    /// <summary>透明→不透明にフェードイン</summary>
    public void StartFadeIn(float duration)
    {
        fadeDuration = duration;
        StartFade(true);
    }

    /// <summary>不透明→透明にフェードアウト</summary>
    public void StartFadeOut(float duration)
    {
        fadeDuration = duration;
        StartFade(false);
    }

    private void StartFade(bool fadeIn)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCoroutine(fadeIn));
    }

    private IEnumerator FadeCoroutine(bool fadeIn)
    {
        var sprites = GetComponentsInChildren<SpriteRenderer>();
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float alpha = fadeIn ? t : (1f - t);
            foreach (var sr in sprites)
            {
                var c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
            yield return null;
        }
        // 最終値
        float finalAlpha = fadeIn ? 1f : 0f;
        foreach (var sr in sprites)
        {
            var c = sr.color;
            c.a = finalAlpha;
            sr.color = c;
        }
    }

    /// <summary>
    /// バー固有の描画処理。派生クラスでオーバーライドしてください。
    /// </summary>
    public abstract void DrawBar(float perProgress);
}