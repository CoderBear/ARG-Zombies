#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
#define UNITY_4_3_AND_LATER
#endif

#if (UNITY_2_6 || UNITY_2_6_1 || UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
#define ONLY_SINGLE_SELECTION_SUPPORTED_IN_INSPECTOR
#endif

#if UNITY_4_3_AND_LATER

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

//-------------------------------------------------------------------------
/// <summary>
/// Editor class for the RuntimeAnimatedColliderSwitch component.
/// </summary>
[CustomEditor(typeof(RuntimeAnimatedColliderSwitch))]
[CanEditMultipleObjects]
public class EditorScriptRuntimeAnimatedColliderSwitch : Editor {

	public override void OnInspectorGUI() {
		EditorGUILayout.LabelField("This script updates the colliders at Runtime according to the precomputed collider frames.");
	}

	//-------------------------------------------------------------------------
	[DrawGizmo(GizmoType.SelectedOrChild)]
	static void DrawColliderRuntimeInfo(RuntimeAnimatedColliderSwitch updateObject, GizmoType gizmoType)
	{
		Handles.Label(updateObject.transform.position, "Animated\nColliders");
	}
}

#endif