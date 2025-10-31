using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // 新Input Systemが有効なときだけ参照
#endif

/// <summary>
/// WASD移動（カメラ相対）+ 走る(LeftShift) + ジャンプ(Space) + 重力
/// CharacterController をアタッチして使います。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class WASDMover : MonoBehaviour
{
    [Header("移動設定")]
    public float walkSpeed = 4.0f;          // 通常速度
    public float sprintMultiplier = 1.6f;   // 走り倍率（LeftShift）
    public float acceleration = 12f;        // 水平加速レート（補間用）

    [Header("ジャンプ & 重力")]
    public float jumpHeight = 1.2f;         // ジャンプ高さ（m）
    public float gravity = -9.81f;          // 重力加速度（負値）
    public float groundedStick = -2f;       // 接地時のわずかな下向き速度

    [Header("カメラ相対移動")]
    public bool cameraRelative = true;      // trueでカメラ前方基準、falseでワールド基準

    private CharacterController controller;
    private Vector3 velocity;               // 現在の速度（y含む）

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // --- 入力取得（新旧両対応） ---
        Vector2 moveInput = ReadMoveInput();   // WASD / ←→↑↓
        bool sprint = ReadSprint();            // LeftShift
        bool jump = ReadJump();                // Space

        // --- 接地処理 ---
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
            velocity.y = groundedStick;

        // --- 目標水平速度を作る ---
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (cameraRelative && Camera.main != null)
        {
            // カメラの向きを水平面に投影
            forward = Camera.main.transform.forward;
            right = Camera.main.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
        }

        Vector3 inputDir = (right * moveInput.x + forward * moveInput.y);
        inputDir = inputDir.sqrMagnitude > 1e-4f ? inputDir.normalized : Vector3.zero;

        float targetSpeed = walkSpeed * (sprint ? sprintMultiplier : 1f);
        Vector3 targetHorizontalVel = inputDir * targetSpeed;

        // 現在の水平成分
        Vector3 currentHorizontalVel = new Vector3(velocity.x, 0f, velocity.z);

        // 水平速度をスムーズに目標へ補間
        currentHorizontalVel = Vector3.Lerp(
            currentHorizontalVel,
            targetHorizontalVel,
            1f - Mathf.Exp(-acceleration * Time.deltaTime)  // 時定数ベースの補間
        );

        // --- ジャンプ & 重力 ---
        if (jump && isGrounded)
        {
            // v = sqrt(2gh)（gは負なので-2*g*H）
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        velocity.y += gravity * Time.deltaTime;

        // 新しい速度をまとめる
        velocity.x = currentHorizontalVel.x;
        velocity.z = currentHorizontalVel.z;

        // --- 移動実行 ---
        controller.Move(velocity * Time.deltaTime);

        // 入力がない & 接地中なら水平速度を自然減衰
        if (isGrounded && inputDir == Vector3.zero)
        {
            velocity.x = Mathf.MoveTowards(velocity.x, 0f, targetSpeed * Time.deltaTime);
            velocity.z = Mathf.MoveTowards(velocity.z, 0f, targetSpeed * Time.deltaTime);
        }
    }

    // ===== 入力ヘルパ =====
    private Vector2 ReadMoveInput()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            float x = (kb.dKey.isPressed ? 1f : 0f) + (kb.aKey.isPressed ? -1f : 0f);
            float y = (kb.wKey.isPressed ? 1f : 0f) + (kb.sKey.isPressed ? -1f : 0f);
            if (Mathf.Abs(x) > 1f) x = Mathf.Sign(x);
            if (Mathf.Abs(y) > 1f) y = Mathf.Sign(y);
            return new Vector2(x, y);
        }
#endif
        // 旧Input Manager（Horizontal/Vertical）
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private bool ReadSprint()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.leftShiftKey.isPressed;
#endif
        return Input.GetKey(KeyCode.LeftShift);
    }

    private bool ReadJump()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null) return kb.spaceKey.wasPressedThisFrame;
#endif
        return Input.GetKeyDown(KeyCode.Space);
    }
}
