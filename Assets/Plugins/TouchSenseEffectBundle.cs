/*
** =========================================================================
** Copyright (c) 2012 Immersion Corporation.  All rights reserved.
**                    Immersion Corporation Confidential and Proprietary
**
** File:
**  TouchSenseEffectBundle.cs
**
** Description:
**  Unity3d C# code for a predefined effect
** =========================================================================
*/

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

/// <summary>
/// Class representing a predefined set of effects stored in a file. The file to load must be in the
/// Assets/Resources folder of the Unity project and must have '.bytes' extension.
///
/// The TouchSense Player API supports TouchSense effects defined in a binary format stored in IVT files.
/// To produce TouchSense effect definitions in IVT files, designers use MOTIV Studio, an interactive
/// authoring tool for TouchSense effects. By default, Studio saves effect definitions in IVS files,
/// a human-readable text format based on XML. It also has an IVT file export feature to save effect
/// definitions in the compact IVT file format, which is readable by the API.
///
/// IVT files can store definitions for any type of effect that is supported by the TouchSense Player API.
/// IVT files provide an alternate way of defining simple effects, besides using API functions to
/// programmatically create the effects. The API does not provide functions to define Timeline effects;
/// therefore, IVT files are the only way to present Timeline effects to the API.
///
/// The main advantage of using IVT files is that the process of defining effects is separated from the
/// process of coding the effects. To fine-tune effects, designers interact with MOTIV Studio and create
/// revised IVT files as needed rather than adjust parameters in the application's code.
///
/// The method of defining and using effects in IVT files is summarized as follows.
///     1. Use Haptic Studio to create and test the desired effects. Save the effects in IVS files.
///     2. When the effects are ready to be integrated into the application, export the effects from
///     Haptic Studio into IVT files.
///     3. In the application, add code to call TouchSense Player API functions to play the effects defined
///     in IVT files.
/// </summary>
public class TouchSenseEffectBundle {

    private byte[] m_ivtBytes = null;
    private Hashtable m_effectTable = new Hashtable();
    private int m_count = -1;

    /// <summary>
    /// Creates an effect bundle from a file. The file loaded will need to be named filename.bytes where the
    /// filename part is the string given as parameter to this function.
    /// </summary>
    /// <param name="effectFileName"></param>
    public TouchSenseEffectBundle(string effectFileName)
    {
        TouchSense ts = TouchSense.instance;
        loadIVTFile(effectFileName);
    }

    /// <summary>
    /// The number of effects defined in this bundle
    /// </summary>
    public int effectCount
    {
        get
        {
            if (m_count < 0)
            {
                if (m_ivtBytes == null)
                {
                    Debug.Log("TouchSenseEffectBundle:m_ivtBytes == null when trying to get effect count!");
                    m_count = 0;
                }
                else
                {
#if UNITY_ANDROID
                    m_count = ImmVibeGetIVTEffectCount(m_ivtBytes);
#else
                    m_count = 0;
#endif
                }
            }
            return m_count;
        }
    }

    private int getEffectIndexFromName(string name)
    {
        if (m_ivtBytes == null)
        {
            Debug.LogError("TouchSense: Cannot get effect id from name because no ivt file loaded");
            return -1;
        }

        int rv = -1;

        if (m_effectTable.Contains(name))
        {
            rv = (int)m_effectTable[name];
        }
#if UNITY_ANDROID
        else
        {
            ImmVibeGetIVTEffectIndexFromName(m_ivtBytes, name, ref rv);
            m_effectTable.Add(name, rv);
        }
#endif

        return rv;
    }

    /// <summary>
    /// A convenience method to play an effect defined in this bundle.
    /// This is effectively the same as <code>getEffect(effectIndex).play();</code>
    /// </summary>
    /// <param name="effectIndex">The index of the effect to play</param>
    public void playEffect(int effectIndex)
    {
        getEffect(effectIndex).play();
    }

    /// <summary>
    /// A convenience method to play an effect defined in this bundle.
    /// This is effectively the same as <code>getEffect(effectName).play();</code>
    /// </summary>
    /// <param name="effectIndex">The name of the effect to play</param>
    public void playEffect(string effectName)
    {
        getEffect(effectName).play();
    }

    /// <summary>
    /// Get an effect defined in this bundle
    /// </summary>
    /// <param name="effectIndex">The index of the effect to get</param>
    /// <returns></returns>
    public TouchSenseIVTEffect getEffect(int effectIndex)
    {
        return new TouchSenseIVTEffect(m_ivtBytes, effectIndex);
    }

    /// <summary>
    /// Get an effect defined in this bundle
    /// </summary>
    /// <param name="effectIndex">The name of the effect to get</param>
    /// <returns></returns>
    public TouchSenseIVTEffect getEffect(string effectName)
    {
        return new TouchSenseIVTEffect(m_ivtBytes, getEffectIndexFromName(effectName));
    }

    private bool loadIVTFile(string filename)
    {
        TextAsset ta = Resources.Load(filename, typeof(TextAsset)) as TextAsset;
        if (ta == null)
        {
            Debug.LogError("TouchSense: Failed to open resource " + filename + ".bytes!");
            return false;
        }

        m_ivtBytes = ta.bytes;
        return true;
    }

#if UNITY_ANDROID
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibeGetIVTEffectIndexFromName(byte[] pIVT, string szEffectName,
        ref int pnEffectIndex);
    [DllImport("ImmEmulatorJ")]
    private static extern int ImmVibeGetIVTEffectCount(byte[] pIVT);
#endif
}
