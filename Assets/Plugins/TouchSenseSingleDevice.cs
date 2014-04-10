/*
** =========================================================================
** Copyright (c) 2012 Immersion Corporation.  All rights reserved.
**                    Immersion Corporation Confidential and Proprietary
**
** File:
**  TouchSense.cs
**
** Description:
**  Unity3d C# code for representing an actuator
** =========================================================================
*/

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Class representing a single discrete vibration actuator
/// </summary>
public class TouchSenseSingleDevice : TouchSenseDevice {

    private int m_deviceIndex = -1;
    private ActuatorType m_type = ActuatorType.Unknown;

    public override ActuatorType type
    {
        get
        {
            if (m_type == ActuatorType.Unknown)
            {
                int val = 0;
                //VIBE_DEVCAPTYPE_ACTUATOR_TYPE = 3
#if UNITY_ANDROID
                int rv = ImmVibeGetDeviceCapabilityInt32(m_deviceIndex, 3, ref val);
                if (rv != 0)
                {
                    Debug.LogError(
                        "TouchSenseSingleDevice: GetDeviceCapability (device type) failed with error code " + rv);
                    return m_type;
                }
#endif
                switch (val)
                {
                    case 0:
                    default:
                        m_type = ActuatorType.ERM;
                        break;
                    case 1:
                        m_type = ActuatorType.BLDC;
                        break;
                    case 2:
                        m_type = ActuatorType.LRA;
                        break;
                    case 4:
                        m_type = ActuatorType.Piezo;
                        break;
                }
            }
            return m_type;
        }
    }

    /// <summary>
    /// The name of this device
    /// </summary>
    public string name
    {
        get
        {
            StringBuilder bldr = new StringBuilder(512);
            //VIBE_DEVCAPTYPE_DEVICE_NAME = 10
#if UNITY_ANDROID
            int rv = ImmVibeGetDeviceCapabilityString(m_deviceIndex, 10, 512, bldr);
            if(rv != 0)
            {
                Debug.LogError(
                    "TouchSenseSingleDevice: GetDeviceCapability (device name) failed with error code " + rv);
            }

#endif
            return bldr.ToString();
        }
    }

    /// <summary>
    /// Number of effect slots. The number of effect slots represents the maximum number of simple
    /// effects that may play simultaneously. If an attempt is made to play more than this number of
    /// effects at the same time, some of the simple effects will not play.
    /// </summary>
    public int numEffectSlots
    {
        get
        {
            int val = 0;
            //VIBE_DEVCAPTYPE_NUM_EFFECT_SLOTS            4
#if UNITY_ANDROID
            int rv = ImmVibeGetDeviceCapabilityInt32(m_deviceIndex, 4, ref val);
            if (rv != 0)
            {
                Debug.LogError(
                    "TouchSenseSingleDevice: GetDeviceCapability (num effect slots) failed with error code " + rv);
            }
#endif
            return val;
        }
    }

    /// <summary>
    /// Minimum period for Periodic effects.
    /// </summary>
    public int minPeriod
    {
        get
        {
            int val = 0;
            //VIBE_DEVCAPTYPE_MIN_PERIOD                  6
#if UNITY_ANDROID
            int rv = ImmVibeGetDeviceCapabilityInt32(m_deviceIndex, 6, ref val);
            if (rv != 0)
            {
                Debug.LogError(
                    "TouchSenseSingleDevice: GetDeviceCapability (min period) failed with error code " + rv);
            }
#endif
            return val;
        }
    }

    /// <summary>
    /// Maximum period for Periodic effects.
    /// </summary>
    public int maxPeriod
    {
        get
        {
            int val = 0;
            //VIBE_DEVCAPTYPE_MAX_PERIOD                  7
#if UNITY_ANDROID
            int rv = ImmVibeGetDeviceCapabilityInt32(m_deviceIndex, 7, ref val);
            if (rv != 0)
            {
                Debug.LogError(
                    "TouchSenseSingleDevice: GetDeviceCapability (max period) failed with error code " + rv);
            }
#endif
            return val;
        }
    }

    /// <summary>
    /// Maximum duration for simple effects measured in milliseconds.
    /// </summary>
    public int maxEffectDuration
    {
        get
        {
            int val = 0;
            //VIBE_DEVCAPTYPE_MAX_EFFECT_DURATION         8
#if UNITY_ANDROID
            int rv = ImmVibeGetDeviceCapabilityInt32(m_deviceIndex, 8, ref val);
            if (rv != 0)
            {
                Debug.LogError(
                    "TouchSenseSingleDevice: GetDeviceCapability (max effect duration) failed with error code " + rv);
            }
#endif
            return val;
        }
    }

    /// <summary>
    /// Maximum start time or fade time in milliseconds for effect envelopes of simple effects.
    /// </summary>
    public int maxEnvelopeTime
    {
        get
        {
            int val = 0;
            //VIBE_DEVCAPTYPE_MAX_ENVELOPE_TIME           11
#if UNITY_ANDROID
            int rv = ImmVibeGetDeviceCapabilityInt32(m_deviceIndex, 11, ref val);
            if (rv != 0)
            {
                Debug.LogError(
                    "TouchSenseSingleDevice: GetDeviceCapability (max envelope time) failed with error code " + rv);
            }
#endif
            return val;
        }
    }

    /// <summary>
    /// TouchSense Player API version number in hexadecimal format. The format is OxMMNNSPBB,
    /// where MM is the API major version number, NN is the minor version number, SP is the special
    /// build number (typically 0) and BB is the build number. For example, for the hexadecimal
    /// format 0x015001B the version number is 1.5.27.0.
    /// </summary>
    public int apiVersionNumber
    {
        get
        {
            int val = 0;
            //VIBE_DEVCAPTYPE_APIVERSIONNUMBER            12
#if UNITY_ANDROID
            int rv = ImmVibeGetDeviceCapabilityInt32(m_deviceIndex, 12, ref val);
            if (rv != 0)
            {
                Debug.LogError(
                    "TouchSenseSingleDevice: GetDeviceCapability (api version number) failed with error code " + rv);
            }
#endif
            return val;
        }
    }

    /// <summary>
    /// TouchSense Player edition level. One of 3000, 4000, 5000.
    /// </summary>
    public int editionLevel
    {
        get
        {
            int val = 0;
            //VIBE_DEVCAPTYPE_EDITION_LEVEL               15
#if UNITY_ANDROID
            int rv = ImmVibeGetDeviceCapabilityInt32(m_deviceIndex, 15, ref val);
            if (rv != 0)
            {
                Debug.LogError(
                    "TouchSenseSingleDevice: GetDeviceCapability (edition level) failed with error code " + rv);
            }
#endif
            return val;
        }
    }

    /// <summary>
    /// Priority with which to play effects. Different applications can use different priorities on the
    /// same device. The priority determines which application's effects are played when multiple
    /// applications are attempting to play effects at the same time. The default priority is
    /// <see cref="minPriority"/>. Priority values range from <see cref="minPriority"/> to
    /// <see cref="maxPriority"/>
    /// </summary>
    public int priority
    {
        get
        {
            int val = 0;
            //VIBE_DEVPROPTYPE_PRIORITY                   1
#if UNITY_ANDROID
            int rv = ImmVibeGetDevicePropertyInt32(deviceHandle, 1, ref val);
            if (rv != 0)
            {
                Debug.LogError(
                    "TouchSenseSingleDevice: GetDeviceProperty (priority) failed with error code " + rv);
            }
#endif
            return val;
        }
        set
        {
#if UNITY_ANDROID
            int rv = ImmVibeSetDevicePropertyInt32(deviceHandle, 1, value);
            if (rv != 0)
            {
                Debug.LogError(
                    "TouchSenseSingleDevice: SetDeviceProperty (priority) failed with error code " + rv);
            }
#endif
        }
    }

    /// <summary>
    /// Disables all effects on this device. When this property is set to true, any playing effects are
    /// immediately stopped and subsequent requests to play effects are ignored. When this property
    /// is set to false, subsequent requests to play effects are honored. The default value for this property
    /// is false.
    /// </summary>
    public bool disableEffects
    {
        get
        {
            int val = 0;
            //VIBE_DEVPROPTYPE_DISABLE_EFFECTS            2
#if UNITY_ANDROID
            int rv = ImmVibeGetDevicePropertyBool(deviceHandle, 2, ref val);
            if (rv != 0)
            {
                Debug.LogError(
                    "TouchSenseSingleDevice: GetDeviceProperty (disable effects) failed with error code " + rv);
            }
#endif
            return (val != 0);
        }
        set
        {
#if UNITY_ANDROID
            int rv = ImmVibeSetDevicePropertyBool(deviceHandle, 2, value?1:0);
            if (rv != 0)
            {
                Debug.LogError(
                    "TouchSenseSingleDevice: SetDeviceProperty (disable effects) failed with error code " + rv);
            }
#endif
        }
    }

    /// <summary>
    /// A property that reduces (or increases) the magnitude of effects. The Strength varies from 0 to 10,000,
    /// where 0 is silent (equivalent to mute), and 10,000 is full strength. The default value is 10,000.
    /// The Strength property only applies to this device instance, not to other device instances held by the
    /// same or a different application. Modifying the Strength does not affect currently playing effects,
    /// only effects played or modified after setting the value.
    /// </summary>
    public int strength
    {
        get
        {
            int val = 0;
            //VIBE_DEVPROPTYPE_STRENGTH                   3
#if UNITY_ANDROID
            int rv = ImmVibeGetDevicePropertyInt32(deviceHandle, 3, ref val);
            if (rv != 0)
            {
                Debug.LogError(
                    "TouchSenseSingleDevice: GetDeviceProperty (strength) failed with error code " + rv);
            }
#endif
            return val;
        }
        set
        {
#if UNITY_ANDROID
            int rv = ImmVibeSetDevicePropertyInt32(deviceHandle, 3, value);
            if (rv != 0)
            {
                Debug.LogError(
                    "TouchSenseSingleDevice: SetDeviceProperty (strength) failed with error code " + rv);
            }
#endif
        }
    }

    /// <summary>
    /// Initializes a device instance
    /// </summary>
    /// <param name="deviceIndex">The index of the device to initialize</param>
    public TouchSenseSingleDevice(int deviceIndex)
    {
        m_deviceIndex = deviceIndex;
    }

    protected override bool openDevice()
    {
        if (m_deviceHandle == -1)
        {
#if UNITY_ANDROID
            int rv = ImmVibeOpenDevice(m_deviceIndex, ref m_deviceHandle);
            if (rv != 0)
            {
                Debug.LogError("TouchSenseSingleDevice: OpenDevice failed with return code " + rv);
                return false;
            }
#endif
        }

        return true;
    }

#if UNITY_ANDROID
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibeOpenDevice(int nDeviceIndex, ref int phDeviceHandle);
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibeGetDeviceCapabilityInt32(int nDeviceIndex, int nDevCapType,
        ref int pnDevCapVal);
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibeGetDeviceCapabilityString(int nDeviceIndex, int nDevCapType,
        int nSize, StringBuilder pnDevCapVal);
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibeGetDevicePropertyInt32(int nDeviceIndex, int nDevPropType,
        ref int pnDevPropVal);
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibeSetDevicePropertyInt32(int nDeviceIndex, int nDevPropType,
        int nDevPropVal);
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibeGetDevicePropertyBool(int nDeviceIndex, int nDevPropType,
        ref int pnDevPropVal);
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibeSetDevicePropertyBool(int nDeviceIndex, int nDevPropType,
        int nDevPropVal);
#endif

}
