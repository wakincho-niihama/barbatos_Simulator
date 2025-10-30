using UnityEngine;

// Rigidbodyコンポーネントが必須であることを示す
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5.0f; // プレイヤーの移動速度

    [Header("Camera Control")]
    public float mouseSensitivity = 2.0f; // マウス感度
    public Transform cameraTransform; // カメラのTransform

    private Rigidbody rb;
    private float cameraPitch = 0.0f; // カメラの上下の回転角度（X軸回転）

    void Start()
    {
        // コンポーネントを取得
        rb = GetComponent<Rigidbody>();

        // カメラが設定されていなければ、子オブジェクトから探す
        if (cameraTransform == null)
        {
            cameraTransform = GetComponentInChildren<Camera>().transform;
        }

        // Rigidbodyの不要な回転を固定
        rb.freezeRotation = true;

        // カーソルを画面中央にロックし、非表示にする
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // マウスによる視点移動
        LookAround();
    }

    void FixedUpdate()
    {
        // キーボードによる移動
        Move();
    }

    // プレイヤーの移動処理
    void Move()
    {
        // WASDキーの入力を取得
        float horizontalInput = Input.GetAxis("Horizontal"); // A, Dキー
        float verticalInput = Input.GetAxis("Vertical");     // W, Sキー

        // 入力に基づいた移動方向のベクトルを計算
        Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;

        // 1. プレイヤーの向きを考慮した「目標の速度」を計算
        Vector3 targetVelocity = transform.TransformDirection(moveDirection) * moveSpeed;

        // 2. 現在の速度を取得
        Vector3 currentVelocity = rb.linearVelocity; // .velocity を .linearVelocity に変更

        // 3. 目標の速度と現在の速度の「差」を計算
        //    (Y軸方向の速度は重力に任せたいため、Y=0 にして変更しない)
        Vector3 velocityChange = (targetVelocity - currentVelocity);
        velocityChange.y = 0; // Y軸(垂直方向)の速度は変更しない

        // 4. ForceMode.VelocityChange を使って、質量(mass)を無視して「即座に」速度を変更する
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    // 視点移動処理
    void LookAround()
    {
        // マウスの移動量を取得
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 左右の視点移動（プレイヤー本体をY軸周りに回転させる）
        transform.Rotate(Vector3.up * mouseX);

        // 上下の視点移動（カメラをX軸周りに回転させる）
        cameraPitch -= mouseY; // マウスを上に動かすと視点も上を向くように '-' をつける

        // カメラの上下回転角度を制限（-90度から90度まで）
        cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);

        // カメラのX軸回転を更新（ローカル座標基準で回転）
        cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0, 0);
    }
}