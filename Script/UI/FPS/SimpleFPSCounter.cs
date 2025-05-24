using UnityEngine;

public class SimpleFPSCounter : MonoBehaviour
{
    [Header("FPS Display Settings")]    
    [SerializeField] private bool showFPS = true;    
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;    
    [SerializeField] private Vector2 position = new Vector2(10, 10);    
    [SerializeField] private int fontSize = 20;    
    [SerializeField] private Color goodFPSColor = Color.white;    
    [SerializeField] private Color mediumFPSColor = Color.yellow;    
    [SerializeField] private Color badFPSColor = Color.red;    
    [SerializeField] private Color backgroundColor = Color.black;    
    [SerializeField] private float updateInterval = 0.5f;    
    [SerializeField] private float mediumFPSThreshold = 90f;    
    [SerializeField] private float badFPSThreshold = 60f;
    
    private float fps;
    private float deltaTime = 0.0f;
    private float timer;
    private GUIStyle textStyle;
    private GUIStyle backgroundStyle;
    private Rect backgroundRect;
    private string fpsText = "";
    
    void Start()
    {
        // GUIスタイルを初期化
        InitializeGUIStyles();
    }
    
    void Update()
    {
        // キー入力でFPS表示を切り替え
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFPSDisplay();
        }
        
        if (!showFPS) return;
        
        // デルタタイムを計算
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        timer += Time.unscaledDeltaTime;
        
        // 指定された間隔でFPSを更新
        if (timer >= updateInterval)
        {
            fps = 1.0f / deltaTime;
            fpsText = $"FPS: {Mathf.Ceil(fps)}";
            UpdateTextColor();
            timer = 0f;
        }
    }
    
    void OnGUI()
    {
        if (!showFPS || string.IsNullOrEmpty(fpsText)) return;
        
        // 背景を描画
        GUI.Box(backgroundRect, "", backgroundStyle);
        
        // FPSテキストを描画
        GUI.Label(new Rect(position.x + 5, position.y + 2, 200, 30), fpsText, textStyle);
    }
    
    void InitializeGUIStyles()
    {
        // テキストスタイル
        textStyle = new GUIStyle();
        textStyle.fontSize = fontSize;
        textStyle.normal.textColor = goodFPSColor;
        textStyle.fontStyle = FontStyle.Bold;
        
        // 背景スタイル
        backgroundStyle = new GUIStyle();
        Texture2D backgroundTexture = new Texture2D(1, 1);
        backgroundTexture.SetPixel(0, 0, new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0.7f));
        backgroundTexture.Apply();
        backgroundStyle.normal.background = backgroundTexture;
        
        // 背景の矩形を設定
        backgroundRect = new Rect(position.x, position.y, 100, 25);
    }
    
    void UpdateTextColor()
    {
        if (textStyle == null) return;
        
        // FPS値に応じて色を変更
        if (fps >= mediumFPSThreshold)
        {
            textStyle.normal.textColor = goodFPSColor;
        }
        else if (fps >= badFPSThreshold)
        {
            textStyle.normal.textColor = mediumFPSColor;
        }
        else
        {
            textStyle.normal.textColor = badFPSColor;
        }
    }
    
    // 外部からFPS表示のON/OFFを切り替える
    public void ToggleFPSDisplay()
    {
        showFPS = !showFPS;
    }
    
    // FPS表示の有効/無効を設定
    public void SetFPSDisplay(bool enable)
    {
        showFPS = enable;
    }
} 