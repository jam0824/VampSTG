using UnityEngine;

public class BGObjMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 1f;          // Z軸マイナス方向への移動速度
    [SerializeField] private float rotationSpeed = 30f;     // Y軸回転速度（度/秒）

    [Header("Object Settings")]
    [SerializeField] private bool isRotate = true;
    [SerializeField] private bool isMove = true;

    private void Start()
    {

    }

    private void Update()
    {
        // Y軸でゆっくりと回転
        if (isRotate)
        {
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
        }

        // Z軸のマイナス方向に少しずつ移動
        if (isMove)
        {
            transform.Translate(0f, 0f, -moveSpeed * Time.deltaTime, Space.World);
        }
    }
} 