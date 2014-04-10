/*
** =========================================================================
** Copyright (c) 2012 Immersion Corporation.  All rights reserved.
**                    Immersion Corporation Confidential and Proprietary
**
** File:
**  TouchSense.cs
**
** Description:
**  Unity3d C# code for defining a waveform effect
** =========================================================================
*/

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Class encapsulating a Waveform effect
///
/// In a Waveform effect the actuator drive signal is defined by 8-bit or
/// 16-bit mono PCM data in the same format as the audio data portion of a WAV file. Waveform effects
/// may be used to mimic mechanical buttons from recorded data using high-bandwidth actuators such
/// as piezo.
/// </summary>
public class TouchSenseWaveformEffect : TouchSenseEffect
{
    private byte[] m_waveformBytes = null;
    private int m_samplingRate = 1024;
    private int m_magnitude = 10000;
    private int m_bitDepth = 8;

    public override void play()
    {
        if (!TouchSense.instance.hapticsEnabled) return;
        if (playing) return;

#if UNITY_ANDROID
        if (m_waveformBytes != null)
        {
            //Debug.LogError("TouchSense: Playing IVT effect " + effectId);
            int rv = ImmVibePlayWaveformEffect(m_device.deviceHandle, m_waveformBytes, m_waveformBytes.Length,
                m_samplingRate, m_bitDepth, m_magnitude, ref m_currentEffect);
            if (rv != 0)
            {
                Debug.LogError("TouchSenseIVTEffect: PlayWaveformEffect failed with return value " + rv);
                return;
            }
        }
        else
#endif
        {
            Debug.LogError("TouchSense: Trying to play waveform effect when no data loaded!");
            return;
        }

        m_playing = true;
    }

    /// <summary>
    /// Sampling rate of PCM data in Hertz (number of samples per second).
    /// </summary>
    public int samplingRate
    {
        get
        {
            return m_samplingRate;
        }
        set
        {
            m_samplingRate = value;
        }
    }

    /// <summary>
    /// Magnitude of the effect. The effect magnitude is clamped to a value from 0
    /// to 10000, inclusive. When set to 10000, the PCM data samples are
    /// not attenuated; for example, an 8-bit sample having a value of 192 will output a drive signal
    /// that is approximately 50% of maximum (192 - 128) / 127 = 50%. Magnitude values less than
    /// 10000 serve to attenuate the PCM data samples.
    /// </summary>
    public int magnitude
    {
        get
        {
            return m_magnitude;
        }
        set
        {
            m_magnitude = value;
        }
    }

    /// <summary>
    /// Bit depth of PCM data, or number of bits per sample. The only supported values are 8 or 16;
    /// that is, the TouchSense Player 5000 Series supports only 8-bit or 16-bit PCM data.
    /// </summary>
    public int bitDepth
    {
        get
        {
            return m_bitDepth;
        }
        set
        {
            m_bitDepth = value;
        }
    }

    /// <summary>
    /// The PCM data defining the actuator drive signal for the Waveform effect.
    /// The data is formatted in the same way as the PCM data in a WAV file with the exception
    /// that only 8-bit or 16-bit mono (not stereo) samples are supported.
    /// </summary>
    public byte[] data
    {
        get
        {
            return m_waveformBytes;
        }
        set
        {
            m_waveformBytes = value;
        }
    }

    /// <summary>
    /// Creates a new waveform effect
    /// </summary>
    /// <param name="waveformBytes">The raw waveform data</param>
    public TouchSenseWaveformEffect(byte[] waveformBytes, int _samplingRate, int _bitDepth, int _magnitude)
        : base(-1)
    {
        m_waveformBytes = waveformBytes;
        m_samplingRate = _samplingRate;
        m_bitDepth = _bitDepth;
        m_magnitude = _magnitude;
    }

    protected override void acquireType()
    {
        m_type = EffectTypes.Waveform;
    }

#if UNITY_ANDROID
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibePlayWaveformEffect(int hDeviceHandle, byte[] pData, int nDataSize,
        int nSamplingRate, int nBitDepth, int nMagnitude, ref int phEffectHandle);
#endif

}
