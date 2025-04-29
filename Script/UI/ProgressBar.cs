using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    [Header("Progress Bar")]
    [SerializeField] GameObject bar;
    [SerializeField] float barAllLength = 3f;
    [Header("Player Icon")]
    [SerializeField] GameObject playerIcon;
    [SerializeField] float wayAllLength = 12f;
    [SerializeField] float defaultIconZ = -6f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        elapsedTime += Time.deltaTime;
        float per = elapsedTime / allTime;
        DrawProgressBar(per);
        DrawPlayerIcon(per);
        */
        
    }

    public void DrawProgressBar(float perProgress){
        // プログレスバーの長さを変更
        float perLeft = 1.0f - perProgress;
        Vector3 scale = bar.transform.localScale;
        scale.x = this.barAllLength * perLeft;
        bar.transform.localScale = scale;

        //プログレスバーの描画位置を調整
        float offset = this.barAllLength * perProgress;
        Vector3 pos = bar.transform.position;
        pos.z = offset * 2;
        bar.transform.position = pos;
    }
    public void DrawPlayerIcon(float perProgress){
        float offset = perProgress * wayAllLength;
        Vector3 pos = playerIcon.transform.position;
        pos.z = defaultIconZ + offset;
        playerIcon.transform.position = pos;
    }
}
