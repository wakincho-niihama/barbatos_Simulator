using NUnit.Framework;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR;

[RequireComponent(typeof(Animator))]
public class RobotWalk : MonoBehaviour
{
    //各パラメータ
    public float rotateSpeed=120f;
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

    private Animator animator;//アニメ用インスタンス

    //アニメーション用変数
    private bool isWalking;
    private bool isSidewalking;
    private bool isBackwalking;
    private bool isBoosting;
    private bool isJumping;
    private bool isFalling;
    private bool isIdle;

    //デバイス(コントローラ)設定
    private InputDevice leftHand;
    private InputDevice rightHand;

    void ApplyBoost()
    {
        animator.speed = 0f;//スラスター噴射中はアニメーション再生を停止
        boost = boost_amount;
        step = 0;
    }


    void Start()
    {
        animator = GetComponent<Animator>();
        
        leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        baseY = transform.position.y;
    }

    void Update()
    {
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
        if (rightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 R_axis))
        {
            R_stickInput.x = R_axis.x;
            R_stickInput.y = R_axis.y;
        }

        //各変数初期化

        //入力をしていない間、値が引き継がれるとずっと移動してしまう。移動量が引き継がれないように初期化しておく
        FrontVelocity = 0f;
        HorizontalVelocity = 0f;
        //VerticalVelocity = 0f;//垂直方向は初期化しない。垂直方向操作なしの時、常に下降させるため。

        animator.speed = 1f;//アニメーション再生速度

        step = stepHeight;//上下振動用変数の初期化。構文各所で操作がない限りデフォルトの振動高さにする、スラスター稼働時0になる。

        boost = 1;//スラスター倍率初期値　1以上でスラスター、つまり加速

        //各アニメーション判定変数の初期化
        isWalking = false;
        isBackwalking = false;
        isSidewalking = false;
        isBoosting = false;
        isJumping = false;
        isFalling = false;
        isIdle = false;

        //前移動
        if (L_stickInput.y > 0.4f || Input.GetKey(KeyCode.W))
        {
            isWalking = true;
            FrontVelocity = FrontVelocityAmount;
            //以下を関数化しておく
            if (Input.GetKey(KeyCode.Space))//前進中スペース押下でスラスター噴射
            {
                ApplyBoost();
            }
            FrontVelocity += FrontVelocity * boost * Time.deltaTime;
        }
        else if (L_stickInput.y < -0.4f || Input.GetKey(KeyCode.S))//後移動
        {
            isBackwalking = true;
            FrontVelocity = FrontVelocityAmount_Back;
            if (Input.GetKey(KeyCode.Space))//前進中スペース押下でスラスター噴射
            {
                ApplyBoost();
            }
            FrontVelocity += FrontVelocity * boost * Time.deltaTime;
        }//OK
        

        //横移動
        if (L_stickInput.x > 0.4f || Input.GetKey(KeyCode.D))//前移動
        {
            isSidewalking = true;
            HorizontalVelocity = HorizontalVelocityAmount;
            //以下を関数化しておく
            if (Input.GetKey(KeyCode.Space))//前進中スペース押下でスラスター噴射
            {
                ApplyBoost();
            }
            HorizontalVelocity += HorizontalVelocity * boost * Time.deltaTime;
        }
        else if (L_stickInput.x < -0.4f || Input.GetKey(KeyCode.A))//後移動
        {
            isSidewalking = true;
            HorizontalVelocity = -HorizontalVelocityAmount;
            if (Input.GetKey(KeyCode.Space))//前進中スペース押下でスラスター噴射
            {
                ApplyBoost();
            }
            HorizontalVelocity += HorizontalVelocity * boost * Time.deltaTime;
        }//OK

        //上昇下降の制御
        if (Input.GetKey(KeyCode.UpArrow))
        {
            isJumping = true;
            VerticalVelocity += VerticalVelocityAmount;
        }
        else if (VerticalVelocity > 0.2f)
        {
            isFalling = true;
            VerticalVelocity -= 0.1f;
        }//OK
        
        //回転
        RotationVelocity = 0f;
        if (R_stickInput.x > 0.4f || Input.GetKey(KeyCode.RightArrow)) RotationVelocity = 1f;
        else if (R_stickInput.x < -0.4f || Input.GetKey(KeyCode.LeftArrow)) RotationVelocity = -1f;
        //OK

        //アニメーション制御用
        //IsWalkingで歩行アニメーション再生。閾値で管理
        //スラスター使用時は歩行アニメーション停止

        animator.SetBool("isWalking", isWalking);

        //isWalking = FrontVelocity > 0.01f;
        //isBackwalking = FrontVelocity < 0.01f;
        isSidewalking = math.abs(HorizontalVelocity) > 0.01f;
        isBoosting = boost > 1f;
        isIdle = !isWalking && !isBackwalking && !isSidewalking && !isBoosting && !isJumping && !isFalling;


        Debug.Log($"iswalking : {isWalking}");
        Debug.Log($"isBackwalking : {isBackwalking}");
        Debug.Log($"isSidewalking : {isSidewalking}");
        Debug.Log($"isBoosting : {isBoosting}");
        Debug.Log($"isJumping : {isJumping}");
        Debug.Log($"isfalling : {isFalling}");
        Debug.Log($"isIdle : {isIdle}");

        if (isWalking)
        {
            stepTimer += Time.deltaTime * stepFrequency * Mathf.PI * 2;
            offsetY = Mathf.Sin(stepTimer) * step;

            // baseYを基準に直接セット
            Vector3 pos = transform.position;
            pos.y = baseY + offsetY;
            pos += transform.TransformDirection(new Vector3(HorizontalVelocity, VerticalVelocity, FrontVelocity));
            transform.position = pos;
        }
        else
        {
            stepTimer = 0f;
            offsetY = 0f;
            Vector3 pos = transform.position;
            pos.y = baseY;
            pos += transform.TransformDirection(new Vector3(HorizontalVelocity, VerticalVelocity, FrontVelocity));
            transform.position = pos;
        }
        
        transform.Rotate(Vector3.up * RotationVelocity * rotateSpeed * Time.deltaTime);//回転
        
    }
}