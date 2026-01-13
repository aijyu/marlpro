using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -10);
    public float smoothSpeed = 0.125f;

    public float scrollSpeed = 2.0f;

    void Update()
    {
        // 強制スクロール: Y軸方向に一定速度で移動
        transform.position += Vector3.up * scrollSpeed * Time.deltaTime;
        
        // プレイヤーのX座標も追従するかどうか？
        // 塔ならカメラは中央固定で、プレイヤーが左右に動く形が一般的。
        // なのでTarget追従は完全に廃止する。
    }
}
