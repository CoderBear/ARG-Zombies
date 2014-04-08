/*
** =========================================================================
** Copyright (c) 2012 Immersion Corporation.  All rights reserved.
**                    Immersion Corporation Confidential and Proprietary
**
** File:
**  TouchSenseIVTEffect.cs
**
** Description:
**  Unity3d C# code for representing an IVT effect
** =========================================================================
*/

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Class representing an effect stored in <see cref="TouchSenseEffectBundle"/>
/// </summary>
public class TouchSenseIVTEffect : TouchSenseEffect
{
    private byte[] m_ivtBytes;
    private int m_effectIndex;

    private string m_name = null;

    /// <summary>
    /// The name of this effect defined in the ivt data
    /// </summary>
    public string name
    {
        get
        {
            if (m_name == null)
            {
                StringBuilder sb = new StringBuilder(512);
#if UNITY_ANDROID
                int rv = ImmVibeGetIVTEffectName(m_ivtBytes, m_effectIndex, 512, sb);
                if (rv != 0)
                {
                    Debug.LogError("TouchSenseIVTEffect: GetIVTEffectName failed with error code " + rv);
                }
#endif
                m_name = sb.ToString();
            }

            return m_name;
        }
    }

    /// <summary>
    /// The index of the effect in the bundle
    /// </summary>
    public int index
    {
        get
        {
            return m_effectIndex;
        }
    }

    public override void play()
    {
        if (!TouchSense.instance.hapticsEnabled) return;
        if (playing) return;

        if (m_effectIndex < 0)
        {
            Debug.LogError("TouchSenseIVTEffect: Invalid effect id " + m_effectIndex);
            return;
        }

#if UNITY_ANDROID
        if (m_ivtBytes != null)
        {
            //Debug.LogError("TouchSense: Playing IVT effect " + effectId);
            int rv = ImmVibePlayIVTEffect(m_device.deviceHandle, m_ivtBytes, m_effectIndex, ref m_currentEffect);
            if (rv != 0)
            {
                Debug.LogError("TouchSenseIVTEffect: PlayIVTEffect failed with return value " + rv + 
                    ", effect index " + m_effectIndex);
                return;
            }
        }
        else
#endif
        {
            Debug.LogError("TouchSense: Trying to play effect (" + m_effectIndex + ") when no ivt loaded!");
            return;
        }

        m_playing = true;
    }

    /// <summary>
    /// Repeatedly plays a Timeline effect defined in IVT data.
    /// </summary>
    /// <param name="repeat">Number of times to repeat the effect. To play the effect indefinitely, 
    /// set this to -1. Setting this to 0 plays the effect once (repeats the effect zero times) and is 
    /// equivalent to calling <see cref="play()"/>. To stop the effect before it has repeated the requested 
    /// number of times or to stop an effect that is playing indefinitely, call <see cref="stop()"/> or 
    /// <see cref="TouchSenseDevice.stopAllEffects()"/>.</param>
    public void playRepeat(int repeat)
    {
        if (!TouchSense.instance.hapticsEnabled) return;
        if (playing) return;

        if (m_effectIndex < 0)
        {
            Debug.LogError("TouchSense: Invalid effect id " + m_effectIndex);
            return;
        }

#if UNITY_ANDROID
        if (m_ivtBytes != null)
        {
            //Debug.LogError("TouchSense: Playing IVT effect " + effectId);
            int rv = ImmVibePlayIVTEffectRepeat(m_device.deviceHandle, m_ivtBytes, m_effectIndex, repeat, ref m_currentEffect);
            if (rv != 0)
            {
                Debug.LogError("TouchSenseIVTEffect: PlayIVTEffectRepeat failed with return value " + rv +
                    ", effect index " + m_effectIndex);
                return;
            }
        }
        else
#endif
        {
            Debug.LogError("TouchSense: Trying to play effect (" + m_effectIndex + ") when no ivt loaded!");
            return;
        }

        m_playing = true;
    }

    /// <summary>
    /// Get the effect definition
    /// </summary>
    /// <returns>The effect definition object. Depending on the type of effect represented by this object
    /// the return value can be <see cref="TouchSenseBasisEffect"/> or <see cref="TouchSenseWaveformEffect"/>
    /// </returns>
    public TouchSenseEffect getEffectDefinition()
    {
        int duration = 0;
        int magnitude = 0;
        int period = 0;
        int style = 0;
        int attackTime = 0;
        int attackLevel = 0;
        int fadeTime = 0;
        int fadeLevel = 0;

        int rv = 0;

        switch (type)
        {
#if UNITY_ANDROID
            case EffectTypes.Periodic:
                //periodic
                rv = ImmVibeGetIVTPeriodicEffectDefinition(m_ivtBytes, m_effectIndex, ref duration, ref magnitude,
                    ref period, ref style, ref attackTime, ref attackLevel, ref fadeTime, ref fadeLevel);
                if (rv != 0)
                {
                    Debug.LogError("TouchSenseIVTEffect: GEtIVTPeriodicEffectDefinition failed with return value " + rv +
                        ", effect index " + m_effectIndex);
                    return null;
                }

                return new TouchSenseBasisEffect(duration, magnitude, period, getStyleFromInt(style), 
                    attackTime, attackLevel, fadeTime, fadeLevel);
            case EffectTypes.MagSweep:
                //magsweep
                rv = ImmVibeGetIVTMagSweepEffectDefinition(m_ivtBytes, m_effectIndex, ref duration, ref magnitude,
                    ref style, ref attackTime, ref attackLevel, ref fadeTime, ref fadeLevel);
                if (rv != 0)
                {
                    Debug.LogError("TouchSenseIVTEffect: GEtIVTMagSweepEffectDefinition failed with return value " + rv +
                        ", effect index " + m_effectIndex);
                    return null;
                }

                return new TouchSenseBasisEffect(duration, magnitude, 0, getStyleFromInt(style), 
                    attackTime, attackLevel, fadeTime, fadeLevel);
#endif
            default:
                return null;
        }
    }

    internal TouchSenseIVTEffect(byte[] ivtBytes, int index)
        : base(-1)
    {
        m_ivtBytes = ivtBytes;
        m_effectIndex = index;
    }

    protected override void acquireType() 
    {
        int intType = 0;
#if UNITY_ANDROID
        int rv = ImmVibeGetIVTEffectType(m_ivtBytes, m_effectIndex, ref intType);
        if (rv != 0)
        {
            Debug.LogError("TouchSenseIVTEffect: GetIVTEffectType failed with error code " + rv);
            m_type = EffectTypes.Unknown;
            return;
        }
#endif
        switch (intType)
        {
            case 0:
                m_type = EffectTypes.Periodic;
                break;
            case 1:
                m_type = EffectTypes.MagSweep;
                break;
            case 2:
                m_type = EffectTypes.Timeline;
                break;
            case 3:
                m_type = EffectTypes.Streaming;
                break;
            case 4:
                m_type = EffectTypes.Waveform;
                break;
            default:
                m_type = EffectTypes.Unknown;
                break;
        }
    }
    private TouchSenseBasisEffect.Style getStyleFromInt(int st)
    {
        switch (st)
        {
            default:
            case 2:
                return TouchSenseBasisEffect.Style.Sharp;
            case 0:
                return TouchSenseBasisEffect.Style.Smooth;
            case 1:
                return TouchSenseBasisEffect.Style.Strong;
        }
    }

#if UNITY_ANDROID
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibePlayIVTEffect(int hDeviceHandle, byte[] pIVT, int nEffectIndex,
        ref int phEffectHandle);
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibePlayIVTEffectRepeat(int hDeviceHandle, byte[] pIVT, int nEffectIndex,
        int nRepeat, ref int phEffectHandle);
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibeGetIVTEffectType(byte[] pIVT, int nEffectIndex, ref int pnEffectType);
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibeGetIVTMagSweepEffectDefinition(byte[] pIVT, int nEffectIndex,
        ref int pnDuration, ref int pnMagnitude, ref int pnStyle, ref int pnAttackTime, ref int pnAttackLevel,
        ref int pnFadeTime, ref int pnFadeLevel);
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibeGetIVTPeriodicEffectDefinition(byte[] pIVT, int nEffectIndex,
        ref int pnDuration, ref int pnMagnitude, ref int pnPeriod, ref int pnStyle, ref int pnAttackTime,
        ref int pnAttackLevel, ref int pnFadeTime, ref int pnFadeLevel);
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibeGetIVTEffectName(byte[] pIVT, int nEffectIndex, int nSize, 
        StringBuilder szEffectName);
#endif

}
