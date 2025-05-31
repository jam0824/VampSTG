using UnityEngine;
using System.Collections.Generic;
public class MakeInnerBg : MonoBehaviour
{
    [Header("Wall Parts")]
    [SerializeField] public List<GameObject> listWallParts;
    [Header("Big Wall Parts")]
    [SerializeField] public List<GameObject> bigWallParts;
    [Header("Floor Parts")]
    [SerializeField] public GameObject floorParts;
    [Header("Scroll Speed")]
    [SerializeField] public float scrollSpeed = 1f;
    [Header("Delete Position")]
    [SerializeField] private float deletePosition = -20f;

    public string floorTag = "Ground";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.back * scrollSpeed * Time.deltaTime);
        DeleteParts();
    }

    void DeleteParts(){
        if(transform.position.z < deletePosition){
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 指定したオブジェクトとその子供全てのタグを"Ground"に設定
    /// </summary>
    /// <param name="targetObject">タグを設定するオブジェクト</param>
    private void SetTagToAllChildren(GameObject targetObject, string tag = "Ground")
    {
        if (targetObject == null) return;

        // 自身のタグを設定
        targetObject.tag = tag;

        // 子オブジェクトのタグも再帰的に設定
        foreach (Transform child in targetObject.transform)
        {
            SetTagToAllChildren(child.gameObject, tag);
        }
    }

    /// <summary>
    /// listWallPartsからランダムに選んだオブジェクトを4つ縦に並べる
    /// y=-4から開始し、3間隔で配置、それぞれy軸で90度回転
    /// </summary>
    public void CreateVerticalWallParts(float startZ)
    {
        if (listWallParts == null || listWallParts.Count == 0)
        {
            Debug.LogWarning("listWallPartsが空またはnullです");
            return;
        }

        float rnd = Random.Range(0, 100);
        float startY = -4f;
        float startX = -1f;
        float interval = 6f;
        int partCount = 2;

        if(rnd < 80){
            for (int i = 0; i < partCount; i++)
            {
                // ランダムにパーツを選択
                int randomIndex = Random.Range(0, listWallParts.Count);
                GameObject selectedPart = listWallParts[randomIndex];

                if (selectedPart == null)
                {
                    Debug.LogWarning($"listWallParts[{randomIndex}]がnullです");
                    continue;
                }

                // 配置位置を計算
                Vector3 position = new Vector3(startX, startY + (i * interval), startZ);
                
                // y軸で90度回転
                Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);

                // オブジェクトを生成
                GameObject instantiatedPart = Instantiate(selectedPart, position, rotation, transform);
                
                // サイズを2倍に設定
                instantiatedPart.transform.localScale = Vector3.one * 2f;
                
                Debug.Log($"Wall Part {i + 1}: {selectedPart.name} を位置 {position} に配置しました");
            }
        }else{
            int randomIndex = Random.Range(0, bigWallParts.Count);
            GameObject selectedPart = bigWallParts[randomIndex];

            // 配置位置を計算
            Vector3 position = new Vector3(startX, startY, startZ);
            
            // y軸で90度回転
            Quaternion rotation = Quaternion.Euler(0f, 90f, 0f);

            // オブジェクトを生成
            GameObject instantiatedPart = Instantiate(selectedPart, position, rotation, transform);
            
            // サイズを2倍に設定
            instantiatedPart.transform.localScale = Vector3.one * 2f;

            Debug.Log($"Big Wall Part {selectedPart.name} を位置 {position} に配置しました");
        }
    }

    public void CreateFloorParts(float startZ, float startY)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, 180f);
        GameObject instantiatedPart = Instantiate(floorParts, new Vector3(0f, startY, startZ),rotation, transform);
        
        // サイズを2倍に設定
        instantiatedPart.transform.localScale = Vector3.one * 2f;
        
        // 生成したオブジェクトとその子供全てにGroundタグを設定
        SetTagToAllChildren(instantiatedPart, floorTag);
    }
    
        
}
