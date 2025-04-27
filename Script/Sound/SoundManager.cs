using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    // シングルトン化
    public static SoundManager Instance { get; private set; }

    [Header("AudioSource プールの初期サイズ")]
    [SerializeField] private int initialPoolSize = 10;
    [Header("AudioSource の親にする Transform (任意)")]
    [SerializeField] private Transform audioRoot;

    // プール
    private List<AudioSource> pool = new List<AudioSource>();

    private void Awake()
    {
        // シングルトン設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
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
            CreateNewSource();
        }
    }

    // 新規 AudioSource を作ってプールに加える
    private AudioSource CreateNewSource()
    {
        GameObject go = new GameObject("SE_Source");
        if (audioRoot != null) go.transform.SetParent(audioRoot);
        AudioSource src = go.AddComponent<AudioSource>();
        // 必要に応じてデフォルト設定（3D/2D、ループ、spatialBlend など）をここで行う
        src.playOnAwake = false;
        pool.Add(src);
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
        AudioSource src = pool.Find(s => !s.isPlaying);
        if (src == null)
        {
            // 全部使用中 → 新規作成
            src = CreateNewSource();
        }

        src.clip   = clip;
        src.volume = volume;
        src.Play();
    }
}
