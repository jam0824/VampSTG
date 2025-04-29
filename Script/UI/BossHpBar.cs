using UnityEngine;

public class BossHpBar : MonoBehaviour
{
    [Header("Hp Bar")]
    [SerializeField] GameObject bar;
    [SerializeField] float barAllLength = 3f;

    public void DrawHpBar(float perProgress){
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
}
