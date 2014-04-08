/*
** =========================================================================
** Copyright (c) 2012 Immersion Corporation.  All rights reserved.
**                    Immersion Corporation Confidential and Proprietary
**
** File:
**  TouchSense.cs
**
** Description:
**  Unity3d C# code for accessing CUHL functionalities
** =========================================================================
*/

using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

/// <summary>
/// Singleton class containing convenience methods and properties for easy TouchSense usage.
/// </summary>
public class TouchSense : MonoBehaviour{

    private static TouchSense m_instance = null;

    private bool m_enabled = true;
    private TouchSenseEffectBundle m_defaultBundle = null;
    private TouchSenseDevice m_defaultDevice = null;
    private int m_currentEffect = -1;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a sharp click effect at full strength.
    /// </summary>
    public const int SHARP_CLICK_100                   = 0;

    /// <summary>
     /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a sharp click effect at medium strength.
    /// </summary>
    public const int SHARP_CLICK_66                    = 1;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a sharp click effect at low strength.
    /// </summary>
    public const int SHARP_CLICK_33                    = 2;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a strong click effect at full strength.
    /// </summary>
    public const int STRONG_CLICK_100                  = 3;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a strong click effect at medium strength.
    /// </summary>
    public const int STRONG_CLICK_66                   = 4;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a strong click effect at low strength.
    /// </summary>
    public const int STRONG_CLICK_33                   = 5;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a bump effect at full strength.
    /// </summary>
    public const int BUMP_100                          = 6;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a bump effect at medium strength.
    /// </summary>
    public const int BUMP_66                           = 7;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a bump effect at low strength.
    /// </summary>
    public const int BUMP_33                           = 8;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a bounce effect at full strength.
    /// </summary>
    public const int BOUNCE_100                        = 9;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a bounce effect at medium strength.
    /// </summary>
    public const int BOUNCE_66                         = 10;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a bounce effect at low strength.
    /// </summary>
    public const int BOUNCE_33                         = 11;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a double sharp click effect at full strength.
    /// </summary>
    public const int DOUBLE_SHARP_CLICK_100            = 12;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a double sharp click effect at medium strength.
    /// </summary>
    public const int DOUBLE_SHARP_CLICK_66             = 13;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a double sharp click effect at low strength.
    /// </summary>
    public const int DOUBLE_SHARP_CLICK_33             = 14;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a double strong click effect at full strength.
    /// </summary>
    public const int DOUBLE_STRONG_CLICK_100           = 15;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a double strong click effect at medium strength.
    /// </summary>
    public const int DOUBLE_STRONG_CLICK_66            = 16;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a double strong click effect at low strength.
    /// </summary>
    public const int DOUBLE_STRONG_CLICK_33            = 17;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a double bump effect at full strength.
    /// </summary>
    public const int DOUBLE_BUMP_100                   = 18;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a double bump effect at medium strength.
    /// </summary>
    public const int DOUBLE_BUMP_66                    = 19;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a double bump effect at low strength.
    /// </summary>
    public const int DOUBLE_BUMP_33                    = 20;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a triple strong click effect at full strength.
    /// </summary>
    public const int TRIPLE_STRONG_CLICK_100           = 21;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a triple strong click effect at medium strength.
    /// </summary>
    public const int TRIPLE_STRONG_CLICK_66            = 22;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a triple strong click effect at low strength.
    /// </summary>
    public const int TRIPLE_STRONG_CLICK_33            = 23;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a tick effect at full strength.
    /// </summary>
    public const int TICK_100                          = 24;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a tick effect at medium strength.
    /// </summary>
    public const int TICK_66                           = 25;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a tick effect at low strength.
    /// </summary>
    public const int TICK_33                           = 26;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a long buzz effect at full strength.
    /// </summary>
    public const int LONG_BUZZ_100                     = 27;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a long buzz effect at medium strength.
    /// </summary>
    public const int LONG_BUZZ_66                      = 28;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a long buzz effect at low strength.
    /// </summary>
    public const int LONG_BUZZ_33                      = 29;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a short buzz effect at full strength.
    /// </summary>
    public const int SHORT_BUZZ_100                    = 30;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a short buzz effect at medium strength.
    /// </summary>
    public const int SHORT_BUZZ_66                     = 31;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a short buzz effect at low strength.
    /// </summary>
    public const int SHORT_BUZZ_33                     = 32;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a long transition ramp up effect at full strength.
    /// </summary>
    public const int LONG_TRANSITION_RAMP_UP_100       = 33;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a long transition ramp up effect at medium strength.
    /// </summary>
    public const int LONG_TRANSITION_RAMP_UP_66        = 34;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a long transition ramp up effect at low strength.
    /// </summary>
    public const int LONG_TRANSITION_RAMP_UP_33        = 35;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a short transition ramp up effect at full strength.
    /// </summary>
    public const int SHORT_TRANSITION_RAMP_UP_100      = 36;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a short transition ramp up effect at medium strength.
    /// </summary>
    public const int SHORT_TRANSITION_RAMP_UP_66       = 37;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a short transition ramp up effect at low strength.
    /// </summary>
    public const int SHORT_TRANSITION_RAMP_UP_33       = 38;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a long transition ramp down effect at full strength.
    /// </summary>
    public const int LONG_TRANSITION_RAMP_DOWN_100     = 39;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a long transition ramp down effect at medium strength.
    /// </summary>
    public const int LONG_TRANSITION_RAMP_DOWN_66      = 40;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a long transition ramp down effect at low strength.
    /// </summary>
    public const int LONG_TRANSITION_RAMP_DOWN_33      = 41;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a short transition ramp down effect at full strength.
    /// </summary>
    public const int SHORT_TRANSITION_RAMP_DOWN_100    = 42;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a short transition ramp down effect at medium strength.
    /// </summary>
    public const int SHORT_TRANSITION_RAMP_DOWN_66     = 43;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a short transition ramp down effect at low strength.
    /// </summary>
    public const int SHORT_TRANSITION_RAMP_DOWN_33     = 44;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a fast pulse effect at full strength.
    /// </summary>
    public const int FAST_PULSE_100                    = 45;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a fast pulse effect at medium strength.
    /// </summary>
    public const int FAST_PULSE_66                     = 46;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a fast pulse effect at low strength.
    /// </summary>
    public const int FAST_PULSE_33                     = 47;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a fast pulsing effect at full strength.
    /// </summary>
    public const int FAST_PULSING_100                  = 48;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a fast pulsing effect at medium strength.
    /// </summary>
    public const int FAST_PULSING_66                   = 49;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a fast pulsing effect at low strength.
    /// </summary>
    public const int FAST_PULSING_33                   = 50;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a slow pulse effect at full strength.
    /// </summary>
    public const int SLOW_PULSE_100                    = 51;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a slow pulse effect at medium strength.
    /// </summary>
    public const int SLOW_PULSE_66                     = 52;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a slow pulse effect at low strength.
    /// </summary>
    public const int SLOW_PULSE_33                     = 53;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a slow pulsing effect at full strength.
    /// </summary>
    public const int SLOW_PULSING_100                  = 54;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a slow pulsing effect at medium strength.
    /// </summary>
    public const int SLOW_PULSING_66                   = 55;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a slow pulsing effect at low strength.
    /// </summary>
    public const int SLOW_PULSING_33                   = 56;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a transition bump effect at full strength.
    /// </summary>
    public const int TRANSITION_BUMP_100               = 57;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a transition bump effect at medium strength.
    /// </summary>
    public const int TRANSITION_BUMP_66                = 58;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a transition bump effect at low strength.
    /// </summary>
    public const int TRANSITION_BUMP_33                = 59;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a transition bounce effect at full strength.
    /// </summary>
    public const int TRANSITION_BOUNCE_100             = 60;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a transition bounce effect at medium strength.
    /// </summary>
    public const int TRANSITION_BOUNCE_66              = 61;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a transition bounce effect at low strength.
    /// </summary>
    public const int TRANSITION_BOUNCE_33              = 62;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an alert effect of type 1.
    /// </summary>
    public const int ALERT1                            = 63;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an alert effect of type 2.
    /// </summary>
    public const int ALERT2                            = 64;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an alert effect of type 3.
    /// </summary>
    public const int ALERT3                            = 65;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an alert effect of type 4.
    /// </summary>
    public const int ALERT4                            = 66;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an alert effect of type 5.
    /// </summary>
    public const int ALERT5                            = 67;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an alert effect of type 6.
    /// </summary>
    public const int ALERT6                            = 68;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an alert effect of type 7.
    /// </summary>
    public const int ALERT7                            = 69;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an alert effect of type 8.
    /// </summary>
    public const int ALERT8                            = 70;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an alert effect of type 9.
    /// </summary>
    public const int ALERT9                            = 71;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an alert effect of type 10.
    /// </summary>
    public const int ALERT10                           = 72;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an explosion effect of type 1.
    /// </summary>
    public const int EXPLOSION1                        = 73;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an explosion effect of type 2.
    /// </summary>
    public const int EXPLOSION2                        = 74;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an explosion effect of type 3.
    /// </summary>
    public const int EXPLOSION3                        = 75;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an explosion effect of type 4.
    /// </summary>
    public const int EXPLOSION4                        = 76;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an explosion effect of type 5.
    /// </summary>
    public const int EXPLOSION5                        = 77;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an explosion effect of type 6.
    /// </summary>
    public const int EXPLOSION6                        = 78;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an explosion effect of type 7.
    /// </summary>
    public const int EXPLOSION7                        = 79;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an explosion effect of type 8.
    /// </summary>
    public const int EXPLOSION8                        = 80;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an explosion effect of type 9.
    /// </summary>
    public const int EXPLOSION9                        = 81;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an explosion effect of type 10.
    /// </summary>
    public const int EXPLOSION10                       = 82;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a weapon effect of type 1.
    /// </summary>
    public const int WEAPON1                           = 83;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a weapon effect of type 2.
    /// </summary>
    public const int WEAPON2                           = 84;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a weapon effect of type 3.
    /// </summary>
    public const int WEAPON3                           = 85;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a weapon effect of type 4.
    /// </summary>
    public const int WEAPON4                           = 86;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a weapon effect of type 5.
    /// </summary>
    public const int WEAPON5                           = 87;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a weapon effect of type 6.
    /// </summary>
    public const int WEAPON6                           = 88;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a weapon effect of type 7.
    /// </summary>
    public const int WEAPON7                           = 89;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a weapon effect of type 8.
    /// </summary>
    public const int WEAPON8                           = 90;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a weapon effect of type 9.
    /// </summary>
    public const int WEAPON9                           = 91;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a weapon effect of type 10.
    /// </summary>
    public const int WEAPON10                          = 92;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a wood impact effect at full strength.
    /// </summary>
    public const int IMPACT_WOOD_100                   = 93;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a wood impact effect at medium strength.
    /// </summary>
    public const int IMPACT_WOOD_66                    = 94;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a wood impact effect at low strength.
    /// </summary>
    public const int IMPACT_WOOD_33                    = 95;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a metal impact effect at full strength.
    /// </summary>
    public const int IMPACT_METAL_100                  = 96;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a metal impact effect at medium strength.
    /// </summary>
    public const int IMPACT_METAL_66                   = 97;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a metal impact effect at low strength.
    /// </summary>
    public const int IMPACT_METAL_33                   = 98;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a rubber impact effect at high strength.
    /// </summary>
    public const int IMPACT_RUBBER_100                 = 99;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a rubber impact effect at medium strength.
    /// </summary>
    public const int IMPACT_RUBBER_66                  = 100;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a rubber impact effect at low strength.
    /// </summary>
    public const int IMPACT_RUBBER_33                  = 101;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a texture effect of type 1.
    /// </summary>
    public const int TEXTURE1                          = 102;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a texture effect of type 2.
    /// </summary>
    public const int TEXTURE2                          = 103;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a texture effect of type 3.
    /// </summary>
    public const int TEXTURE3                          = 104;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a texture effect of type 4.
    /// </summary>
    public const int TEXTURE4                          = 105;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a texture effect of type 5.
    /// </summary>
    public const int TEXTURE5                          = 106;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a texture effect of type 6.
    /// </summary>
    public const int TEXTURE6                          = 107;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a texture effect of type 7.
    /// </summary>
    public const int TEXTURE7                          = 108;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a texture effect of type 8.
    /// </summary>
    public const int TEXTURE8                          = 109;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a texture effect of type 9.
    /// </summary>
    public const int TEXTURE9                          = 110;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play a texture effect of type 10.
    /// </summary>
    public const int TEXTURE10                         = 111;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an engine effect of type 1, at high strength.
    /// </summary>
    public const int ENGINE1_100                       = 112;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an engine effect of type 1, at medium strength.
    /// </summary>
    public const int ENGINE1_66                        = 113;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an engine effect of type 1, at low strength.
    /// </summary>
    public const int ENGINE1_33                        = 114;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an engine effect of type 2, at high strength.
    /// </summary>
    public const int ENGINE2_100                       = 115;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an engine effect of type 2, at medium strength.
    /// </summary>
    public const int ENGINE2_66                        = 116;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an engine effect of type 2, at low strength.
    /// </summary>
    public const int ENGINE2_33                        = 117;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an engine effect of type 3, at high strength.
    /// </summary>
    public const int ENGINE3_100                       = 118;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an engine effect of type 3, at medium strength.
    /// </summary>
    public const int ENGINE3_66                        = 119;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an engine effect of type 3, at low strength.
    /// </summary>
    public const int ENGINE3_33                        = 120;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an engine effect of type 4, at high strength.
    /// </summary>
    public const int ENGINE4_100                       = 121;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an engine effect of type 4, at medium strength.
    /// </summary>
    public const int ENGINE4_66                        = 122;

    /// <summary>
    /// Pass this constant to <see cref="playBuiltinEffect()"/> to play an engine effect of type 4, at low strength.
    /// </summary>
    public const int ENGINE4_33                        = 123;

    /// <summary>
    /// The global instance that can be used in simple fashion by calling TouchSense.instance.function()
    /// </summary>
    public static TouchSense instance
    {
        get
        {
            if(m_instance == null)
            {
                //Save the object reference to the scene to make sure we don't need to keep re-creating it
                GameObject container = new GameObject();
                container.name = "TouchSenseContainer";
                m_instance = container.AddComponent(typeof(TouchSense)) as TouchSense;

                //Set initial state based on prefs setting. If the setting doesn't exist set enabled.
                if(PlayerPrefs.GetInt("Haptics", 1) != 1)
                    m_instance.hapticsEnabled = false;
            }

            return m_instance;
        }
    }

    /// <summary>
    /// Global enabled flag to conveniently turn haptics on/off
    /// </summary>
    public bool hapticsEnabled
    {
        get
        {
            return m_enabled;
        }
        set
        {
            m_enabled = value;
        }
    }

    /// <summary>
    /// The default effect bundle. The ivt data file must be stored as Assets/Resources/haptics_data.bytes
    /// </summary>
    public TouchSenseEffectBundle defaultBundle
    {
        get
        {
            if (m_defaultBundle == null)
            {
                //The default IVT file must be named haptics_data.bytes due to Unity funny business
                m_defaultBundle = new TouchSenseEffectBundle("haptics_data");
            }

            return m_defaultBundle;
        }
    }

    /// <summary>
    /// The global default device used if a specific device is not set for an effect and with the built-in
    /// effects
    /// </summary>
    public TouchSenseDevice defaultDevice
    {
        get
        {
            if(m_defaultDevice == null)
            {
                m_defaultDevice = new TouchSenseCompositeDevice(TouchSenseDevice.count);
            }
            return m_defaultDevice;
        }
        set
        {
            m_defaultDevice = value;
        }
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public TouchSense()
    {
        Debug.Log("TouchSense: Constructor");
#if UNITY_ANDROID
        int rv = ImmVibeInitialize(0);
        if (rv < 0)
        {
            Debug.LogError("TouchSense: Initialization failed: " + rv);
            return;
        }
#endif
    }

    /// <summary>
    /// Plays an effect from the built-in set
    /// </summary>
    /// <param name="effectId">The effect id to play</param>
    public void playBuiltinEffect(int effectId)
    {
        if(!hapticsEnabled)
            return;

        if(effectId < 0) {
            Debug.LogError("TouchSense: Invalid built-in effect id " + effectId);
            return;
        }

#if UNITY_ANDROID
        int rv = ImmVibePlayUHLEffect(defaultDevice.deviceHandle, effectId, ref m_currentEffect);
        if (rv != 0)
        {
            Debug.LogError("TouchSense: ImmVibePlayUHLEffect returned " + rv);
        }
#endif
    }

    /// <summary>
    /// Stops the built-in effect that is currently playing if any.
    /// </summary>
    public void stopPlayingBuiltinEffect()
    {
        if (m_currentEffect != -1 && defaultDevice.deviceHandle != -1)
        {
#if UNITY_ANDROID
            ImmVibeStopPlayingEffect(defaultDevice.deviceHandle, m_currentEffect);
#endif
        }
        m_currentEffect = -1;
    }

    /// <summary>
    /// Terminates the C-UHL API upon the application closing
    /// </summary>
    private void OnDestroy()
    {
#if UNITY_ANDROID
        ImmVibeTerminate();
#endif
    }

    /// <summary>
    /// Makes sure that Unity adds the android VIBRATE permission
    ///     to the auto-generated AndroidManifest.xml
    /// This function should never be called.
    /// </summary>
    private void enablePermission()
    {
        Handheld.Vibrate();
    }

#if UNITY_ANDROID
    [DllImport("libImmEmulatorJ.so")]
    private static extern int ImmVibePlayUHLEffect(int hDeviceHandle, int nEffectIndex, ref int phEffectHandle);
    [DllImport("libImmEmulatorJ.so")]
    private static extern int ImmVibeStopPlayingEffect(int hDeviceHandle, int hEffectHandle);
    [DllImport("libImmEmulatorJ.so")]
    private static extern int ImmVibeTerminate();
    [DllImport("libImmEmulatorJ.so")]
    private static extern int ImmVibeInitialize(int nVersionNumber);
#endif

}
