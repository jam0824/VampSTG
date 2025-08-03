using UnityEngine;

public class ClusterChildrenBomb : MonoBehaviour
{
    public bool isExplosion = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < GameManager.Instance.minY + 0.5f){
            if(isExplosion) EffectController.Instance.PlaySmallExplosion(transform.position, transform.rotation);
            gameObject.SetActive(false);
        }

    }
}
