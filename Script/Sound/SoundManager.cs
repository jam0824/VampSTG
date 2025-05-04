using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    // シングルトン化
    public static SoundManager Instance { get; private set; }

    [Header("AudioSource プールの初期サイズ")]
    [SerializeField] private int initialPoolSize = 10;
    [Header("AudioSource の親にする Transform (任意)")]
    [SerializeField] private Transform audioRoot;

    // SE 用プール
    private List<AudioSource> sePool = new List<AudioSource>();

    // BGM 用 AudioSource
    private AudioSource bgmSource;

    private void Awake()
    {
        // シングルトン設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
            InitializeBGMSource();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 初期プール生成
    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            sePool.Add(CreateNewSource("SE_Source", loop: false));
        }
    }

    // BGM 用 AudioSource を生成
    private void InitializeBGMSource()
    {
        bgmSource = CreateNewSource("BGM_Source", loop: true);
        bgmSource.playOnAwake = false;
        bgmSource.volume = 1f;
    }

    // 新規 AudioSource を作って返す
    private AudioSource CreateNewSource(string name, bool loop)
    {
        var go = new GameObject(name);
        // audioRoot が null のときは必ずこの.transform（＝SoundManager本体）を親に
        go.transform.SetParent(audioRoot != null ? audioRoot : this.transform);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = loop;
        return src;
    }

    /// <summary>
    /// SE を再生。既存の空いている AudioSource がなければ新規作成する。
    /// </summary>
    /// <param name="clip">再生する AudioClip</param>
    /// <param name="volume">音量 (0〜1)</param>
    public void PlaySE(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        // 再生中でないソースを探す
        AudioSource src = sePool.Find(s => !s.isPlaying);
        if (src == null)
        {
            // 全部使用中 → 新規作成してプールに追加
            src = CreateNewSource("SE_Source", loop: false);
            sePool.Add(src);
        }

        src.clip = clip;
        src.volume = volume * GameManager.Instance.globalSeVol;
        src.Play();
    }

    /// <summary>
    /// BGM を再生（ループ再生）。
    /// </summary>
    /// <param name="clip">再生する AudioClip</param>
    /// <param name="volume">音量 (0〜1)</param>
    public void PlayBGM(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            // 同じBGMが既に再生中なら何もしない
            return;
        }
        bgmSource.clip = clip;
        bgmSource.volume = volume * GameManager.Instance.globalBgmVol;
        bgmSource.Play();
        
    }

    /// <summary>
    /// BGM を停止する。
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// BGM のフェードイン再生（コルーチンで制御）。
    /// </summary>
    public void PlayBGMWithFadeIn(AudioClip clip, float targetVolume, float duration)
    {
        StopAllCoroutines();
        targetVolume *= GameManager.Instance.globalBgmVol;
        StartCoroutine(FadeInCoroutine(clip, targetVolume, duration));
    }

    private IEnumerator<YieldInstruction> FadeInCoroutine(AudioClip clip, float targetVolume, float duration)
    {
        bgmSource.clip = clip;
        bgmSource.volume = 0f;
        bgmSource.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }
        bgmSource.volume = targetVolume;
    }

    /// <summary>
    /// BGM のフェードアウト後停止。
    /// </summary>
    public void StopBGMWithFadeOut(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutCoroutine(duration));
    }

    private IEnumerator<YieldInstruction> FadeOutCoroutine(float duration)
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.volume = startVolume;
    }
}
