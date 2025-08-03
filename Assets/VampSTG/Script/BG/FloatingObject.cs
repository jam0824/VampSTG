using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("浮遊設定")]
    [SerializeField] private float floatStrength = 10f; // 浮遊の強さ
    [SerializeField] private float floatSpeed = 10f; // 浮遊の速度
    [SerializeField] private float rotationSpeed = 360f; // 回転速度
    
    [Header("ランダム要素")]
    [SerializeField] private bool useRandomFloat = true; // ランダムな浮遊
    [SerializeField] private float randomRange = 1f; // ランダムの範囲
    
    [Header("微細な振動")]
    [SerializeField] private bool enableMicroMovement = true; // 微細な動き
    [SerializeField] private float microStrength = 0.1f; // 微細な動きの強さ
    [SerializeField] private float microSpeed = 3f; // 微細な動きの速度

    [Header("Scroll Speed")]
    [SerializeField] private float scrollSpeed = 0.6f;
    
    private Rigidbody rb;
    private Vector3 originalPosition;
    private Vector3 randomOffset;
    private float timeOffset;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning($"{gameObject.name}にRigidbodyがアタッチされていません");
            return;
        }
        
        // Rigidbodyの設定
        rb.useGravity = false; // 重力を無効化
        rb.linearDamping = 2f; // 線形減衰を設定して自然な動きに
        rb.angularDamping = 5f; // 角度減衰（回転の抵抗）
        
        // 制約なし（自由に動ける）
        rb.constraints = RigidbodyConstraints.None;
        
        // 初期位置を記録
        originalPosition = transform.position;
        
        // ランダムなオフセットを生成
        if (useRandomFloat)
        {
            randomOffset = new Vector3(
                Random.Range(-randomRange, randomRange),
                Random.Range(-randomRange, randomRange),
                Random.Range(-randomRange, randomRange)
            );
        }
        
        // 時間のオフセットをランダムに設定（同期を避けるため）
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }
    void GetScrollSpeed(){
        scrollSpeed = GameObject.Find("Stage3BG").GetComponent<InnerBgManager>().scrollSpeed;
    }
    void Update()
    {
        transform.Translate(Vector3.back * scrollSpeed * Time.deltaTime);
    }
    
    void FixedUpdate()
    {
        if (rb == null) return;
        
        float time = Time.time + timeOffset;
        
        // 基本的な浮遊運動（サイン波）
        Vector3 floatForce = Vector3.zero;
        
        // X軸の浮遊（左右運動）
        floatForce.x = Mathf.Cos(time * floatSpeed * 0.6f) * floatStrength * 0.5f;
        
        // Y軸の浮遊（上下運動）
        floatForce.y = Mathf.Sin(time * floatSpeed) * floatStrength;
        
        // Z軸の微細な動き
        if (enableMicroMovement)
        {
            floatForce.z = Mathf.Cos(time * microSpeed * 0.8f) * microStrength;
        }
        
        // ランダムオフセットを適用
        if (useRandomFloat)
        {
            floatForce += randomOffset * Mathf.Sin(time * floatSpeed * 0.7f) * 0.5f;
        }
        
        // 力を適用
        rb.AddForce(floatForce, ForceMode.Force);
        
        // ゆっくりとした回転を追加
        Vector3 torque = new Vector3(
            Mathf.Sin(time * 0.4f) * rotationSpeed * 0.3f,
            Mathf.Cos(time * 0.3f) * rotationSpeed,
            Mathf.Sin(time * 0.7f) * rotationSpeed
        );
        
        rb.AddTorque(torque * Time.fixedDeltaTime, ForceMode.Force);
        
        // 元の位置から離れすぎないように制限
        
        Vector3 currentPos = transform.position;
        Vector3 distanceFromOriginal = currentPos - originalPosition;
        
        if (distanceFromOriginal.magnitude > floatStrength * 2f)
        {
            // 元の位置に戻す力を適用
            Vector3 returnForce = -distanceFromOriginal.normalized * floatStrength * 0.5f;
            rb.AddForce(returnForce, ForceMode.Force);
        }
        
    }
    
    /// <summary>
    /// 浮遊の強さを動的に変更
    /// </summary>
    public void SetFloatStrength(float newStrength)
    {
        floatStrength = newStrength;
    }
    
    /// <summary>
    /// 浮遊の速度を動的に変更
    /// </summary>
    public void SetFloatSpeed(float newSpeed)
    {
        floatSpeed = newSpeed;
    }
    
    /// <summary>
    /// 回転速度を動的に変更
    /// </summary>
    public void SetRotationSpeed(float newSpeed)
    {
        rotationSpeed = newSpeed;
    }
    
    /// <summary>
    /// 浮遊効果を一時停止/再開
    /// </summary>
    public void SetFloatingEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled && rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
} 