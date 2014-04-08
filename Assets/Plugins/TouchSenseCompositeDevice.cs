/*
** =========================================================================
** Copyright (c) 2012 Immersion Corporation.  All rights reserved.
**                    Immersion Corporation Confidential and Proprietary
**
** File:
**  TouchSenseCompositeDevice.cs
**
** Description:
**  Unity3d C# code for defining a composite device
** =========================================================================
*/

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

/// <summary>
/// Class representing a logical combination of discrete devices
///
/// The composite device supports playing different effects simultaneously on different physical devices,
/// or actuators. Effects may be defined specifying a device index on which to play the effect.
/// When played on a composite device, the effects are rendered on the physical device, or actuator
/// corresponding with the specified device index.
/// </summary>
public class TouchSenseCompositeDevice : TouchSenseDevice {

    private int m_compositeDeviceAmount = -1;

    public override ActuatorType type
    {
        get
        {
            return ActuatorType.Composite;
        }
    }

    /// <summary>
    /// Creates a composite device
    /// </summary>
    /// <param name="deviceCount">The amount of devices to use in this composite device</param>
    public TouchSenseCompositeDevice(int deviceCount)
    {
        m_compositeDeviceAmount = deviceCount;
    }

    protected override bool openDevice()
    {
        if (m_deviceHandle == -1)
        {
#if UNITY_ANDROID
            int rv = ImmVibeOpenCompositeDevice(IntPtr.Zero, m_compositeDeviceAmount, ref m_deviceHandle);
            if (rv != 0)
            {
                Debug.LogError("TouchSenseCompositeDevice: OpenCompositeDevice failed with return code " + rv);
                return false;
            }
#endif
        }

        return true;
    }

#if UNITY_ANDROID
    [DllImport("libImmEmulatorJ.so")]
    private static extern int ImmVibeOpenCompositeDevice(IntPtr nDeviceIndex, int nNumDevice,
        ref int phDeviceHandle);
#endif

}
