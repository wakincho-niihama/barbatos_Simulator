using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class NewWalk_CC : MonoBehaviour
{

    [Header("参照先(スクリプト)")]
    public GetControllerValues inputHandler;
    public AnimationController animationController;
    public Animator animator;

    [Header("移動パラメータ")]
    public float FrontVelocityAmount = 5f;
    public float FrontVelocityAmount_Back = 3f;
    public float HorizontalVelocityAmount = 4f;
    public float VerticalVelocityMAX = 5f;
    public float jumpPower = 5f;
    public float gravity_amount = 9.81f;
    public float boost_amount = 1.2f;
    public float rotateSpeedAmount = 30f;

    [Header("ステップ・振動")]
    public float stepHeight = 0.5f;
    public float stepFrequency = 0.75f;

    private CharacterController controller;
    private float boost = 1f;
    private float stepTimer = 0f;
    private float offsetY = 0f;

    private Vector3 moveDirection = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    private float baseY;
    private float step;

    // 状態管理
    private bool isGrounded;
    private bool isWalking;
    private bool isSidewalking;
    private bool isBackwalking;
    private bool isIdle;
    private bool isTurningRight;
    private bool isTurningLeft;
    private bool isJumpCharge;
    private bool isJumping;
    private bool isBackBoost;
    private bool isBoosting;
    private bool isRightBoost;
    private bool isLeftBoost;
    private bool isFalling;
    private bool isHoldGun;
    private bool isGunAttack;
    private bool isMaceAttack;

    // 入力
    private Vector2 L_stickInput;
    private Vector2 R_stickInput;
    private bool JumpButton;
    private bool thrastor;
    private bool WeaponState;
    private bool Attack;
    public bool isCockpitActivate { get; private set; }

    // BlendTree反映用
    public float MoveX { get; private set; }
    public float MoveZ { get; private set; }

    void Start()
    {
        animationController = GetComponent<AnimationController>();
        controller = GetComponent<CharacterController>();
        baseY = transform.position.y;
    }

    void Update()
    {
        InitializeParameters();
        InputFromXR();

        // --- 接地判定 ---
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            isJumping = false;
            isFalling = false;
        }

        // --- 水平移動管理 ---
        Vector3 localMove = Vector3.zero;

        // 前進
        if (L_stickInput.y > 0.4f)
        {
            step = stepHeight;
            isWalking = true;
            localMove.z = FrontVelocityAmount;
            if (thrastor) ApplyBoost();
            localMove.z *= boost;
        }
        // 後退
        else if (L_stickInput.y < -0.4f)
        {
            step = stepHeight;
            isBackwalking = true;
            localMove.z = -FrontVelocityAmount_Back;
            if (thrastor)
            {
                ApplyBoost();
                isBackBoost = true;
                isBoosting = false;
            }
            localMove.z *= boost;
        }

        // 右移動
        if (L_stickInput.x > 0.4f)
        {
            step = stepHeight;
            isSidewalking = true;
            MoveX = 1f;
            localMove.x = HorizontalVelocityAmount;
            if (thrastor)
            {
                ApplyBoost();
                isRightBoost = true;
                isBoosting = false;
            }
            localMove.x *= boost;
        }
        // 左移動
        else if (L_stickInput.x < -0.4f)
        {
            step = stepHeight;
            isSidewalking = true;
            MoveX = -1f;
            localMove.x = -HorizontalVelocityAmount;
            if (thrastor)
            {
                ApplyBoost();
                isLeftBoost = true;
                isBoosting = false;
            }
            localMove.x *= boost;
        }

        // --- 上昇・下降 ---
        if (JumpButton)
        {
            isGrounded = false;
            isJumpCharge = true;
            isJumping = true;
            isFalling = false;
            if (velocity.y < VerticalVelocityMAX)
                velocity.y += jumpPower * Time.deltaTime;
        }
        else if (!isGrounded && !isJumping)
        {
            velocity.y -= gravity_amount * Time.deltaTime;
            isFalling = true;
        }

        // --- 回転 ---
        float rotationInput = 0f;
        if (R_stickInput.x > 0.4f)
        {
            isTurningRight = true;
            rotationInput = 1f;
        }
        else if (R_stickInput.x < -0.4f)
        {
            isTurningLeft = true;
            rotationInput = -1f;
        }
        transform.Rotate(Vector3.up * rotationInput * rotateSpeedAmount * Time.deltaTime);

        // --- 実際の移動処理 ---
        moveDirection = transform.TransformDirection(localMove);
        moveDirection.y = velocity.y;
        controller.Move(moveDirection * Time.deltaTime);

        // --- ステップ振動 ---
        Vector3 vibration = Vector3.zero;
        if (isGrounded && (isWalking || isSidewalking || isBackwalking))
        {
            stepTimer += Time.deltaTime * stepFrequency * Mathf.PI * 2;
            offsetY = Mathf.Sin(stepTimer) * step;
            vibration = new Vector3(0, offsetY, 0);
        }

        // --- 状態確定 ---
        isIdle = !isWalking && !isBackwalking && !isSidewalking && !isBoosting && !isJumping && !isFalling;

        // --- アニメーション制御 ---
        if (isWalking) MoveZ = 1f;
        else if (isBackwalking) MoveZ = -1f;

        animationController.UpdateMovement(MoveX, MoveZ);
        animationController.UpdateSpecialStates(
            isBoosting, isJumpCharge, isJumping, isFalling,
            isTurningRight, isTurningLeft, isHoldGun,
            isGunAttack, isMaceAttack, controller.isGrounded,
            isBackBoost, isRightBoost, isLeftBoost
        );
    }

    // --- 以下関数群 ---
    void ApplyBoost()
    {
        boost = boost_amount;
        isBoosting = true;
        step = 0;
    }

    void InitializeParameters()
    {
        boost = 1f;
        isWalking = false;
        isBackwalking = false;
        isSidewalking = false;
        isBoosting = false;
        isBackBoost = false;
        isRightBoost = false;
        isLeftBoost = false;
        isJumpCharge = false;
        isJumping = false;
        isFalling = false;
        isIdle = false;
        isTurningRight = false;
        isTurningLeft = false;
        thrastor = false;
        JumpButton = false;
        Attack = false;
        MoveX = 0f;
        MoveZ = 0f;
    }

    void InputFromXR()
    {
        L_stickInput = inputHandler.L_stickInput;
        R_stickInput = inputHandler.R_stickInput;
        thrastor = inputHandler.L_triggerButton;
        JumpButton = inputHandler.L_gripButton;
        isCockpitActivate = inputHandler.L_stickButton;
        WeaponState = inputHandler.R_menuButton;
        Attack = inputHandler.R_triggerButton;

        isHoldGun = WeaponState;
        if (isHoldGun) isGunAttack = Attack;
        else isMaceAttack = Attack;
    }
}
