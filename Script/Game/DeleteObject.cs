using UnityEngine;

public class DeleteObject : MonoBehaviour
{
    [SerializeField] float deleteTime = 2f;
    float elapsedTime = 0;


    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if(elapsedTime >= deleteTime) Destroy(gameObject);
    }
}
