using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private Image whiteImage;  // Inspectorで割り当て
    [SerializeField] private float fadeDuration = 1f;

    public void FadeToWhite(System.Action onComplete = null)
    {
        StartCoroutine(FadeCoroutine(1f, onComplete));
    }

    private IEnumerator FadeCoroutine(float targetAlpha, System.Action onComplete)
    {
        float elapsed = 0f;
        Color c = whiteImage.color;
        float startAlpha = c.a;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            whiteImage.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }

        whiteImage.color = new Color(1f, 1f, 1f, targetAlpha);

        onComplete?.Invoke();
    }
}
