using UnityEngine;
using System.Collections;

public class ARGZMap : MonoBehaviour {
	public static AndroidJavaClass ARGZMapJavaClass;

	// Use this for initialization
	void Start () {
		if (Application.platform == RuntimePlatform.Android) {
			// Initialize Android View
			ARGZMapJavaClass = new AndroidJavaClass("com.tandosbs.argz.ARGZappUnity");
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			if (GUI.Button(new Rect(10, 300, 150, 120), "Show android Screen"))
			{
				AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
				activity.Call("showAndroidView");
			}
		}
		else
		{
			if (GUI.Button(new Rect(10, 300, 150, 120), "Show non-android Screen"))
			{
				
			}			
		}		
	}
}