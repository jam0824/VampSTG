using System.Collections.Generic;
using UnityEngine;

public static class VampSTGUtils
{
    /// <summary>
    /// 引数で与えたGameObjectの全ての子（孫以降も含む）を取得して返します。
    /// </summary>
    /// <param name="parent">親となるGameObject</param>
    /// <returns>全子オブジェクトのリスト</returns>
    public static List<GameObject> GetAllChildGameObjects(GameObject parent)
    {
        var listChildren = new List<GameObject>();
        if (parent == null) return listChildren;

        void Collect(Transform t)
        {
            foreach (Transform child in t)
            {
                listChildren.Add(child.gameObject);
                Collect(child);
            }
        }

        Collect(parent.transform);
        return listChildren;
    }

    /// <summary>
    /// 引数で与えたTransform配下の全ての子（孫以降も含む）を取得して返します。
    /// </summary>
    /// <param name="parent">親となるTransform</param>
    /// <returns>全子Transformのリスト</returns>
    public static List<Transform> GetAllChildTransforms(Transform parent)
    {
        var listTransforms = new List<Transform>();
        if (parent == null) return listTransforms;

        void Collect(Transform t)
        {
            foreach (Transform child in t)
            {
                listTransforms.Add(child);
                Collect(child);
            }
        }

        Collect(parent);
        return listTransforms;
    }

    /// <summary>
    /// プールから指定した名前の非アクティブなオブジェクトを探す
    /// </summary>
    /// <param name="pool"></param>
    /// <param name="objectName"></param>
    /// <returns></returns>
    public static GameObject FindInactivePooledObject(GameObject pool, string objectName)
    {
        for (int i = 0; i < pool.transform.childCount; i++)
        {
            Transform child = pool.transform.GetChild(i);
            
            // 名前が一致し、かつ非アクティブなオブジェクトを探す
            if (child.name.Contains(objectName) && !child.gameObject.activeInHierarchy)
            {
                return child.gameObject;
            }
        }
        
        return null; // 見つからない場合
    }
}


