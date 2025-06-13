using UnityEngine;

public class ScrollMovingObj : MonoBehaviour
{
    private StageManager stageManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
    }

    // Update is called once per frame
    void Update()
    {
        MoveWithScroll();
    }

    /// <summary>
    /// BGのスクロールスピードに合わせてz軸方向に移動
    /// </summary>
    void MoveWithScroll()
    {
        // x軸は0のまま、z軸方向のみ移動
        Vector3 scrollMovement = new Vector3(0f, 0f, -stageManager.scrollSpeed * Time.deltaTime);
        transform.Translate(scrollMovement, Space.World);
    }
}
