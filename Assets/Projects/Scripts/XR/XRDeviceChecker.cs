using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class XRDeviceChecker : MonoBehaviour
{
    void Start()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);

        Debug.Log($"[XR] 検出デバイス数: {devices.Count}");
        foreach (var device in devices)
        {
            Debug.Log($"[XR] デバイス名: {device.name}, ロール: {device.characteristics}");
        }
    }
}
