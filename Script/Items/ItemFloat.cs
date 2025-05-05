using UnityEngine;
using System.Collections;

public class ItemFloat : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("上下あわせた移動にかける総時間（秒）")]
    public float totalDuration = 10f;
    [Tooltip("頂点（最高点）の高さ（ワールド単位）")]
    public float peakHeight = 2f;
    [Header("PlayerCoreに寄ってくる時のパラメーター")]
    public float moveSpeed = 5f;
    public float stopDistance = 0.1f;

    private Vector3 _startPos;
    private float _elapsed;

    private bool isInCollectionArea = false;

    private Transform playerTransform;

    void Start()
    {
        _startPos = transform.position;
        if(GameManager.Instance.playerCore != null) 
            playerTransform = GameManager.Instance.playerCore.transform;
    }

    void Update()
    {
        _elapsed += Time.deltaTime;

        if (!isInCollectionArea)
        {
            float t = _elapsed / totalDuration;
            // 放物線（頂点 at t=0.5）: y = -4*(t-0.5)^2 + 1
            float parabola = -4f * (t - 0.5f) * (t - 0.5f) + 1f;
            float yOffset = parabola * peakHeight;

            transform.position = _startPos + Vector3.up * yOffset;
        }
        else
        {
            if(playerTransform == null)
                playerTransform = GameManager.Instance.playerCore.transform;
            // ─── 移動 ───
            Vector3 toPlayer = playerTransform.position - transform.position;
            toPlayer.x = 0f;
            float dist = toPlayer.magnitude;
            if (dist > stopDistance)
            {
                float step = moveSpeed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, step);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("ItemCollectionArea")){
            if(isInCollectionArea == false) isInCollectionArea = true;
        }
    }
}
