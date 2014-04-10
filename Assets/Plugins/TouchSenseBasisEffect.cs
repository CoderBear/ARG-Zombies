/*
** =========================================================================
** Copyright (c) 2012 Immersion Corporation.  All rights reserved.
**                    Immersion Corporation Confidential and Proprietary
**
** File:
**  TouchSenseBasisEffect.cs
**
** Description:
**  Unity3d C# code for defining a basic effect
** =========================================================================
*/

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

/// <summary>
/// Class representing the basis effect types Periodic and MagSweep.
///
/// MagSweep effects vibrate at a magnitude- and device-dependent rate.
/// Periodic effects allow the period or speed of vibration to be explicitly controlled
/// independent of magnitude. Periodic effects accurately produce vibration frequencies up to about
/// 30 Hz on actuators such as eccentric rotating mass (ERM) actuators that have relatively low
/// bandwidth, and frequencies in excess of 200 Hz on higher bandwidth actuators such as piezo
/// supported by the TouchSense Player 5000 Series.
/// </summary>
public class TouchSenseBasisEffect : TouchSenseEffect
{

    private int m_duration = 0;
    private int m_magnitude = 0;
    private int m_period = 0;
    private Style m_style = 0;
    private int m_attackTime = 0;
    private int m_attackLevel = 0;
    private int m_fadeTime = 0;
    private int m_fadeLevel = 0;

    /// <summary>
    /// MagSweep and Periodic effects can be played in several styles that influence how
    /// the effects feel.
    ///     - Strong effects feel strongest.
    ///     - Smooth effect strength is diminished to achieve higher frequencies for Periodic effects.
    ///     Same as Strong for MagSweep effects.
    ///     - Sharp achieves highest possible frequencies for Periodic effects without diminishing
    ///     strength. MagSweep effects stop more quickly.
    /// </summary>
    public enum Style
    {
        Sharp,
        Smooth,
        Strong
    }

    /// <summary>
    /// Duration of the effect in milliseconds. To specify an infinite duration, the effect duration
    /// should be set to -1. For a finite duration, the effect duration is clamped to a
    /// value from 0 to the value int <see cref="TouchSenseSingleDevice.maxEffectDuration"/>, inclusive
    /// </summary>
    public int duration
    {
        get { return m_duration; }
        set
        {
            m_duration = value;
            modifyPlaying();
        }
    }

    /// <summary>
    /// Magnitude of the effect. The effect magnitude is clamped to a value from
    /// 0 to 10000, inclusive.
    /// </summary>
    public int magnitude
    {
        get { return m_magnitude; }
        set
        {
            m_magnitude = value;
            if (m_magnitude < 0) m_magnitude = 0;
            if (m_magnitude > 10000) m_magnitude = 10000;
            modifyPlaying();
        }
    }

    /// <summary>
    /// Period of the effect in milliseconds or microseconds. If the most significant bit is 0,
    /// the period is in milliseconds. If the most significant bit is 1, the period is in microseconds.
    /// The effect period (in milliseconds) should go from the value of
    /// <see cref="TouchSenseSingleDevice.minPeriod"/> to the value of
    /// <see cref="TouchSenseSingleDevice.maxPeriod"/>, inclusive. A period of 0 means this is a MagSweep effect.
    /// </summary>
    public int period
    {
        get { return m_period; }
        set
        {
            if (m_period > 0 && value > 0)
            {
                m_period = value;
                modifyPlaying();
            }
        }
    }

    /// <summary>
    /// The Style of the effect. See <see cref="Style"/>
    /// </summary>
    public Style style
    {
        get { return m_style; }
        set
        {
            m_style = value;
            modifyPlaying();
        }
    }

    /// <summary>
    /// Attack time of the effect in milliseconds. The attack time is clamped to a value from 0 to the value
    /// of <see cref="TouchSenseSingleDevice.maxEnvelopeTime"/>,
    /// inclusive
    /// </summary>
    public int attackTime
    {
        get { return m_attackTime; }
        set
        {
            m_attackTime = value;
            modifyPlaying();
        }
    }

    /// <summary>
    /// Attack level of the effect. The attack level is clamped to a value from
    /// 0 to 10000, inclusive.
    /// </summary>
    public int attackLevel
    {
        get { return m_attackLevel; }
        set
        {
            m_attackLevel = value;
            if (m_attackLevel < 0) m_attackLevel = 0;
            if (m_attackLevel > 10000) m_attackLevel = 10000;
            modifyPlaying();
        }
    }

    /// <summary>
    /// Fade time of the effect in milliseconds. The fade time is clamped to a value from 0 to the value
    /// of <see cref="TouchSenseSingleDevice.maxEnvelopeTime"/>,
    /// inclusive
    /// </summary>
    public int fadeTime
    {
        get { return m_fadeTime; }
        set
        {
            m_fadeTime = value;
            modifyPlaying();
        }
    }

    /// <summary>
    /// Fade level of the effect. The fade level is clamped to a value from
    /// 0 to 10000, inclusive.
    /// </summary>
    public int fadeLevel
    {
        get { return m_fadeLevel; }
        set
        {
            m_fadeLevel = value;
            if (m_fadeLevel < 0) fadeLevel = 0;
            if (m_fadeLevel > 10000) fadeLevel = 10000;
            modifyPlaying();
        }
    }

    public override void play()
    {
        if (!TouchSense.instance.hapticsEnabled) return;
        if (playing) return;
        /*
        Debug.Log("Playing custom effect, dur: " + duration + ", mag: " + magnitude);
        Debug.Log("period: " + period + ", style: " + style + ", attackTime: " + attackTime +
            ", attackLevel: " + attackLevel + ", fadeTime: " + fadeTime + ", fadeLevel: " + fadeLevel);
         */

#if UNITY_ANDROID
        if (period > 0)
        {
            ImmVibePlayPeriodicEffect(m_device.deviceHandle, duration, magnitude, period,
                getIntStyle(), attackTime, attackLevel, fadeTime, fadeLevel,
                ref m_currentEffect);
        }
        else
        {
            ImmVibePlayMagSweepEffect(m_device.deviceHandle, duration, magnitude, getIntStyle(),
                attackTime, attackLevel, fadeTime, fadeLevel, ref m_currentEffect);
        }

        m_playing = true;
#endif
    }

    /// <summary>
    /// Create a new custom effect
    /// </summary>
    /// <param name="_duration">See <see cref="duration"/></param>
    /// <param name="_magnitude">See <see cref="magnitude"/></param>
    /// <param name="_period">See <see cref="period"/></param>
    /// <param name="_style">See <see cref="style"/></param>
    /// <param name="_attackTime">See <see cref="attackTime"/></param>
    /// <param name="_attackLevel">See <see cref="attackLevel"/></param>
    /// <param name="_fadeTime">See <see cref="fadeTime"/></param>
    /// <param name="_fadeLevel">See <see cref="fadeLevel"/></param>
    public TouchSenseBasisEffect(int _duration, int _magnitude, int _period, Style _style,
        int _attackTime, int _attackLevel, int _fadeTime, int _fadeLevel)
        : base(-1)
    {
        m_duration = _duration;
        m_magnitude = _magnitude;
        m_period = _period;
        m_style = _style;
        m_attackTime = _attackTime;
        m_attackLevel = _attackLevel;
        m_fadeTime = _fadeTime;
        m_fadeLevel = _fadeLevel;
    }

    private int getIntStyle()
    {
        switch (style)
        {
            default:
            case Style.Sharp:
                return 2;
            case Style.Smooth:
                return 0;
            case Style.Strong:
                return 1;
        }
    }

    private bool modifyPlaying()
    {
        if (!playing)
        {
            Debug.Log("TouchSenseBasisEffect: Trying to modify effect that is not playing");
            return false;
        }

#if UNITY_ANDROID
        if (m_period > 0)
        {
            int rv = ImmVibeModifyPlayingPeriodicEffect(m_device.deviceHandle, m_currentEffect, duration,
                magnitude, period, getIntStyle(), attackTime, attackLevel, fadeTime, fadeLevel);

            if (rv != 0)
            {
                Debug.LogError("TouchSenseBasisEffect: ModifyPlayingPeriodicEffect failed with error code " + rv);
                return false;
            }
        }
        else
        {
            int rv = ImmVibeModifyPlayingMagSweepEffect(m_device.deviceHandle, m_currentEffect, duration,
                magnitude, getIntStyle(), attackTime, attackLevel, fadeTime, fadeLevel);

            if (rv != 0)
            {
                Debug.LogError("TouchSenseBasisEffect: ModifyPlayingMagSweepEffect failed with error code " + rv);
                return false;
            }
        }
#endif

        return true;
    }

#if UNITY_ANDROID
    [DllImport("libImmEmulatorJ.so")]
    private static extern int ImmVibePlayMagSweepEffect(int hDeviceHandle, int nDuration, int nMagnitude,
        int nStyle, int nAttackTime, int nAttackLevel, int nFadeTime, int nFadeLevel, ref int phEffectHandle);
    [DllImport("libImmEmulatorJ.so")]
    private static extern int ImmVibePlayPeriodicEffect(int hDeviceHandle, int nDuration, int nMagnitude,
        int nPeriod, int nStyle, int nAttackTime, int nAttackLevel, int nFadeTime, int nFadeLevel,
        ref int phEffectHandle);
    [DllImport("libImmEmulatorJ.so")]
    private static extern int ImmVibeModifyPlayingMagSweepEffect(int hDeviceHandle, int hEffectHandle,
        int nDuration, int nMagnitude, int nStyle, int nAttackTime, int nAttackLevel, int nFadeTime,
        int nFadeLevel);
    [DllImport("libImmEmulatorJ.so")]
    private static extern int ImmVibeModifyPlayingPeriodicEffect(int hDeviceHandle, int hEffectHandle,
        int nDuration, int nMagnitude, int nPeriod, int nStyle, int nAttackTime, int nAttackLevel,
        int nFadeTime, int nFadeLevel);
#endif

}
