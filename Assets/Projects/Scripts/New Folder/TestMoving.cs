using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Animator))]
public class TestMoving : MonoBehaviour
{
    public float speed = 2f;
    public float rotateSpeed = 120f;
    public float stepHeight = 0.5f;
    public float stepFrequency = 0.75f;
    public float boost_amount = 1.4f;
    public float jump_amount = 2f;

    public float moveAmount = 1f;//移動量倍率
    public float turn;//旋回倍率
    public float boost;//移動時のスラスター倍率
    public float step;//移動時の上下振動の振幅
    public float jump;

    public float FrontVelocity=0;//前方向
    public float VerticalVelocity=0;//縦方向
    public float HorizontalVelocity=0;//横方向

    Vector3 direction;//移動地点変数　transform.positionに適用　positionはtranslateよりも処理が軽いらしい\

    public bool isBoost;

    private Animator animator;//アニメ用インスタンス
    private float stepTimer = 0f;

    public bool IsWalking { get; private set; } = false;

    private InputDevice leftHand;
    private InputDevice rightHand;

    void Start()
    {
        animator = GetComponent<Animator>();
        leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
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

        //移動
        //アイドル状態では各移動量値は初期値にしておく アイドル状態は静止している
        animator.speed = 1f;
        step = stepHeight;
        isBoost = false;
        boost = 1;//スラスター倍率初期値　1以上でスラスター、つまり加速。moveはあくまで方向制御用
        IsWalking = false;

        //前移動
        if (L_stickInput.y > 0.4f || Input.GetKey(KeyCode.W))
        {
            IsWalking = true;
            FrontVelocity = moveAmount;
            //以下を関数化しておく
            if (Input.GetKey(KeyCode.Space))//前進中スペース押下でスラスター噴射
            {
                IsWalking = false;
                animator.speed = 0f;//スラスター噴射中はモーション停止
                boost = boost_amount;
                step = 0;
                isBoost = true;
            }
        }
        else if (L_stickInput.y < -0.4f || Input.GetKey(KeyCode.S))//後移動
        {
            IsWalking = true;
            FrontVelocity = moveAmount;
            if (Input.GetKey(KeyCode.Space))//前進中スペース押下でスラスター噴射
            {
                IsWalking = false;
                animator.speed = 0f;//スラスター噴射中はモーション停止
                boost = boost_amount;
                step = 0;
                isBoost = true;
            }
        }

        //横移動
        if (L_stickInput.x > 0.4f || Input.GetKey(KeyCode.D))//前移動
        {
            IsWalking = true;
            HorizontalVelocity = 1f;
            //以下を関数化しておく
            if (Input.GetKey(KeyCode.Space))//前進中スペース押下でスラスター噴射
            {
                IsWalking = false;
                animator.speed = 0f;//スラスター噴射中はモーション停止
                boost = boost_amount;
                step = 0;
                isBoost = true;
            }
        }
        else if (L_stickInput.x < -0.4f || Input.GetKey(KeyCode.S))//後移動
        {
            IsWalking = true;
            HorizontalVelocity = -1f;
            if (Input.GetKey(KeyCode.Space))//前進中スペース押下でスラスター噴射
            {
                IsWalking = false;
                animator.speed = 0f;//スラスター噴射中はモーション停止
                boost = boost_amount;
                step = 0;
                isBoost = true;
            }
        }

        //回転
        turn = 0f;
        if (R_stickInput.x > 0.4f || Input.GetKey(KeyCode.RightArrow)) turn = 1f;
        else if (R_stickInput.x < -0.4f || Input.GetKey(KeyCode.LeftArrow)) turn = -1f;

        //上昇下降の制御
        if (Input.GetKey(KeyCode.LeftShift))
        {
            jump = jump_amount;
        }
        else
        {
            if (jump < 0f) jump -= 0.1f;
        }

        Debug.Log($"Num : {direction}");

        // 前後左右移動・左右回転 translateからpositionに変更する
        //transform.Translate(Vector3.forward * move * speed * boost * Time.deltaTime);
        FrontVelocity += FrontVelocity * boost * Time.deltaTime;
        HorizontalVelocity = HorizontalVelocity * speed * boost * Time.deltaTime;
        VerticalVelocity = VerticalVelocity * 1;
        direction = new Vector3(HorizontalVelocity, VerticalVelocity,FrontVelocity);


        transform.position = direction;//移動

        transform.Rotate(Vector3.up * turn * rotateSpeed * Time.deltaTime);//回転


        //アニメーション制御用
        //IsWalkingで歩行アニメーション再生　
        //スラスター使用時は歩行アニメーション停止
        animator.SetBool("isWalking", IsWalking);

        // 上下振動（前後移動はそのまま）
        /*
        if (IsWalking)
        {
            stepTimer += Time.deltaTime * stepFrequency * Mathf.PI * 2;
            float offsetY = Mathf.Abs(Mathf.Sin(stepTimer)) * step;

            // 現在の transform.position の y に加算
            Vector3 pos = transform.position;
            pos.y = offsetY; // 地面からの高さに合わせる場合はオフセットを加える
            transform.position = new Vector3(pos.x, offsetY, pos.z);
        }
        else
        {
            stepTimer = 0f;
        }
        */
    }
}