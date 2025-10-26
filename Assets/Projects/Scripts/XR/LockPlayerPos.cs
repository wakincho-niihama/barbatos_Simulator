using UnityEngine;
using Unity.XR.CoreUtils;  // XR Origin用

public class LockPlayerPosition : MonoBehaviour
{
    [Header("追従対象")]
    public Transform targetObject;

    public XROrigin xrOrigin;  // XR OriginをInspectorで指定
    public Vector3 fixedPosition { get; private set; }// 固定したい座標
    public bool lockRotation = true; // 向きも固定する場合

    void Start()
    {
        fixedPosition = targetObject.position;
    }

    void LateUpdate()
    {
        if (xrOrigin == null) return;

        xrOrigin.transform.position = fixedPosition;

        if (lockRotation)
            xrOrigin.transform.rotation = Quaternion.identity;
    }
}
