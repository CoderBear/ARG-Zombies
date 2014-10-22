using UnityEngine;
using UnityEditor;
using System.Collections;

#if UNITY_EDITOR

//-------------------------------------------------------------------------
/// <summary>
/// Editor class for the AlphaMeshColliderUpdateOTTilesSpriteColliders component.
/// </summary>
[CustomEditor(typeof(AlphaMeshColliderUpdateOTTilesSpriteColliders))]
public class EditorScriptAlphaMeshColliderUpdateOTTilesSpriteColliders : Editor {

	//-------------------------------------------------------------------------
	public override void OnInspectorGUI() {
		
		//EditorGUIUtility.LookLikeInspector();
		
		EditorGUILayout.LabelField("This script updates the colliders at Runtime according to the tiles.");
		
		//EditorGUIUtility.LookLikeControls();
	}
	
	//-------------------------------------------------------------------------
	[DrawGizmo(GizmoType.SelectedOrChild)]
    static void DrawColliderRuntimeInfo(AlphaMeshColliderUpdateOTTilesSpriteColliders updateObject, GizmoType gizmoType)
    {
		if (updateObject.transform.Find(AlphaMeshColliderUpdateOTTilesSpriteColliders.RUNTIME_GROUP_NODE_NAME) == null) {
			Handles.Label(updateObject.transform.position, "Colliders added at Runtime");
		}
    }
}

#endif // #if UNITY_EDITOR
