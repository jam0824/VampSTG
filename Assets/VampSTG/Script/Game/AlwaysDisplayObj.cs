using UnityEngine;

public class AlwaysDisplayObj : MonoBehaviour
{
    Vector3 pos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pos = new Vector3(0, GameManager.Instance.minY+1f, GameManager.Instance.minZ+1f);
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = pos;
    }
}
