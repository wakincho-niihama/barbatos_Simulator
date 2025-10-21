using UnityEngine;
using UnityEngine.XR;

public class VRLocomotionController : MonoBehaviour
{
    public float moveSpeed = 1.5f; // 移動速度(m/s)

    private Transform xrOrigin;

    void Start()
    {
        // XR OriginのTransformを取得（このスクリプトをXR OriginにアタッチしてもOK）
        xrOrigin = transform;
    }

    void Update()
    {
        // 右手コントローラ取得
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (rightHand.isValid &&
            rightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 inputAxis))
        {
            // ヘッドセットの向きを基準に進む方向を決める
            Transform hmd = Camera.main.transform;

            // HMDの前方方向（Y軸の回転のみ利用）
            Vector3 forward = new Vector3(hmd.forward.x, 0, hmd.forward.z).normalized;
            Vector3 right = new Vector3(hmd.right.x, 0, hmd.right.z).normalized;

            // 入力に応じて移動
            Vector3 moveDirection = (forward * inputAxis.y + right * inputAxis.x) * moveSpeed * Time.deltaTime;
            xrOrigin.position += moveDirection;
        }
    }
}
