using UnityEngine;

public class DeleteObject : MonoBehaviour
{
    [SerializeField] float deleteTime = 2f;
    [Header("消さずにDisableにする場合")]
    [SerializeField] bool disableInsteadOfDestroy = false;
    float elapsedTime = 0;


    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime >= deleteTime) {
            if(disableInsteadOfDestroy) gameObject.SetActive(false);
            else Destroy(gameObject);
        }
    }
}
