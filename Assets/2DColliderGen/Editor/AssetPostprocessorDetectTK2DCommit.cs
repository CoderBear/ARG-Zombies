using UnityEngine;
using System.Collections;
using UnityEditor;

//-------------------------------------------------------------------------
/// <summary>
/// AssetPostprocessor to detect when a tk2d SpriteCollection was "committed".
/// </summary>
public class AssetPostprocessorDetectTK2DCommit : AssetPostprocessor {

	static private EditorScriptAlphaMeshColliderTK2DWindow mTargetColliderGenTK2DWindow = null;
	
	public static EditorScriptAlphaMeshColliderTK2DWindow TargetColliderGenTK2DWindow {
		set {
			mTargetColliderGenTK2DWindow = value;
		}
		get {
			return mTargetColliderGenTK2DWindow;
		}
	}
	
	//-------------------------------------------------------------------------
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath) {

		if (mTargetColliderGenTK2DWindow != null) {
			foreach (string path in importedAssets) {
				if (path.Equals(EditorScriptAlphaMeshColliderTK2DWindow.AtlasPathToMonitorForCommit)) {
					mTargetColliderGenTK2DWindow.OnSpriteCollectionCommit();
				}
			}
		}
    }
}
