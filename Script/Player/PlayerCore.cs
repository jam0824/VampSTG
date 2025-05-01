using UnityEngine;

public class PlayerCore : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] PlayerManager playerManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other != null && playerManager != null &&
        (other.CompareTag("Enemy") || other.CompareTag("Boss")))
        {
            playerManager.HitEnemy();
        }
    }
}
