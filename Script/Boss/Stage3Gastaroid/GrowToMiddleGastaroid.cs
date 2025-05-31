using UnityEngine;
using System.Collections;

public class GrowToMiddleGastaroid : MonoBehaviour
{
    [Header("MiddleGastaroidのPrefab")]
    [SerializeField] GameObject middleGastaroidPrefab;
    [Header("成長時間")]
    [SerializeField] float growTime = 10f;
    [Header("成長演出設定")]
    [SerializeField] float growAnimationDuration = 0.5f;  // 成長演出の時間
    [SerializeField] float growScale = 1.5f;              // 成長後のスケール倍率

    private Animator animator;
    private Vector3 originalScale;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        originalScale = transform.localScale;  // 元のスケールを保存
        StartCoroutine(GrowToMiddleGastaroidCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator GrowToMiddleGastaroidCoroutine()
    {
        yield return new WaitForSeconds(growTime); 
        animator.SetTrigger("attack");
        
        // サイズを徐々に大きくする演出
        yield return StartCoroutine(GrowSizeCoroutine());
        
        Instantiate(middleGastaroidPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }
    
    /// <summary>
    /// サイズを徐々に大きくするコルーチン
    /// </summary>
    IEnumerator GrowSizeCoroutine()
    {
        Vector3 startScale = originalScale;
        Vector3 targetScale = originalScale * growScale;
        float elapsedTime = 0f;
        
        while (elapsedTime < growAnimationDuration)
        {
            float t = elapsedTime / growAnimationDuration;
            // イージング効果を追加（最初はゆっくり、後半は早く）
            float easedT = t * t;
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, easedT);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 最終スケールを確実に設定
        transform.localScale = targetScale;
    }
}
