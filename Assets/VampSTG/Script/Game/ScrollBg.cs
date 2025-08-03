using UnityEngine;

public class ScrollBg : MonoBehaviour
{
    [SerializeField] float scrollSpeed = 1f;

    void Update()
    {
        // フレーム時間を掛けて一定速度に
        float deltaZ = scrollSpeed * Time.deltaTime;
        
        // 直接 transform.position を更新
        transform.position -= new Vector3(0f, 0f, deltaZ);
    }
}
