using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChangeToStage : MonoBehaviour
{
    [Header("出撃設定")]
    [SerializeField] private Image syutsugekiImage;
    [SerializeField] private AudioClip syutsugekiSe;
    [SerializeField] private float syutsugekiSeVol = 1f;
    [SerializeField] private float fadeDuration = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Syutsugeki());
    }

    //出撃エフェクト表示
    private IEnumerator Syutsugeki(){
        yield return new WaitForSeconds(0.5f);
        SoundManager.Instance.PlaySE(syutsugekiSe, syutsugekiSeVol);
        yield return new WaitForSeconds(0.5f);
        syutsugekiImage.color = new Color(1f,1f,1f,1f);

        string sceneName = GameManager.Instance.selectedStage.sceneName;
        yield return new WaitForSeconds(2f);
        FadeManager.FadeToScene(sceneName, FadeColor.Black);

    }
}
