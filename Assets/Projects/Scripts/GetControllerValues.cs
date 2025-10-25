using UnityEngine;
using UnityEngine.XR;

public class GetControllerValues : MonoBehaviour
{
    [Header("If you check Button state")]
    public bool showState;
    //デバイス(コントローラ)設定
    private InputDevice leftHand;
    private InputDevice rightHand;

    //入力値保管変数

    //stickInput (Left or Right)
    public Vector2 L_stickInput;//{ get; private set; }
    public Vector2 R_stickInput;// { get; private set; }

    //buttunInput (Left or Right)
    public bool L_stickButton { get; private set; }
    public bool L_menuButton { get; private set; }
    public bool L_triggerButton { get; private set; }
    public bool L_gripButton { get; private set; }

    public bool R_stickButton { get; private set; }
    public bool R_menuButton { get; private set; }
    public bool R_triggerButton { get; private set; }
    public bool R_gripButton { get; private set; }

    void checkInput()
    {
        Debug.Log($"L_stickInput : {L_stickInput}");
        Debug.Log($"L_stickButton : {L_stickButton}");
        Debug.Log($"L_gripButton : {L_gripButton}");
        Debug.Log($"L_menuButton : {L_menuButton}");
        Debug.Log($"L_triggerButton : {L_triggerButton}");

        Debug.Log($"R_stickINput : {R_stickInput}");
        Debug.Log($"R_stickButton : {R_stickButton}");
        Debug.Log($"R_gripButton : {R_gripButton}");
        Debug.Log($"R_menuButton : {R_menuButton}");
        Debug.Log($"R_triggerButton : {R_triggerButton}");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    // Update is called once per frame
    void Update()
    {

        //コントローラ再接続時処理
        if (!leftHand.isValid)
            leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (!rightHand.isValid)
            rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);


        //stickInput
        if (leftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 L_axis))
            L_stickInput = L_axis;
        else
            L_stickInput = Vector2.zero;

        if (rightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 R_axis))
            R_stickInput = R_axis;
        else
            R_stickInput = Vector2.zero;

        //buttunInput
        //Left
        if (leftHand.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool L_StickButton))
            L_stickButton = L_StickButton;
        else
            L_stickButton = false;

        if (leftHand.TryGetFeatureValue(CommonUsages.gripButton, out bool L_GripButton))
            L_gripButton = L_GripButton;
        else
            L_gripButton = false;

        if (leftHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool L_TriggerButton))
            L_triggerButton = L_TriggerButton;
        else
            L_triggerButton = false;

        if (leftHand.TryGetFeatureValue(CommonUsages.menuButton, out bool L_MenuButton))
            L_menuButton = L_MenuButton;
        else
            L_menuButton = false;

        //Right
        if (leftHand.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool R_StickButton))
            R_stickButton = R_StickButton;
        else
            R_stickButton = false;

        if (rightHand.TryGetFeatureValue(CommonUsages.gripButton, out bool R_GripButton))
            R_gripButton = R_GripButton;
        else
            R_gripButton = false;

        if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool R_TriggerButton))
            R_triggerButton = R_TriggerButton;
        else
            R_triggerButton = false;

        if (rightHand.TryGetFeatureValue(CommonUsages.menuButton, out bool R_MenuButton))
            R_menuButton = R_MenuButton;
        else
            R_menuButton = false;

        //from keyboad
        L_stickInput.y = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;
        L_stickInput.x = Input.GetKey(KeyCode.D) ? 1f : Input.GetKey(KeyCode.A) ? -1f : 0f;
        R_stickInput.x = Input.GetKey(KeyCode.RightArrow) ? 1f : Input.GetKey(KeyCode.LeftArrow) ? -1f : 0f;
        R_stickInput.y = Input.GetKey(KeyCode.UpArrow) ? 1f : Input.GetKey(KeyCode.DownArrow) ? -1f : 0f;

        L_stickButton = Input.GetKey(KeyCode.Y);
        L_menuButton = Input.GetKey(KeyCode.U);
        L_triggerButton = Input.GetKey(KeyCode.I);
        L_gripButton = Input.GetKey(KeyCode.O);

        R_stickButton = Input.GetKey(KeyCode.H);
        R_menuButton = Input.GetKey(KeyCode.J);
        R_triggerButton = Input.GetKey(KeyCode.K);
        R_gripButton = Input.GetKey(KeyCode.L);


        if (showState) checkInput();

    }
}
