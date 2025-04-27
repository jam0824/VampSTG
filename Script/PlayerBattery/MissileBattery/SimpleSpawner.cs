using System.Collections;
using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{

    [SerializeField]
    GameObject prefab = null;
    [SerializeField, Min(1)]
    int spawnCount = 1;
    [SerializeField, Min(0.01f)]
    float spawnInterval = 0.5f;

    bool isSpawning = false;
    Transform thisTransform;
    WaitForSeconds spawnWait;

	void Start()
	{
        thisTransform = transform;
        spawnWait = new WaitForSeconds(spawnInterval);
	}

	void Update()
    {
		if (!isSpawning)
		{
            StartCoroutine(nameof(SpawnTimer));
		}
    }

    IEnumerator SpawnTimer()
	{
        isSpawning = true;

        Spawn();

        yield return spawnWait;

        isSpawning = false;
	}

    void Spawn()
	{
        for(int i = 0; i < spawnCount; i++)
		{
            Instantiate(prefab, thisTransform.position, Quaternion.identity);
		}
	}

}
