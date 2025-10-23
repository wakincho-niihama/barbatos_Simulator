using NUnit.Framework;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;

[RequireComponent(typeof(Animator))]
public class NewWalk : MonoBehaviour
{
    //各パラメータ
    public float rotateSpeed = 120f;
    public float stepHeight = 0.5f;
    public float stepFrequency = 0.75f;
    public float boost_amount = 1.2f;

    private float boost;//移動時のスラスター倍率

    public float RotationVelocity = 0;//旋回速度

    public float FrontVelocity = 0;//前方向移動速度
    public float FrontVelocityAmount = 0;//前進移動量
    public float FrontVelocityAmount_Back = 0;//後進移動量

    private float VerticalVelocity = 0;//縦方向
    public float VerticalVelocityAmount = 0;//縦方向移動量

    private float HorizontalVelocity = 0;//横方向
    public float HorizontalVelocityAmount = 0;//横方向移動量

    //振動再現用変数
    private float step;//移動時の上下振動の振幅
    private float stepTimer = 0f;//振動周期
    private float offsetY = 0;//振動振幅
    private float baseY; // 地面高さを記憶

    public float gravity_amount = 9.81f;     // 重力加速度
    public float VerticalVelocityMAX = 0f;//上昇速度最大値
    public float jumpPower = 5f;      // 上昇初速
    private bool isGrounded = true;   // 接地判定（簡易）

    //アニメ用インスタンス
    public AnimationController animationController;
    public Animator animator;

    //状態用変数
    private bool isWalking;
    private bool isSidewalking;
    private bool isBackwalking;
    private bool isBoosting;
    private bool isJumping;
    private bool isFalling;
    private float isTurning;
    private bool isIdle;

    //アニメーション用変数
    public float MoveX = 0f;
    public float MoveZ = 0f;

    //デバイス(コントローラ)設定
    private InputDevice leftHand;
    private InputDevice rightHand;

    private bool JumpButton = false;
    private bool thrastor = false;
    public bool isCockpitActivate;


    void ApplyBoost()
    {
        animator.speed = 0f;//スラスター噴射中はアニメーション再生を停止
        boost = boost_amount;
        step = 0;
    }

    void CheckMovingState()
    {
        Debug.Log($"iswalking : {isWalking}");
        Debug.Log($"isBackwalking : {isBackwalking}");
        Debug.Log($"isSidewalking : {isSidewalking}");
        Debug.Log($"isBoosting : {isBoosting}");
        Debug.Log($"isJumping : {isJumping}");
        Debug.Log($"isfalling : {isFalling}");
        Debug.Log($"isIdle : {isIdle}");
    }


    void Start()
    {
        animationController = GetComponent<AnimationController>();
        animator = GetComponent<Animator>();

        leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        baseY = transform.position.y;
    }

    void Update()
    {
        //各変数初期化

        //入力をしていない間、値が引き継がれるとずっと移動してしまう。移動量が引き継がれないように初期化しておく
        FrontVelocity = 0f;
        HorizontalVelocity = 0f;
        //VerticalVelocity = 0f;//垂直方向は初期化しない。垂直方向操作なしの時、常に下降させるため。

        step = 0;//上下振動用変数の初期化。Idle状態は振動なし

        boost = 1;//スラスター倍率初期値　1以上でスラスター、つまり加速

        //各アニメーション判定変数の初期化
        animator.speed = 1f;

        isWalking = false;
        isBackwalking = false;
        isSidewalking = false;
        isBoosting = false;
        isJumping = false;
        isGrounded = false;
        isFalling = false;
        isIdle = false;
        isTurning = 0;

        thrastor = false;
        JumpButton = false;

        MoveX = 0f;
        MoveZ = 0f;

        //コントローラ値読み取り
        leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        Vector2 L_stickInput = Vector2.zero;
        Vector2 R_stickInput = Vector2.zero;

        if (leftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 L_axis))
        {
            L_stickInput.x = L_axis.x;
            L_stickInput.y = L_axis.y;
        }
        if (rightHand.TryGetFeatureValue(CommonUsages.gripButton, out bool GripButton))
        {
            JumpButton = GripButton;
        }
        //Debug.Log($"is JumpButton : {JumpButton}");
        if (leftHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool TriggerButton))
        {
            thrastor = TriggerButton;
        }
        if (leftHand.TryGetFeatureValue(CommonUsages.menuButton, out bool CockpitButton))
        {
            isCockpitActivate = CockpitButton;
        }

        if (rightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 R_axis))
        {
            R_stickInput.x = R_axis.x;
            R_stickInput.y = R_axis.y;
        }

        //水平移動管理　vector3 localMoveへ代入
        //前移動
        if (L_stickInput.y > 0.4f || Input.GetKey(KeyCode.W))
        {
            step = stepHeight;
            isWalking = true;
            FrontVelocity = FrontVelocityAmount;
            //以下を関数化しておく
            if (Input.GetKey(KeyCode.LeftShift) || thrastor)//前進中スペース押下でスラスター噴射
            {
                ApplyBoost();
            }
            FrontVelocity *= boost;
        }
        else if (L_stickInput.y < -0.4f || Input.GetKey(KeyCode.S))//後移動
        {
            step = stepHeight;
            isBackwalking = true;
            FrontVelocity = FrontVelocityAmount_Back;
            if (Input.GetKey(KeyCode.LeftShift) || thrastor)//前進中スペース押下でスラスター噴射
            {
                ApplyBoost();
            }
            FrontVelocity *= boost;
        }//OK

        //横移動
        if (L_stickInput.x > 0.4f || Input.GetKey(KeyCode.D))//右移動
        {
            step = stepHeight;
            MoveX = 1f;//blendtreeへの反映
            isSidewalking = true;
            HorizontalVelocity = HorizontalVelocityAmount;
            //以下を関数化しておく
            if (Input.GetKey(KeyCode.LeftShift) || thrastor)//前進中スペース押下でスラスター噴射
            {
                ApplyBoost();
            }
            HorizontalVelocity *= boost;
        }
        else if (L_stickInput.x < -0.4f || Input.GetKey(KeyCode.A))//左移動
        {
            step = stepHeight;
            MoveX = -1f;//blendtreeへの反映
            isSidewalking = true;
            HorizontalVelocity = -HorizontalVelocityAmount;
            if (Input.GetKey(KeyCode.LeftShift) || thrastor)//前進中スペース押下でスラスター噴射
            {
                ApplyBoost();
            }
            HorizontalVelocity *= boost;
        }//OK

        Vector3 localMove = new Vector3(HorizontalVelocity, 0f, FrontVelocity);

        /////////////////////////////////////////////////
        /// y軸表現以下　jumpingposへ代入
        /// 
        //上昇下降の制御
        
        if (Input.GetKey(KeyCode.UpArrow) || JumpButton)
        {
            isGrounded = false;
            isJumping = true;
            isFalling = false;
            if (VerticalVelocity < VerticalVelocityMAX)
            {
                VerticalVelocity += jumpPower * Time.deltaTime;  // 初速加える
            }
        }
        else
        {
            isJumping = false;
        }

        // --- 重力処理 ---
        if (!isGrounded && !isJumping)
        {
            VerticalVelocity -= gravity_amount * Time.deltaTime; // 毎フレーム減速
            isFalling = true;
        }

        // --- 着地判定 ---
        Vector3 Jumpingpos = transform.position;
        if (Jumpingpos.y <= baseY && VerticalVelocity < 0)
        {
            Jumpingpos.y = baseY;
            VerticalVelocity = 0f;
            isGrounded = true;
            isFalling = false;
        }
        /////////////////
        /// 以下　rotarion

        //回転
        RotationVelocity = 0f;
        if (R_stickInput.x > 0.4f || Input.GetKey(KeyCode.RightArrow))
        {
            isTurning = 1;
            step = stepHeight;
            RotationVelocity = 1f;
        }
        else if (R_stickInput.x < -0.4f || Input.GetKey(KeyCode.LeftArrow))
        {
            isTurning = -1;
            step = stepHeight;
            RotationVelocity = -1f;
        }
        //OK

        /////////////////
        /// blendtree用のbool変数確認

        isSidewalking = math.abs(HorizontalVelocity) > 0.01f;
        isBoosting = boost > 1f;
        isIdle = !isWalking && !isBackwalking && !isSidewalking && !isBoosting && !isJumping && !isFalling;

        //CheckMovingState();

        /////////////////////////////
        //振動表現
        Vector3 vibrationVector;//振動用vector3変数

        if (isGrounded && !isIdle && !isJumping && !isFalling)
        {
            stepTimer += Time.deltaTime * stepFrequency * Mathf.PI * 2;
            offsetY = Mathf.Sin(stepTimer) * step;

            vibrationVector = new Vector3(0, offsetY, 0);
        }
        else
        {
            offsetY = 0;
            vibrationVector = Vector3.zero;
        }

        ////////////////////////////////////////
        //xyz軸方向の移動量反映

        //移動座標の合成
        Vector3 moveDirection = transform.TransformDirection(localMove);
        moveDirection.y += VerticalVelocity;

        Vector3 finalpos = moveDirection;// + vibrationVector;

        transform.position += finalpos;

        //現在位置でのx-z回転
        transform.Rotate(Vector3.up * RotationVelocity * rotateSpeed * Time.deltaTime);//回転

        //blendtreeへの反映
        if (isWalking) MoveZ = 1f;
        else if (isBackwalking) MoveZ = -1f;

        // AnimationControllerに値を送る
        animationController.UpdateMovement(MoveX, MoveZ);
        animationController.UpdateSpecialStates(isBoosting, isJumping, isFalling,isTurning);

    }
}