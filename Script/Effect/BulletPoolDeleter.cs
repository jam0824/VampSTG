using UnityEngine;
using System.Collections;

public class BulletPoolDeleter : MonoBehaviour
{
    [Header("削除設定")]
    [SerializeField] private float checkInterval = 5f; // チェック間隔（秒）
    [SerializeField] private float extraRange = 2f; // 範囲拡張値
    
    void Start()
    {
        // 5秒間隔で範囲外オブジェクトの削除を開始
        StartCoroutine(CleanupOutOfRangeBullets());
    }

    IEnumerator CleanupOutOfRangeBullets()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            
            // GameManagerから範囲を取得
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("GameManagerが見つかりません");
                continue;
            }
            
            float maxY = GameManager.Instance.maxY + extraRange;
            float minY = GameManager.Instance.minY - extraRange;
            float maxZ = GameManager.Instance.maxZ + extraRange;
            float minZ = GameManager.Instance.minZ - extraRange;
            
            
            // 非アクティブ化対象のリストを作成
            var listBulletsToDeactivate = new System.Collections.Generic.List<Transform>();
            
            // 全ての子オブジェクトをチェック
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                
                // 既に非アクティブなオブジェクトはスキップ
                if (!child.gameObject.activeInHierarchy)
                    continue;
                    
                Vector3 pos = child.position;
                
                if (IsOutOfRange(pos, minY, maxY, minZ, maxZ))
                {
                    listBulletsToDeactivate.Add(child);
                }
            }
            
            // 非アクティブ化実行
            foreach (Transform child in listBulletsToDeactivate)
            {
                if (child != null && child.gameObject.activeInHierarchy)
                {
                    child.gameObject.SetActive(false);
                }
            }
            
            /*
            if (listBulletsToDeactivate.Count > 0)
            {
                Debug.Log($"弾クリーンアップ完了 - {listBulletsToDeactivate.Count}個非アクティブ化");
            }
            */
        }
    }
    
    bool IsOutOfRange(Vector3 position, float minY, float maxY, float minZ, float maxZ)
    {
        // Y軸範囲チェック
        if (position.y < minY || position.y > maxY)
        {
            return true;
        }
        
        // Z軸範囲チェック
        if (position.z < minZ || position.z > maxZ)
        {
            return true;
        }
        
        // 範囲内の場合
        return false;
    }
    
    void Update()
    {
        // 必要に応じて手動でクリーンアップをトリガーできるように残しておく
    }
}
