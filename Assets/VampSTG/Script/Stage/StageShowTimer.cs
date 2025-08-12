using UnityEngine;

public class StageShowTimer : MonoBehaviour
{
    /// <summary>
    /// 特定の時間が来た時に登録されているオブジェクトをアクティブにする
    /// </summary>
    [Header("GameObject(初期状態は非表示)")]
    public GameObject[] gameObjects;

    [Header("表示を開始する時間(秒)")]
    public float time;

    private StageManager stageManager;
    private bool isShow = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isShow) return;
        if (stageManager.allElapsedTime >= time && !isShow)
        {
            foreach (var gameObject in gameObjects)
            {
                if(gameObject != null) gameObject.SetActive(true);
            }
            isShow = true;
        }
    }
}
