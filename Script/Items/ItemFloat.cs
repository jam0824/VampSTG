using UnityEngine;
using System.Collections;

public class ItemFloat : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("上下あわせた移動にかける総時間（秒）")]
    public float totalDuration = 3f;
    [Tooltip("頂点（最高点）の高さ（ワールド単位）")]
    public float peakHeight = 1f;

    private Vector3 _startPos;
    private float _elapsed;

    void Start()
    {
        _startPos = transform.position;
    }

    void Update()
    {
        _elapsed += Time.deltaTime;
        float t = _elapsed / totalDuration;

        /*
        if (t > 1f)
        {
            // 終了後は元位置にピタっと止める
            transform.position = _startPos;
            enabled = false;
            return;
        }
        */

        // 放物線（頂点 at t=0.5）: y = -4*(t-0.5)^2 + 1
        float parabola = -4f * (t - 0.5f) * (t - 0.5f) + 1f;
        float yOffset = parabola * peakHeight;

        transform.position = _startPos + Vector3.up * yOffset;
    }
}
