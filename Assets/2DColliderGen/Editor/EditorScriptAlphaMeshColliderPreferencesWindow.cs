#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
#define UNITY_4_3_AND_LATER
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;

//-------------------------------------------------------------------------
/// <summary>
/// Editor window for the AlphaMeshCollider preference values.
/// </summary>
public class EditorScriptAlphaMeshColliderPreferencesWindow : EditorWindow {
	
	GUIContent mDefaultColliderDirectoryLabel = new GUIContent("Collider Directory", "Set the default output directory for generated collider mesh files.");
	GUIContent mDefaultLiveUpdateLabel = new GUIContent("Live Update", "Recalculate the collider mesh when changing parameters in the inspector.");
	GUIContent mDefaultColliderPointCountLabel = new GUIContent("Outline Vertex Count", "Default point count of the collider shape.");
	GUIContent mColliderPointCountSliderMaxValueLabel = new GUIContent("Vertex Count Slider Max", "Maximum value of the outline vertex count slider.");
	GUIContent mDefaultColliderThicknessLabel = new GUIContent("Z-Thickness", "Default thickness of a collider.");
#if UNITY_4_3_AND_LATER
	GUIContent mDefaultTargetColliderTypeLabel = new GUIContent("Collider Type", "Default output collider type - MeshCollider or PolygonCollider2D.");
#endif

	//-------------------------------------------------------------------------
	[MenuItem ("2D ColliderGen/Collider Preferences", false, 10000)]
	static void ColliderPreferences() {
		
		// Get existing open window or if none, make a new one:
		EditorScriptAlphaMeshColliderPreferencesWindow window = EditorWindow.GetWindow<EditorScriptAlphaMeshColliderPreferencesWindow>();
		window.title = "Default Values";
    }
	
	//-------------------------------------------------------------------------
	void OnGUI()
	{
		//EditorGUIUtility.LookLikeControls(150.0f);
		
		AlphaMeshColliderPreferences.Instance.DefaultColliderDirectory = EditorGUILayout.TextField(mDefaultColliderDirectoryLabel, AlphaMeshColliderPreferences.Instance.DefaultColliderDirectory);
		AlphaMeshColliderPreferences.Instance.DefaultLiveUpdate = EditorGUILayout.Toggle(mDefaultLiveUpdateLabel, AlphaMeshColliderPreferences.Instance.DefaultLiveUpdate);
		AlphaMeshColliderPreferences.Instance.DefaultColliderPointCount = EditorGUILayout.IntField(mDefaultColliderPointCountLabel, AlphaMeshColliderPreferences.Instance.DefaultColliderPointCount);
		AlphaMeshColliderPreferences.Instance.ColliderPointCountSliderMaxValue = EditorGUILayout.IntField(mColliderPointCountSliderMaxValueLabel, AlphaMeshColliderPreferences.Instance.ColliderPointCountSliderMaxValue);
		AlphaMeshColliderPreferences.Instance.DefaultAbsoluteColliderThickness = EditorGUILayout.FloatField(mDefaultColliderThicknessLabel, AlphaMeshColliderPreferences.Instance.DefaultAbsoluteColliderThickness);	
#if UNITY_4_3_AND_LATER
		AlphaMeshColliderPreferences.Instance.DefaultTargetColliderType = (AlphaMeshCollider.TargetColliderType) EditorGUILayout.EnumPopup(mDefaultTargetColliderTypeLabel, AlphaMeshColliderPreferences.Instance.DefaultTargetColliderType);
#endif
	}
}
