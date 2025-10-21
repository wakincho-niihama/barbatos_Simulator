using UnityEngine;

public class KeepDistanceFromTarget : MonoBehaviour
{
    [Header("追従対象")]
    public Transform target;

    [Header("静止時のオフセット（m, targetのローカル軸基準）")]
    public Vector3 idleOffset = new Vector3(0f, 0f, 0f);

    [Header("移動時のオフセット（m, targetのローカル軸基準）")]
    public Vector3 movingOffset = new Vector3(0f, 0f, -5f);

    [Header("静止判定に使うしきい値")]
    public float moveThreshold = 0.01f;

    [Header("追従スムーズさ")]
    public float followSpeed = 5f;

    private Vector3 lastTargetPos;
    private bool isMoving = false;

    public NewWalk robotWalk;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("Targetが設定されていません。");
            enabled = false;
            return;
        }

        lastTargetPos = target.position;
    }

    void LateUpdate()
    {
        // targetの移動量を算出
        float movedDist = (target.position - lastTargetPos).magnitude;
        isMoving = movedDist > moveThreshold;

        Vector3 localOffset = (robotWalk.FrontVelocity != 0 || robotWalk.RotationVelocity != 0) ? movingOffset : idleOffset;

        // targetのローカル方向を考慮したオフセットをワールド座標へ変換
        Vector3 worldOffset =
            target.right * localOffset.x +
            target.up * localOffset.y +
            target.forward * localOffset.z;

        Vector3 desiredPos = target.position + worldOffset;

        // スムーズに追従させる
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * followSpeed);

        lastTargetPos = target.position;
    }
}
