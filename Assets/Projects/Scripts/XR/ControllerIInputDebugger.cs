using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class ControllerInputDebugger : MonoBehaviour
{
    void Update()
    {
        // 左手コントローラの取得
        UnityEngine.XR.InputDevice leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        //UnityEngine.XR.InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

                // ボタン入力
        if (leftHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryTouch, out bool secondButton))
            Debug.Log($"Left Button second: {secondButton:F2}");

        if (leftHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool primaryButton))//ここです
            Debug.Log($"Left Button primary: {primaryButton:F2}");


        // トリガー値
        if (leftHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float leftTriggerValue))
            Debug.Log($"Left TriggerValue: {leftTriggerValue:F2}");//OK

        if (leftHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool leftTriggerButton))
            Debug.Log($"Left TriggerButton: {leftTriggerButton:F2}");//OK


        // スティック入力
        if (leftHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out Vector2 leftStick))
            Debug.Log($"Left Stick: {leftStick}");//OK

        if (leftHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out bool leftStickClick))
            Debug.Log($"Left Stick Click: {leftStickClick}");//OK


        // グラブ
        if (leftHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out float leftGrip))
            Debug.Log($"Left Grip: {leftGrip:F2}");//無い

        if (leftHand.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out bool IsGrabed))
            Debug.Log($"Left IsGrabed: {IsGrabed:F2}");//OK



    }
}
