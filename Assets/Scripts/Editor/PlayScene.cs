using UnityEngine;
using System.Collections;
using UnityEditor;

/*
 * Written by Ralph A. Barbagallo III
 * 
 * www.ralphbarbagallo.com
 * 
 * I wrote this becuase I was sick of switching scenes to play my loader scene and test my game.
 * If you break up your game into multiple scenes, this could be a timesaver.  Even if it is
 * ridiculously simplistic
 */

public class PlayScene : EditorWindow {
	/*
	 * SetScene
	 * 
	 * Saves the currently selected scene name to the prefs.  Bails
	 * if not a scene.
	 */
	
	[MenuItem("Window/Set Scene")]
	public static void SetScene()
	{
		Object sel = Selection.activeObject;
		
		if (sel == null)
		{
			Debug.LogError("No scene selected.");
			return;
		}
		
		string sceneName = AssetDatabase.GetAssetOrScenePath(sel);
		
		if (!sceneName.EndsWith(".unity"))
		{
			Debug.LogError("Not a scene.");
			return;
		}
		
		EditorPrefs.SetString("runscene", sceneName);
		Debug.Log("Scene set to: " + sceneName);
	}
	
	/*
	 * PlaySetScene
	 *
	 * Load scene at path saved in the prefs, then run it.
	 */
	
	[MenuItem("Window/Play Scene %p")]
	public static void PlaySetScene()
	{
		string sceneName = EditorPrefs.GetString("runscene", null);
		
		if (sceneName == null)
		{
			Debug.LogError("No scene saved.  Select scene and use SetScene to choose the scene to load.");
			return;
		}
		
		EditorApplication.OpenScene(sceneName);
		EditorApplication.ExecuteMenuItem("Edit/Play");
	}
}