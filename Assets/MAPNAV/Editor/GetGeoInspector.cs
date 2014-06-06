//MAPNAV Navigation ToolKit v.1.0
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GetGeolocation))]
public class GetGeoInspector : Editor {
	private SerializedObject getGeo;
	private SerializedProperty
		lat,
		lon,
		height,
		orientation,
		scaleX,
		scaleY,
		scaleZ;

	private void OnEnable(){
 		getGeo = new SerializedObject(target);
		lat = getGeo.FindProperty("lat");
		lon = getGeo.FindProperty("lon");
		height = getGeo.FindProperty("height");
		orientation = getGeo.FindProperty("orientation");
		scaleX = getGeo.FindProperty("scaleX");
		scaleY = getGeo.FindProperty("scaleY");
		scaleZ = getGeo.FindProperty("scaleZ");
	}

	public override void OnInspectorGUI () {
		getGeo.Update();
		EditorGUILayout.HelpBox("Use during Runtime when map is displayed.",MessageType.Info);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Latitude (Read Only):",lat.floatValue.ToString());
		EditorGUILayout.LabelField("Longitude (Read Only):",lon.floatValue.ToString());
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("",GUILayout.Width(Screen.width/4));
		if(GUILayout.Button("Copy Lat/Lon/Transform", GUILayout.Width(Screen.width/2),GUILayout.Height(30))){		
        	//Use PlayerPrefs to store transform and geolocation data
			PlayerPrefs.SetFloat("Lat"+target.name, lat.floatValue);
        	PlayerPrefs.SetFloat("Lon"+target.name, lon.floatValue);
			PlayerPrefs.SetFloat("Height"+target.name, height.floatValue);
			PlayerPrefs.SetFloat("Orient"+target.name, orientation.floatValue);
			PlayerPrefs.SetFloat("ScaleX"+target.name, scaleX.floatValue);
			PlayerPrefs.SetFloat("ScaleY"+target.name, scaleY.floatValue);
			PlayerPrefs.SetFloat("ScaleZ"+target.name, scaleZ.floatValue);
			Debug.Log(target.name+" location saved!\nPlease use the SetGeolocation script to geolocate gameObject using saved data.\n");	
		}	
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		getGeo.ApplyModifiedProperties ();
	}	
}