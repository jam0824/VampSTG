using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

// フェード色の種類
public enum FadeColor { Black, White }

public class FadeManager : MonoBehaviour
{
    [Header("Fade 設定")]
    [Tooltip("画面全体を覆う Image (Alpha 0 の黒 or 白)")]
    public Image fadeImage;
    [Tooltip("フェードにかける時間（秒）")]
    public float fadeDuration = 1f;
    [Tooltip("Inspector で選べるデフォルトのフェード色")]
    public FadeColor defaultFadeColor = FadeColor.Black;

    // シングルトン化
    private static FadeManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Inspector で設定したデフォルト色でシーン切り替え＋フェード実行
    /// </summary>
    public void FadeToSceneDefault(string sceneName)
    {
        StartCoroutine(FadeRoutine(sceneName, defaultFadeColor));
    }

    /// <summary>
    /// コードから黒／白を指定してシーン切り替え＋フェード実行
    /// </summary>
    /// <param name="sceneName">読み込み先シーン名</param>
    /// <param name="color">フェード色（Black or White）。省略時は Black。</param>
    public static void FadeToScene(string sceneName, FadeColor color = FadeColor.Black)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.FadeRoutine(sceneName, color));
        }
        else
        {
            Debug.LogError("SceneFader がシーン内に見つかりません。");
        }
    }

    // フェード処理のコルーチン
    private IEnumerator FadeRoutine(string sceneName, FadeColor fadeColor)
    {
        // ベースカラー（黒 or 白）
        Color baseCol = (fadeColor == FadeColor.Black) ? Color.black : Color.white;

        // ● フェードイン（透明→不透明）
        yield return StartCoroutine(Fade(0f, 1f, baseCol));

        // ● シーン読み込み（非同期）
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;

        // ● フェードアウト（不透明→透明）
        yield return StartCoroutine(Fade(1f, 0f, baseCol));
    }

    // 実際のアルファ値 Lerp を行うコルーチン
    private IEnumerator Fade(float startA, float endA, Color baseCol)
    {
        float elapsed = 0f;
        fadeImage.color = new Color(baseCol.r, baseCol.g, baseCol.b, startA);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(startA, endA, elapsed / fadeDuration);
            fadeImage.color = new Color(baseCol.r, baseCol.g, baseCol.b, a);
            yield return null;
        }

        fadeImage.color = new Color(baseCol.r, baseCol.g, baseCol.b, endA);
    }
}
