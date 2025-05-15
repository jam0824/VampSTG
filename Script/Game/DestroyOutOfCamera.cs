using UnityEngine;

public class DestroyOutOfCamera : MonoBehaviour
{
    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
