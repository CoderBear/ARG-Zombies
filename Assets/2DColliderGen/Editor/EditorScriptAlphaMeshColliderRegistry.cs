using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR // FIXXXME: uncomment his again! for some reason this mono project does not define the unity defs..

//-------------------------------------------------------------------------
/// <summary>
/// Editor script for the AlphaMeshColliderRegistry component.
/// </summary>
[CustomEditor(typeof(AlphaMeshColliderRegistry))]
public class EditorScriptAlphaMeshColliderRegistry : Editor {
	
	protected Dictionary<string, bool> mShowGroupAtPath = new Dictionary<string, bool>();
	
	//-------------------------------------------------------------------------
	public override void OnInspectorGUI() {
		
		AlphaMeshColliderRegistry registry = (AlphaMeshColliderRegistry)target;
		
		//EditorGUIUtility.LookLikeInspector();
		
		EditorGUILayout.TextField("Collider Groups", registry.mColliderGroups.Count.ToString());
		foreach (AlphaMeshColliderRegistry.ColliderGroup colliderGroup in registry.mColliderGroups) {
			
			EditorGUI.indentLevel = 0; // Indent 0
			
			string path = colliderGroup.mFullColliderMeshPath;
			int pointCount = 0;
			if ((colliderGroup.FirstColliderMesh) &&
			    (colliderGroup.FirstColliderMesh.triangles != null) &&
			    (colliderGroup.FirstColliderMesh.triangles.Length > 0)) {
				
				pointCount = colliderGroup.FirstColliderMesh.triangles.Length / 6;
			}
			else if (colliderGroup.mAlphaMeshColliderObjects != null && colliderGroup.mAlphaMeshColliderObjects.Count != 0) {
				AlphaMeshCollider firstInstance = (AlphaMeshCollider) colliderGroup.mAlphaMeshColliderObjects[0].Target;
				pointCount = firstInstance.ColliderRegionsTotalMaxPointCount;
			}
			else if (colliderGroup.mGeneratedColliderData != null &&
			         colliderGroup.mGeneratedColliderData.Length > 0 &&
			         colliderGroup.mGeneratedColliderData[0].mOutlineAlgorithm != null) {

				pointCount = colliderGroup.mGeneratedColliderData[0].mOutlineAlgorithm.MaxPointCount;
			}
			
			bool showGroup = mShowGroupAtPath.ContainsKey(path) && mShowGroupAtPath[path];
			string foldoutString = System.IO.Path.GetFileName(path);
			if (pointCount != 0) {
				foldoutString += "\t  [" + pointCount + " vertices]";
			}
			if (colliderGroup.mAlphaMeshColliderObjects != null) {
				foldoutString += "\t  " + colliderGroup.mAlphaMeshColliderObjects.Count + "x";
			}
			
			mShowGroupAtPath[path] = EditorGUILayout.Foldout(showGroup, foldoutString);
        	if(mShowGroupAtPath[path]) {
				
				EditorGUI.indentLevel = 1; // Indent 1
				
				EditorGUILayout.TextField("Collider Mesh Path", path);
				if (pointCount != 0) {
					EditorGUILayout.IntField("Outline Vertex Count", pointCount);
				}
				else {
					EditorGUILayout.TextField("Outline Vertex Count", "<not yet calculated>");
				}
				EditorGUILayout.ObjectField("Mesh", colliderGroup.FirstColliderMesh, typeof(Mesh), true);
				if (colliderGroup.mAlphaMeshColliderObjects == null) {
					EditorGUILayout.LabelField("No Instances");
				}
				else {
					int index = 1;
					foreach (System.WeakReference colliderInstanceRef in colliderGroup.mAlphaMeshColliderObjects) {
						AlphaMeshCollider instance = (AlphaMeshCollider) colliderInstanceRef.Target;
						EditorGUILayout.ObjectField("Instance " + index++, instance, typeof(AlphaMeshCollider), true);
					}
					if(GUILayout.Button("Select Instances")) {
						GameObject[] newSelection = new GameObject[colliderGroup.mAlphaMeshColliderObjects.Count];
						int selectionIndex = 0;
						foreach (System.WeakReference colliderInstanceRef in colliderGroup.mAlphaMeshColliderObjects) {
							AlphaMeshCollider instance = (AlphaMeshCollider) colliderInstanceRef.Target;
							newSelection[selectionIndex++] = instance.gameObject;
						}
							
						Selection.objects = newSelection;
					}
				}
			}
		}
		if(GUILayout.Button("Select All Instances")) {
			SelectAllInstances(registry);
		}
		
		EditorGUI.indentLevel = 0;
		
		//EditorGUIUtility.LookLikeControls();
    }
	
    //-------------------------------------------------------------------------
	protected void SelectAllInstances(AlphaMeshColliderRegistry registry) {
		
		int numInstances = 0;
		foreach (AlphaMeshColliderRegistry.ColliderGroup colliderGroup in registry.mColliderGroups) {
			if (colliderGroup.mAlphaMeshColliderObjects != null) {
				numInstances += colliderGroup.mAlphaMeshColliderObjects.Count;
			}
		}
		
		GameObject[] newSelection = new GameObject[numInstances];
		int selectionIndex = 0;
		
		foreach (AlphaMeshColliderRegistry.ColliderGroup colliderGroup in registry.mColliderGroups) {
			if (colliderGroup.mAlphaMeshColliderObjects != null) {
				
				foreach (System.WeakReference colliderInstanceRef in colliderGroup.mAlphaMeshColliderObjects) {
					AlphaMeshCollider instance = (AlphaMeshCollider) colliderInstanceRef.Target;
					newSelection[selectionIndex++] = instance.gameObject;
				}
			}
		}
		Selection.objects = newSelection;
	}
}

#endif // #if UNITY_EDITOR
