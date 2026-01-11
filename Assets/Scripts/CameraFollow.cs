using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -10);
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target == null) return;

        // User Request: 上下は固定にしてほしい
        // Y軸は現在のカメラの高さを維持し、X軸のみ追従する
        Vector3 desiredPosition = new Vector3(target.position.x + offset.x, transform.position.y, transform.position.z);
        
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        // Lerp等の計算誤差でZが変わらないように明示的に設定
        smoothedPosition.z = transform.position.z; 
        
        transform.position = smoothedPosition;
    }
}
