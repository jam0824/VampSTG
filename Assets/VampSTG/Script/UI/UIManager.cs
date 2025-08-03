using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    // Singleton instance
    public static UIManager Instance { get; private set; }

    [Header("アイコンのプレハブ（SpriteをアタッチしたGameObject）")]
    [SerializeField] private GameObject hpIconPrefab;

    [Header("並べ始める座標")]
    [SerializeField] private float startY = -4f;
    [SerializeField] private float startZ = -8f;

    [Header("アイコン間の間隔")]
    [SerializeField] private float spacing = 0.5f;

    [Tooltip("現在のHP数。外部から SetHp() で変更してください")]
    [SerializeField] private int currentHp = 3;

    // 内部管理用
    private int previousHp = -1;
    private readonly List<GameObject> icons = new List<GameObject>();

    private void Awake()
    {
        // Singletonの初期化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 最初に必ず描画しておく
        UpdateIcons();
    }

    private void Update()
    {
        // HP が変化したらのみ再描画
        if (currentHp != previousHp)
            UpdateIcons();
    }

    /// <summary>
    /// HP数に合わせてアイコンを増減表示します。
    /// </summary>
    private void UpdateIcons()
    {
        // アイコンを増やす
        if (currentHp > icons.Count)
        {
            for (int i = icons.Count; i < currentHp; i++)
            {
                GameObject icon = Instantiate(hpIconPrefab, transform);
                icon.transform.localPosition = new Vector3(
                    0f,               // X 軸は固定
                    startY,               // Y 軸は固定
                    startZ + spacing * i
                );
                icons.Add(icon);
            }
        }
        // アイコンを減らす
        else if (currentHp < icons.Count)
        {
            for (int i = icons.Count - 1; i >= currentHp; i--)
            {
                Destroy(icons[i]);
                icons.RemoveAt(i);
            }
        }

        // 状態更新
        previousHp = currentHp;
    }

    /// <summary>
    /// HPを外部から変更するときはこちらを呼び出す
    /// </summary>
    public void SetHp(int newHp)
    {
        currentHp = Mathf.Max(0, newHp);
    }
}
