/*
** =========================================================================
** Copyright (c) 2012 Immersion Corporation.  All rights reserved.
**                    Immersion Corporation Confidential and Proprietary
**
** File:
**  TouchSenseEffect.cs
**
** Description:
**  Unity3d C# code for defining an effect definition
** =========================================================================
*/

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

/// <summary>
/// The base class of TouchSense haptic effects.
///
/// There are five types of effects: three simple types (MagSweep effects, Periodic effects,
/// and Waveform effects), as well as Timeline effects and Streaming effects.
///
/// MagSweep effects vibrate at a magnitude- and device-dependent rate.
/// Periodic effects allow the period or speed of vibration to be explicitly controlled
/// independent of magnitude. Periodic effects accurately produce vibration frequencies up to about
/// 30 Hz on actuators such as eccentric rotating mass (ERM) actuators that have relatively low
/// bandwidth, and frequencies in excess of 200 Hz on higher bandwidth actuators such as piezo
/// supported by the TouchSense Player 5000 Series.
///
/// Waveform effects define the
/// actuator drive signal using 8-bit or 16-bit mono PCM data in the same format as the audio data
/// portion of a WAV file. Waveform effects may be used to mimic mechanical buttons from recorded
/// data using high-bandwidth actuators such as piezo. Waveform effects can be played only in the
/// TouchSense Player 5000 Series.
///
/// Timeline effects consist of simple effects scheduled in time. Timeline effects are normally
/// created using Haptic Studio, an interactive authoring tool for the Windows environment specifically
/// designed to facilitate the process of creating advanced TouchSense effects. Once the effects are
/// created using Studio, the effects can be exported to an IVT file, which can be used by the API.
///
/// Streaming effects are not currently supported by this API
/// </summary>
public class TouchSenseEffect
{
    protected int m_currentEffect = -1;
    protected bool m_playing = false;
    protected bool m_paused = false;
    protected EffectTypes m_type = EffectTypes.Unknown;

    protected TouchSenseDevice m_device = null;

    /// <summary>
    /// There are five types of effects: three simple types (MagSweep effects, Periodic effects,
    /// and Waveform effects), as well as Timeline effects and Streaming effects.
    /// </summary>
    public enum EffectTypes
    {
        Periodic,
        MagSweep,
        Timeline,
        Streaming,
        Waveform,
        Unknown
    };

    /// <summary>
    /// Indicates whether the effect is currently playing.
    /// </summary>
    public bool playing
    {
        get
        {
            if (!m_playing) return false;
            int state = 0;
#if UNITY_ANDROID
            ImmVibeGetEffectState(m_device.deviceHandle, m_currentEffect, ref state);
#endif

            switch(state)
            {
                case 1:
                    //Playing;
                    m_playing = true;
                    m_paused = false;
                    break;
                case 2:
                    //paused
                    m_playing = true;
                    m_paused = true;
                    break;
                default:
                    //stopped
                    m_playing = false;
                    m_paused = false;
                    break;
            }
            return m_playing;
        }
    }

    /// <summary>
    /// Indicates whether the effect is currently paused.
    /// </summary>
    public bool paused
    {
        get { return m_paused; }
    }

    /// <summary>
    /// The haptics device associated with this effect
    /// </summary>
    public TouchSenseDevice device
    {
        get { return m_device; }
        set
        {
            m_device = value;
        }
    }

    /// <summary>
    /// The type of this effect <see cref="EffectTypes"/>
    /// </summary>
    public EffectTypes type
    {
        get
        {
            if (m_type == EffectTypes.Unknown) acquireType();
            return m_type;
        }
    }

    protected virtual void acquireType() {}

    /// <summary>
    /// Starts playback of this effect
    /// </summary>
    public virtual void play() {}

    /// <summary>
    /// Stops the playback of this effect
    /// </summary>
    public void stop()
    {
        if (m_currentEffect != -1)
        {
#if UNITY_ANDROID
            int rv = ImmVibeStopPlayingEffect(m_device.deviceHandle, m_currentEffect);
            //Silently ignore VIBE_W_NOT_PLAYING
            if (rv < 0) Debug.LogError("TouchSenseEffect: Stop failed with error code " + rv);
#endif
        }

        m_paused = false;
        m_playing = false;
        m_currentEffect = -1;
    }

    /// <summary>
    /// Pauses the playback of this effect
    /// </summary>
    public void pause()
    {
        if (!paused && playing)
        {
            int rv = 0;
#if UNITY_ANDROID
            rv = ImmVibePausePlayingEffect(m_device.deviceHandle, m_currentEffect);
#endif
            if (rv != 0)
            {
                //VIBE_W_NOT_PLAYING
                if (rv == 1)
                {
                    stop();
                    return;
                }
                Debug.LogError("TouchSenseEffect: Pause failed with error code " + rv);
            }
            else
            {
                m_paused = true;
            }
        }
    }

    /// <summary>
    /// Resumes the playback of this effect if previously paused
    /// </summary>
    public void resume()
    {
        if (paused)
        {
            m_paused = false;
            int rv = 0;
#if UNITY_ANDROID
            rv = ImmVibeResumePausedEffect(m_device.deviceHandle, m_currentEffect);
#endif
            //VIBE_S_SUCCESS & VIBE_W_NOT_PAUSED
            if (rv == 0 || rv == 4) return;
            Debug.LogError("TouchSenseEffect: Resume failed with error code " + rv);
        }
    }

    protected internal TouchSenseEffect(int effectHandle)
    {
        m_currentEffect = effectHandle;
        if (effectHandle > 0) m_playing = true;
        m_device = TouchSense.instance.defaultDevice;
    }

#if UNITY_ANDROID
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibeStopPlayingEffect(int hDeviceHandle, int hEffectHandle);
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibeGetEffectState(int hDeviceHandle, int hEffectHandle, ref int pnEffectState);
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibePausePlayingEffect(int hDeviceHandle, int hEffectHandle);
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibeResumePausedEffect(int hDeviceHandle, int hEffectHandle);

#endif

}
