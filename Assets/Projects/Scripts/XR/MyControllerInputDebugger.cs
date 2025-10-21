using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class MyControllerInputDebugger : MonoBehaviour
{
    void Update()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);

        foreach (var device in devices)
        {
            if (device.characteristics.HasFlag(InputDeviceCharacteristics.Right))
            {
                Debug.Log($"--- {device.name} ---");

                // よく使われる全ボタンをチェック
                CheckButton(device, CommonUsages.menuButton, "menuButton");
                CheckButton(device, CommonUsages.primaryButton, "primaryButton");
                CheckButton(device, CommonUsages.secondaryButton, "secondaryButton");
                CheckButton(device, CommonUsages.primaryTouch, "primaryTouch");
                CheckButton(device, CommonUsages.secondaryTouch, "secondaryTouch");
                CheckButton(device, CommonUsages.primary2DAxisClick, "primary2DAxisClick");
                CheckButton(device, CommonUsages.primary2DAxisTouch, "primary2DAxisTouch");
                CheckButton(device, CommonUsages.gripButton, "gripButton");
                CheckButton(device, CommonUsages.triggerButton, "triggerButton");
                CheckButton(device, CommonUsages.userPresence, "userPresence");
            }
        }
    }

    void CheckButton(InputDevice device, InputFeatureUsage<bool> feature, string name)
    {
        if (device.TryGetFeatureValue(feature, out bool value) && value)
        {
            Debug.Log($"{name}: {value}");
        }
    }
}
