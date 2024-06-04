// "VIVE SDK 
// © 2020 HTC Corporation. All Rights Reserved.
//
// Unless otherwise required by copyright law and practice,
// upon the execution of HTC SDK license agreement,
// HTC grants you access to and use of the VIVE SDK(s).
// You shall fully comply with all of HTC’s SDK license agreement terms and
// conditions signed by you and all SDK and API requirements,
// specifications, and documentation provided by HTC to You."

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class RigMover : MonoBehaviour
{
    public GameObject playerRig;

    private float analogDetectionThreshold = 0.7f;

    // Update is called once per frame
    void Update()
    {
        MoveRigX();
        MoveRigY();
        RotateRigAxisX();
        RotateRigAxisY();
    }

    void MoveRigY()
    {
        float L_TS_Y_State = KeyAxis2D(kControllerLeftCharacteristics, UnityEngine.XR.CommonUsages.secondary2DAxis).y;
        float L_TP_Y_State = KeyAxis2D(kControllerLeftCharacteristics, UnityEngine.XR.CommonUsages.primary2DAxis).y;

        //DEBUG("L_TS_Y_State: " + L_TS_Y_State + " R_TS_Y_State: " + R_TS_Y_State + " L_TP_Y_State: " + L_TP_Y_State + " R_TP_Y_State: " + R_TP_Y_State);

        if (L_TS_Y_State > analogDetectionThreshold ||
            L_TP_Y_State > analogDetectionThreshold)
        {
            playerRig.transform.position = new Vector3(playerRig.transform.position.x, playerRig.transform.position.y, playerRig.transform.position.z + 0.01f);
        }
        else if (L_TS_Y_State < -analogDetectionThreshold ||
                 L_TP_Y_State < -analogDetectionThreshold)
        {
            playerRig.transform.position = new Vector3(playerRig.transform.position.x, playerRig.transform.position.y, playerRig.transform.position.z - 0.01f);
        }
    }

    void MoveRigX()
    {
        float L_TS_X_State = KeyAxis2D(kControllerLeftCharacteristics, UnityEngine.XR.CommonUsages.secondary2DAxis).x;
        float L_TP_X_State = KeyAxis2D(kControllerLeftCharacteristics, UnityEngine.XR.CommonUsages.primary2DAxis).x;

        //DEBUG("L_TS_X_State: " + L_TS_X_State + " R_TS_X_State: " + R_TS_X_State + " L_TP_X_State: " + L_TP_X_State + " R_TP_X_State: " + R_TP_X_State);

        if (L_TS_X_State > analogDetectionThreshold ||
            L_TP_X_State > analogDetectionThreshold)
        {
            playerRig.transform.position = new Vector3(playerRig.transform.position.x + 0.01f, playerRig.transform.position.y, playerRig.transform.position.z);
        }
        else if (L_TS_X_State < -analogDetectionThreshold ||
                 L_TP_X_State < -analogDetectionThreshold)
        {
            playerRig.transform.position = new Vector3(playerRig.transform.position.x - 0.01f, playerRig.transform.position.y, playerRig.transform.position.z);
        }
    }

    void RotateRigAxisX()
    {
        float R_TS_Y_State = KeyAxis2D(kControllerRightCharacteristics, UnityEngine.XR.CommonUsages.secondary2DAxis).y;
        float R_TP_Y_State = KeyAxis2D(kControllerRightCharacteristics, UnityEngine.XR.CommonUsages.primary2DAxis).y;

        if (R_TS_Y_State > analogDetectionThreshold ||
            R_TP_Y_State > analogDetectionThreshold)
        {
            playerRig.transform.RotateAround(playerRig.transform.position, Vector3.right, 0.1f);
        }
        else if (R_TS_Y_State < -analogDetectionThreshold ||
                 R_TP_Y_State < -analogDetectionThreshold)
        {
            playerRig.transform.RotateAround(playerRig.transform.position, Vector3.right, -0.1f);
        }
    }

    void RotateRigAxisY()
    {
        float R_TS_X_State = KeyAxis2D(kControllerRightCharacteristics, UnityEngine.XR.CommonUsages.secondary2DAxis).x;
        float R_TP_X_State = KeyAxis2D(kControllerRightCharacteristics, UnityEngine.XR.CommonUsages.primary2DAxis).x;

        if (R_TS_X_State > analogDetectionThreshold ||
            R_TP_X_State > analogDetectionThreshold)
        {
            playerRig.transform.RotateAround(playerRig.transform.position, Vector3.up, 0.1f);
        }
        else if (R_TS_X_State < -analogDetectionThreshold ||
                 R_TP_X_State < -analogDetectionThreshold)
        {
            playerRig.transform.RotateAround(playerRig.transform.position, Vector3.up, -0.1f);
        }
    }

    public void ResetPlayerRig()
    {
        playerRig.transform.position = Vector3.zero;
        playerRig.transform.rotation = Quaternion.identity;
    }

    #region Unity XR Buttons
    internal static List<UnityEngine.XR.InputDevice> m_InputDevices = new List<UnityEngine.XR.InputDevice>();
    private static Vector2 KeyAxis2D(InputDeviceCharacteristics device, InputFeatureUsage<Vector2> button)
    {
        Vector2 axis2d = Vector2.zero;

        InputDevices.GetDevices(m_InputDevices);
        foreach (UnityEngine.XR.InputDevice id in m_InputDevices)
        {
            // The device is connected.
            if (id.characteristics.Equals(device))
            {
                if (id.TryGetFeatureValue(button, out Vector2 value))
                {
                    axis2d = value;
                }
            }
        }

        return axis2d;
    }

    /// <summary> VIVE Left Controller Characteristics </summary>
    private const InputDeviceCharacteristics kControllerLeftCharacteristics = (
        InputDeviceCharacteristics.Left |
        InputDeviceCharacteristics.TrackedDevice |
        InputDeviceCharacteristics.Controller |
        InputDeviceCharacteristics.HeldInHand
    );
    /// <summary> VIVE Right Controller Characteristics </summary>
    private const InputDeviceCharacteristics kControllerRightCharacteristics = (
        InputDeviceCharacteristics.Right |
        InputDeviceCharacteristics.TrackedDevice |
        InputDeviceCharacteristics.Controller |
        InputDeviceCharacteristics.HeldInHand
    );
    #endregion
}
