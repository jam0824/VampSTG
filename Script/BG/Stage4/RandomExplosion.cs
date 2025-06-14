using System.Collections;
using UnityEngine;

public class RandomExplosion : MonoBehaviour
{
    [Header("爆発設定")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float minExplosionTime = 1f; // 最小爆発時間
    [SerializeField] private float maxExplosionTime = 5f; // 最大爆発時間
    [SerializeField] private float explosionProbability = 0.7f; // 爆発確率（0.0～1.0）
    
    [Header("爆発エフェクト設定")]
    [SerializeField] private Vector3 explosionOffset = Vector3.zero; // 爆発位置のオフセット
    [SerializeField] private float explosionScale = 1f; // 爆発エフェクトのスケール
    
    private bool hasExploded = false;
    
    void Start()
    {
        // ランダム時間後に爆発判定を開始
        float randomTime = Random.Range(minExplosionTime, maxExplosionTime);
        StartCoroutine(ExplosionTimer(randomTime));
    }
    
    /// <summary>
    /// 指定時間後に爆発判定を行う
    /// </summary>
    private IEnumerator ExplosionTimer(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        
        // まだ爆発していない場合のみ判定
        if (!hasExploded)
        {
            CheckForExplosion();
        }
    }
    
    /// <summary>
    /// 爆発するかどうかをランダムで判定
    /// </summary>
    private void CheckForExplosion()
    {
        float randomValue = Random.Range(0f, 1f);
        
        if (randomValue <= explosionProbability)
        {
            Explode();
        }
        else
        {
            // 爆発しない場合は単純にDestroy
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 爆発処理
    /// </summary>
    private void Explode()
    {
        if (hasExploded) return;
        
        hasExploded = true;
        
        // 爆発エフェクトを生成
        if (explosionPrefab != null)
        {
            Vector3 explosionPosition = transform.position + explosionOffset;
            GameObject explosion = Instantiate(explosionPrefab, explosionPosition, Quaternion.identity);
            
            // 爆発エフェクトのスケールを設定
            explosion.transform.localScale = Vector3.one * explosionScale;
        }
        
        // オブジェクトを破壊
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 外部から強制的に爆発させる
    /// </summary>
    public void ForceExplode()
    {
        if (!hasExploded)
        {
            StopAllCoroutines();
            Explode();
        }
    }
    
    /// <summary>
    /// 爆発確率を動的に変更
    /// </summary>
    public void SetExplosionProbability(float newProbability)
    {
        explosionProbability = Mathf.Clamp01(newProbability);
    }
    
    /// <summary>
    /// 爆発時間範囲を動的に変更
    /// </summary>
    public void SetExplosionTimeRange(float minTime, float maxTime)
    {
        minExplosionTime = minTime;
        maxExplosionTime = maxTime;
    }
} 