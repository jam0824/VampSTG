using UnityEngine;

public class BossHpBar : FadeBar
{
    [Header("HP Bar")]
    [SerializeField] GameObject bar;
    [SerializeField] float barAllLength = 3f;

    public override void DrawBar(float perProgress)
    {
        // HPバー長さと位置
        float perLeft = 1f - perProgress;
        var scale = bar.transform.localScale;
        scale.x = barAllLength * perLeft;
        bar.transform.localScale = scale;

        float offset = barAllLength * perProgress;
        var pos = bar.transform.position;
        pos.z = offset * 2;
        bar.transform.position = pos;
    }
}