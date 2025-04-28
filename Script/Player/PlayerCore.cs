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
        if(other.CompareTag("Enemy")){
            playerManager.HitEnemy();
        }
    }
}
