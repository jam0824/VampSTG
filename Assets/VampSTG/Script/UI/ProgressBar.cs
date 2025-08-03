using UnityEngine;

public class ProgressBar : FadeBar
{
    [Header("Progress Bar")]
    [SerializeField] GameObject bar;
    [SerializeField] float barAllLength = 3f;

    [Header("Player Icon")]
    [SerializeField] GameObject playerIcon;
    [SerializeField] float wayAllLength = 12f;
    [SerializeField] float defaultIconZ = -6f;

    public override void DrawBar(float perProgress)
    {
        // プログレスバー長さと位置
        float perLeft = 1f - perProgress;
        var scale = bar.transform.localScale;
        scale.x = barAllLength * perLeft;
        bar.transform.localScale = scale;

        float offset = barAllLength * perProgress;
        var pos = bar.transform.position;
        pos.z = offset * 2;
        bar.transform.position = pos;
    }

    public void DrawPlayerIcon(float perProgress)
    {
        float offset = perProgress * wayAllLength;
        var pos = playerIcon.transform.position;
        pos.z = defaultIconZ + offset;
        playerIcon.transform.position = pos;
    }
}