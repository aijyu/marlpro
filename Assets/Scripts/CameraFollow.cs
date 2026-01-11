using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -10);
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        // X軸のみ追従し、Y軸はある程度固定するか、両方追従するか選べますが、
        // 2DアクションならY軸も追従した方が上下移動に対応しやすいです。
        // ここではシンプルに両方追従させつつ、Lerpで滑らかにします。
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
