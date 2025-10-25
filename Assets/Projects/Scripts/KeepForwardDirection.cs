using UnityEngine;

public class KeepForwardDirection : MonoBehaviour
{
    private Vector3 initialForward;

    void Start()
    {
        // 初期の正面方向を記録
        initialForward = transform.forward;
    }

    void LateUpdate()
    {
        // 現在の回転を取得
        Quaternion rot = transform.rotation;

        // XとZ軸の回転を固定（Yだけ残す）
        transform.rotation = Quaternion.Euler(0f, rot.eulerAngles.y, 0f);
    }
}
