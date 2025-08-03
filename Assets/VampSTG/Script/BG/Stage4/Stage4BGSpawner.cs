using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Stage4BGSpawner : MonoBehaviour
{
    [Header("隕石")]
    [SerializeField] private List<GameObject> meteorPrefabs;
    [SerializeField] private float meteorSpawnInterval = 1f;
    [SerializeField] private float meteorSpawnRandomDelay = 0f;
    [SerializeField] private float meteorMoveSpeed = 2f;
    [SerializeField] private float minY = -20f;
    [SerializeField] private float maxY = 20f;
    [SerializeField] private float minX = -50f;
    [SerializeField] private float maxX = -30f;
    [SerializeField] private float startZ = 50f; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnMeteor());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator SpawnMeteor()
    {
        while (true)
        {
            // meteorPrefabsの中からランダムで選択
            int randomIndex = Random.Range(0, meteorPrefabs.Count);
            GameObject selectedMeteor = meteorPrefabs[randomIndex];
            
            // スポーン位置を設定
            float randomY = Random.Range(minY, maxY);
            float randomX = Random.Range(minX, maxX);
            float fixedZ = startZ;
            
            Vector3 spawnPosition = new Vector3(randomX, randomY, fixedZ);
            
            // 隕石をスポーン
            GameObject meteor = Instantiate(selectedMeteor, spawnPosition, Quaternion.identity);
            meteor.GetComponent<BGObjMove>().moveSpeed = meteorMoveSpeed;
            
            // ランダムな回転を設定
            float randomRotateX = Random.Range(0f, 360f);
            float randomRotateY = Random.Range(0f, 360f);
            float randomRotateZ = Random.Range(0f, 360f);
            meteor.transform.rotation = Quaternion.Euler(randomRotateX, randomRotateY, randomRotateZ);
            
            
            yield return new WaitForSeconds(meteorSpawnInterval + Random.Range(0f, meteorSpawnRandomDelay));
        }
    }
}
