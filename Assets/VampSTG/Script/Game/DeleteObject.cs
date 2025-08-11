using UnityEngine;

public class DeleteObject : MonoBehaviour
{
    [SerializeField] float deleteTime = 2f;
    [Header("消さずにDisableにする場合")]
    [SerializeField] bool disableInsteadOfDestroy = false;
    [Header("Colliderのみ先に削除する場合")]
    [SerializeField] bool deleteColliderOnly = false;
    [SerializeField] float colliderDeleteTime = 0.1f;
    float elapsedTime = 0;


    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime >= deleteTime) {
            if(disableInsteadOfDestroy) gameObject.SetActive(false);
            else Destroy(gameObject);
        }
        if(deleteColliderOnly) {
            if(elapsedTime >= colliderDeleteTime) {
                foreach(var collider in GetComponents<Collider>()) {
                    collider.enabled = false;
                }
            }
        }
    }
}
