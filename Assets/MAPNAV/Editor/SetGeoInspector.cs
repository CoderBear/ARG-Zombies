//MAPNAV Navigation ToolKit v.1.0
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SetGeolocation))]
public class SetGeoInspector : Editor {

	private SerializedObject setGeo;
	private SerializedProperty
		setLat,
		setLon,
		height,
		orientation,
		scaleX,
		scaleY,
		scaleZ;

	private void OnEnable(){
 		setGeo = new SerializedObject(target);
		setLat = setGeo.FindProperty("lat");
		setLon = setGeo.FindProperty("lon");
		height = setGeo.FindProperty("height");
		orientation = setGeo.FindProperty("orientation");
		scaleX = setGeo.FindProperty("scaleX");
		scaleY = setGeo.FindProperty("scaleY");
		scaleZ = setGeo.FindProperty("scaleZ");
	}
	
	public override void OnInspectorGUI () {
		setGeo.Update();
		EditorGUILayout.Space();
		EditorGUILayout.HelpBox("Use in Editor after game has been stopped.",MessageType.Info);
		EditorGUILayout.PropertyField(setLat,new GUIContent("Latitude:"),GUILayout.MaxWidth(250));
		EditorGUILayout.PropertyField(setLon,new GUIContent("Longitude:"),GUILayout.MaxWidth(250));
		EditorGUILayout.PropertyField(height,new GUIContent("Height (m):"),GUILayout.MaxWidth(250));
		EditorGUILayout.PropertyField(orientation,new GUIContent("Orientation:"),GUILayout.MaxWidth(250));
		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button("Paste Lat/Lon/Transform", GUILayout.Width(Screen.width/2-5),GUILayout.Height(30))){
	        //Read transform and geolocation data from PlayerPrefs
			setLat.floatValue=PlayerPrefs.GetFloat("Lat"+target.name); 
	       	setLon.floatValue=PlayerPrefs.GetFloat("Lon"+target.name);
			height.floatValue=PlayerPrefs.GetFloat("Height"+target.name);
			orientation.floatValue=PlayerPrefs.GetFloat("Orient"+target.name);
			scaleX.floatValue=PlayerPrefs.GetFloat("ScaleX"+target.name);
			scaleY.floatValue=PlayerPrefs.GetFloat("ScaleY"+target.name);
			scaleZ.floatValue=PlayerPrefs.GetFloat("ScaleZ"+target.name);
			Debug.Log("Geolocation succesfully loaded! - "+target.name);	
		}
		
		if(GUILayout.Button("Apply", GUILayout.Width(Screen.width/2-5),GUILayout.Height(30))){
			((SetGeolocation)target).EditorGeoLocation();
			Debug.Log("GameObject position set.");	
		}	
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.Space();
		setGeo.ApplyModifiedProperties ();
	}	
}