using JetBrains.Annotations;
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
    [Header("参照先(スクリプト)")]
    public GetControllerValues inputHandler;

    //アニメ用インスタンス
    public AnimationController animationController;
    public Animator animator;

    [Header("各パラメータ")]
    public float rotateSpeedAmount = 30f;
    public float stepHeight = 0.5f;
    public float stepFrequency = 0.75f;
    public float boost_amount = 1.2f;

    private float boost;//移動時のスラスター倍率

    public float RotationVelocity { get; private set; }//旋回速度

    public float FrontVelocity { get; private set; }//前方向移動速度
    public float FrontVelocityAmount = 0;//前進移動量
    public float FrontVelocityAmount_Back = 0;//後進移動量

    public float VerticalVelocity { get; private set; }//縦方向
    public float VerticalVelocityAmount = 0;//縦方向移動量

    public float HorizontalVelocity { get; private set; }//横方向
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

    //状態用変数(スクリプトのみ使用)
    private bool isWalking;
    private bool isSidewalking;
    private bool isBackwalking;
    private bool isIdle;

    //アニメーション用変数(外部参照でも使用)
    public float MoveX { get; private set; }
    public float MoveZ { get; private set; }
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

    private Vector2 L_stickInput;
    private Vector2 R_stickInput;
    private bool JumpButton;
    private bool thrastor;
    private bool WeaponState;
    public bool Attack { get; private set; }

    public bool isCockpitActivate { get; private set; }//コクピット開閉アクションで使用

    public bool CheckState;


    void ApplyBoost()
    {
        //animator.speed = 0f;//スラスター噴射中はアニメーション再生を停止
        boost = boost_amount;
        isBoosting = true;
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

        Debug.Log($"isTuringRight : {isTurningRight}");
        Debug.Log($"isTuringLeft : {isTurningLeft}");

        Debug.Log($"Attack : {Attack}");

        Debug.Log($"isHoldGun : {isHoldGun}");
    }

    void InitializePrameters()
    {
        //各変数初期化

        //入力をしていない間、値が引き継がれるとずっと移動してしまう。移動量が引き継がれないように初期化しておく
        FrontVelocity = 0f;
        HorizontalVelocity = 0f;
        //VerticalVelocity = 0f;//垂直方向は初期化しない。垂直方向操作なしの時、常に下降させるため。

        step = 0;//上下振動用変数の初期化。Idle状態は振動なし

        boost = 1;//スラスター倍率初期値　1以上でスラスター、つまり加速

        isWalking = false;
        isBackwalking = false;
        isSidewalking = false;

        isBoosting = false;
        isBackBoost = false;
        isRightBoost = false;
        isLeftBoost = false;

        isJumpCharge = false;
        isJumping = false;
        isGrounded = false;
        isFalling = false;

        isIdle = false;

        isTurningRight = false;
        isTurningLeft = false;

        thrastor = false;
        JumpButton = false;
        Attack = false;
        //WeaponState=WeaponState;//武器選択状態は次のUpdateでも引き継ぐ

        MoveX = 0f;
        MoveZ = 0f;
    }

    void InputFromXR()//この部分によりScene中で使う変数をXRコントローラから取得
    {
        L_stickInput = inputHandler.L_stickInput;
        R_stickInput = inputHandler.R_stickInput;

        thrastor = inputHandler.L_triggerButton;
        JumpButton = inputHandler.L_gripButton;
        isCockpitActivate = inputHandler.L_stickButton;
        WeaponState = inputHandler.R_menuButton;//weaponState:1->Gun,weaponState:0->Mace
        Attack = inputHandler.R_triggerButton;//weaponState:1->isGunAttack=Attack,weaponState:0->isMaceAttack=Attack
    }


    void Start()
    {
        animationController = GetComponent<AnimationController>();
        baseY = -0.5f;
    }


    void Update()
    {

        InitializePrameters();
        InputFromXR();

        //水平移動管理　vector3 localMoveへ代入
        //前移動
        if (L_stickInput.y > 0.4f)
        {
            step = stepHeight;
            isWalking = true;
            FrontVelocity = FrontVelocityAmount;
            //以下を関数化しておく
            if (thrastor)
            {
                ApplyBoost();
            }
            FrontVelocity *= boost;
        }
        else if (L_stickInput.y < -0.4f)//後移動
        {
            step = stepHeight;
            isBackwalking = true;
            FrontVelocity = FrontVelocityAmount_Back;
            if (thrastor)
            {
                ApplyBoost();
                isBackBoost = true;//後退ブースト時のみ特殊アニメーション
                isBoosting = false;//後退時前進ダッシュはさせない
            }
            FrontVelocity *= boost;
        }//OK

        //横移動
        if (L_stickInput.x > 0.4f)//右移動
        {
            step = stepHeight;
            MoveX = 1f;//blendtreeへの反映
            isSidewalking = true;
            HorizontalVelocity = HorizontalVelocityAmount;
            //以下を関数化しておく
            if (thrastor)//前進中スペース押下でスラスター噴射
            {
                ApplyBoost();
                isRightBoost = true;
                isBoosting = false;
            }
            HorizontalVelocity *= boost;
        }
        else if (L_stickInput.x < -0.4f)//左移動
        {
            step = stepHeight;
            MoveX = -1f;//blendtreeへの反映
            isSidewalking = true;
            HorizontalVelocity = -HorizontalVelocityAmount;
            if (thrastor)//前進中スペース押下でスラスター噴射
            {
                ApplyBoost();
                isLeftBoost = true;
                isBoosting = false;
            }
            HorizontalVelocity *= boost;
        }//OK

        Vector3 localMove = new Vector3(HorizontalVelocity, 0f, FrontVelocity);

        /////////////////////////////////////////////////
        /// y軸表現以下　jumpingposへ代入
        /// 
        //上昇下降の制御

        if (JumpButton)
        {
            isGrounded = false;
            isJumpCharge = true;
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
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out hit, 2f))
        {
            baseY = hit.point.y; // 足元の地面の高さを更新
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
        if (R_stickInput.x > 0.4f)
        {
            isTurningRight = true;
            step = stepHeight;
            RotationVelocity = 1f;
        }
        else if (R_stickInput.x < -0.4f)
        {
            isTurningLeft = true;
            step = stepHeight;
            RotationVelocity = -1f;
        }
        //OK

        ////////
        /// 以下　武装管理
        ///
        isHoldGun = WeaponState;

        //weaponState:1->isGunAttack=Attack,weaponState:0->isMaceAttack=Attack
        if (isHoldGun)
        {
            isGunAttack = Attack;
        }
        else
        {
            isMaceAttack = Attack;
        }

        /////////////////
        /// blendtree用のbool変数確認
        isSidewalking = math.abs(HorizontalVelocity) > 0.01f;
        isIdle = !isWalking && !isBackwalking && !isSidewalking && !isBoosting && !isJumping && !isFalling;

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
        transform.Rotate(Vector3.up * RotationVelocity * rotateSpeedAmount * Time.deltaTime);//回転

        //blendtreeへの反映
        if (isWalking) MoveZ = 1f;
        else if (isBackwalking) MoveZ = -1f;

        // AnimationControllerに値を送る
        animationController.UpdateMovement(MoveX, MoveZ);
        animationController.UpdateSpecialStates(isBoosting, isJumpCharge, isJumping, isFalling, isTurningRight, isTurningLeft, isHoldGun, isGunAttack, isMaceAttack, isGrounded, isBackBoost, isRightBoost, isLeftBoost);

        // --- 座標補正（床めり込み防止） ---
        Vector3 pos = transform.position;

        // 基準より下がっていたら強制的に補正
        if (pos.y < baseY)
        {
            pos.y = baseY;
            VerticalVelocity = 0f;   // 垂直速度リセット（下方向速度を消す）
            isGrounded = true;       // 接地判定ON
            isFalling = false;
            isJumping = false;
        }

        transform.position = pos;

        if (CheckState) CheckMovingState();
    }
}