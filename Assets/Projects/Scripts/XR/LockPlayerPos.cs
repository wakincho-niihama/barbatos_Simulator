using UnityEngine;
using Unity.XR.CoreUtils;  // XR Origin用

public class LockPlayerPosition : MonoBehaviour
{
    public XROrigin xrOrigin;  // XR OriginをInspectorで指定
    public Vector3 fixedPosition = new Vector3(-0.54f,0f,0f); // 固定したいワールド座標
    public bool lockRotation = true; // 向きも固定する場合

    void LateUpdate()
    {
        if (xrOrigin == null) return;

        // ワールド座標を固定
        xrOrigin.transform.position = fixedPosition;

        if (lockRotation)
            xrOrigin.transform.rotation = Quaternion.identity;
    }
}
