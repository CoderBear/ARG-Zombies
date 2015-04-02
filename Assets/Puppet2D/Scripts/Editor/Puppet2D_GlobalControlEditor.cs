using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Puppet2D_GlobalControl))] 
public class Puppet2D_GlobalControlEditor : Editor
{

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();		
		if(GUILayout.Button("Refresh Global Control"))
		{
			(target as Puppet2D_GlobalControl).Refresh();		
		}

	}
	
	
	
}