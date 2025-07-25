using UnityEngine;
using System.Collections;

public class EffectDeleter : MonoBehaviour
{
    [Header("削除設定")]
    [SerializeField] private float checkInterval = 5f; // チェック間隔（秒）
    
    void Start()
    {
        // 5秒間隔でクリーンアップを開始
        StartCoroutine(CleanupEffects());
    }

    IEnumerator CleanupEffects()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            
            Debug.Log($"エフェクトクリーンアップ開始 - 子オブジェクト数: {transform.childCount}");
            
            // 削除対象のリストを作成（逆順で削除するため）
            var listChildrenToDelete = new System.Collections.Generic.List<Transform>();
            
            // 全ての子オブジェクトをチェック
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                
                if (IsEffectFinished(child))
                {
                    listChildrenToDelete.Add(child);
                    Debug.Log($"削除対象: {child.name}");
                }
            }
            
            // 削除実行
            foreach (Transform child in listChildrenToDelete)
            {
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }
            
            if (listChildrenToDelete.Count > 0)
            {
                Debug.Log($"エフェクトクリーンアップ完了 - {listChildrenToDelete.Count}個削除");
            }
        }
    }
    
    bool IsEffectFinished(Transform effectTransform)
    {
        GameObject effectObj = effectTransform.gameObject;
        
        // オブジェクトが非アクティブなら削除対象
        if (!effectObj.activeInHierarchy)
        {
            return true;
        }
        
        // Animatorのチェック
        Animator animator = effectObj.GetComponent<Animator>();
        if (animator != null && animator.enabled)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            // アニメーションが終了していて、ループしていない場合
            if (stateInfo.normalizedTime >= 1.0f && !animator.IsInTransition(0))
            {
                // ループアニメーションかどうかチェック
                AnimatorClipInfo[] clipInfos = animator.GetCurrentAnimatorClipInfo(0);
                if (clipInfos.Length > 0 && !clipInfos[0].clip.isLooping)
                {
                    return true;
                }
            }
        }
        
        // ParticleSystemのチェック
        ParticleSystem particleSystem = effectObj.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            // パーティクルが再生中でない、かつ残りパーティクルもない場合
            if (!particleSystem.isPlaying && particleSystem.particleCount == 0)
            {
                return true;
            }
        }
        
        // AudioSourceのチェック
        AudioSource audioSource = effectObj.GetComponent<AudioSource>();
        if (audioSource != null && !audioSource.isPlaying)
        {
            // 他にアクティブなコンポーネントがない場合は削除対象
            if (animator == null && particleSystem == null)
            {
                return true;
            }
        }
        
        // すべてのチェックをパスした場合は削除しない
        return false;
    }
}
