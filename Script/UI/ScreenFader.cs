using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private Image whiteImage;   // 白フェード用
    [SerializeField] private Image blackImage;   // 新規：黒フェード用
    [Header("撃滅設定")]
    [SerializeField] private Image gekimetsuImage;
    [SerializeField] private float gekimetsuVol = 1f;
    [SerializeField] private AudioClip gekimetsuSe;
    [SerializeField] private float fadeDuration = 1f;
    [Header("死設定")]
    [SerializeField] private Image deathImage;
    [SerializeField] private float deathFadeDuration = 1f;

    /// <summary>
    /// 白でフェードするメソッド
    /// </summary>
    public void FadeToWhite(string sceneName)
    {
        StartCoroutine(FadeWhiteCoroutine(1f, sceneName));
    }

    /// <summary>
    /// 黒でフェードするメソッド
    /// </summary>
    public void FadeToBlack(string sceneName)
    {
        StartCoroutine(FadeBlackCoroutine(1f, sceneName));
    }

    private IEnumerator FadeWhiteCoroutine(float targetAlpha, string sceneName)
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

        // 撃滅表示シーケンス
        yield return new WaitForSeconds(1f);
        SoundManager.Instance.PlaySE(gekimetsuSe, gekimetsuVol);
        gekimetsuImage.color = new Color(1f,1f,1f,1f);

        yield return new WaitForSeconds(3f);
        
        FadeManager.FadeToScene(sceneName, FadeColor.White);
    }

    private IEnumerator FadeBlackCoroutine(float targetAlpha, string sceneName)
    {
        float elapsed = 0f;
        Color c = blackImage.color;
        float startAlpha = c.a;

        while (elapsed < deathFadeDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / deathFadeDuration);
            blackImage.color = new Color(0f, 0f, 0f, a);
            yield return null;
        }
        blackImage.color = new Color(0f, 0f, 0f, targetAlpha);

        yield return new WaitForSeconds(1f);
        SoundManager.Instance.PlaySE(gekimetsuSe, gekimetsuVol);
        deathImage.color = new Color(1f,1f,1f,1f);

        yield return new WaitForSeconds(3f);

        FadeManager.FadeToScene(sceneName, FadeColor.Black);
    }
}
