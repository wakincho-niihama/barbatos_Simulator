using UnityEngine;
using Unity.XR.CoreUtils;

public class XRFollowWithFixedZ : MonoBehaviour
{
    [Header("追従対象（例：ロボットのコクピット）")]
    public Transform target;

    [Header("XR Origin（HMDの親）")]
    public XROrigin xrOrigin;

    private float zOffset; // 開始時点のZ軸オフセット

    void Start()
    {
        if (target == null || xrOrigin == null)
        {
            Debug.LogWarning("XRFollowWithFixedZ: target または xrOrigin が設定されていません。");
            enabled = false;
            return;
        }

        // 開始時点での相対Zオフセットを記録（ターゲット座標系で）
        Vector3 localOffset = target.InverseTransformPoint(xrOrigin.transform.position);
        zOffset = localOffset.z;
    }

    void LateUpdate()
    {
        if (target == null || xrOrigin == null) return;

        // 現在のXR Originの位置を、target基準で相対座標に変換
        Vector3 currentLocal = target.InverseTransformPoint(xrOrigin.transform.position);

        // Z軸だけ固定（開始時点のzOffsetを維持）
        currentLocal.z = zOffset;

        // ワールド座標に戻して反映
        Vector3 fixedWorldPos = target.TransformPoint(currentLocal);
        xrOrigin.transform.position = fixedWorldPos;
    }
}
