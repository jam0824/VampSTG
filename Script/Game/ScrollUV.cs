using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ScrollUV : MonoBehaviour
{
    [Tooltip("テクスチャをスクロールする速度 (X, Y)")]
    public Vector2 scrollSpeed = new Vector2(0.1f, 0f);

    private Renderer rend;
    private Vector2 offset;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        // フレームごとにオフセットを増加
        offset += scrollSpeed * Time.deltaTime;
        // マテリアルに反映
        rend.material.mainTextureOffset = offset;
    }
}
