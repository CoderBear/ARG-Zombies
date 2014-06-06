//MAPNAV Navigation ToolKit v.1.0
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapNav))]
public class MapNavInspector : Editor {
	
	bool showZoom =true;
	bool showTouch =true;
	private string[] mapTypes = new string[]{"map","sat","hyb"}; //Provide here the available map-types for your maps provider.
	private string[] mapSizes = new string[]{"640x640","1280x1280","1920x1920","2560x2560"};
	private SerializedObject myLoc;
	private SerializedProperty
		simGPS,
		userSpeed,
		realSpeed,
		fixLat,
		fixLon,
		heading,
		zoom,
		key,
		minZoom,
		maxZoom,
		index,
		indexSize,
		triDView,
		camDist,
		camAngle,
		maxWait,
		buttons,
		initTime,
		dmsLat,
		dmsLon,
		updateRate,
		autoCenter,
		mapDisabled,
		freeCam,
		dragToPan,
		pinchToZoom;
		
	private void OnEnable(){

 		myLoc = new SerializedObject(target);
		simGPS = myLoc.FindProperty("simGPS");
		userSpeed = myLoc.FindProperty("userSpeed");
		realSpeed = myLoc.FindProperty("realSpeed");
		fixLat = myLoc.FindProperty("fixLat");
		fixLon = myLoc.FindProperty("fixLon");
		heading = myLoc.FindProperty("heading");
		zoom = myLoc.FindProperty("zoom");
		key = myLoc.FindProperty("key");
		minZoom = myLoc.FindProperty("minZoom");
		maxZoom = myLoc.FindProperty("maxZoom");
		index = myLoc.FindProperty("index");
		indexSize = myLoc.FindProperty("indexSize");
		triDView = myLoc.FindProperty("triDView");
		camDist = myLoc.FindProperty("camDist");
		camAngle = myLoc.FindProperty("camAngle");
		maxWait = myLoc.FindProperty("maxWait");
		buttons = myLoc.FindProperty("buttons");
		initTime = myLoc.FindProperty("initTime");
		dmsLat = myLoc.FindProperty("dmsLat");
		dmsLon = myLoc.FindProperty("dmsLon");
		updateRate = myLoc.FindProperty("updateRate");
		autoCenter = myLoc.FindProperty("autoCenter");
		mapDisabled= myLoc.FindProperty("mapDisabled");
		freeCam = myLoc.FindProperty("freeCam");
		dragToPan = myLoc.FindProperty("dragToPan");
		pinchToZoom = myLoc.FindProperty("pinchToZoom");
	}
 
	
	public override void OnInspectorGUI () {
		
		myLoc.Update();
		
		//GPS Emulator 
		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(key,new GUIContent("Maps AppKey"));
		EditorGUILayout.PropertyField(simGPS,new GUIContent("GPS Emulator"));
		EditorGUI.indentLevel++;
		if(simGPS.boolValue){
			
			//Emulator Pointer Speed 
			EditorGUILayout.PropertyField(userSpeed,new GUIContent("Pointer Speed"),GUILayout.MaxWidth(250));
			EditorGUILayout.PropertyField(realSpeed,new GUIContent("Realistic Speed"),GUILayout.MaxWidth(250));
			EditorGUILayout.HelpBox("On Emulator Mode use WASD or arrow keys to navigate.",MessageType.Info);
			EditorGUILayout.Space();
		}	
		EditorGUI.indentLevel--;
		
		//Latitude / Longitude 
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(fixLat,new GUIContent("Latitude (decimal)"),GUILayout.Width(250));
		EditorGUILayout.LabelField(dmsLat.stringValue);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(fixLon,new GUIContent("Longitude (decimal)"),GUILayout.Width(250));
		EditorGUILayout.LabelField(dmsLon.stringValue);
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.LabelField("Heading(Read Only)",(Mathf.Round(heading.floatValue)).ToString(),GUILayout.Width(250));
		EditorGUILayout.Space();
		
		//Zoom Control 
		showZoom = EditorGUILayout.Foldout(showZoom,"Zoom Levels");
		EditorGUILayout.Space();
		if(showZoom){
		EditorGUI.indentLevel++;	
		EditorGUILayout.IntSlider(zoom,0,20,new GUIContent("Default/Current"));	
		EditorGUILayout.IntSlider(minZoom,0,20,new GUIContent("Min."));
		EditorGUILayout.IntSlider(maxZoom,0,20,new GUIContent("Max."));
		EditorGUILayout.Space();
		EditorGUI.indentLevel--; 
		}
		
		
		//Map Type Drop-down List
		index.intValue = EditorGUILayout.Popup("Map Type",index.intValue,mapTypes);
		//Map Size Drop-down List
		indexSize.intValue = EditorGUILayout.Popup("Map Size(px)",indexSize.intValue,mapSizes);	
		//Disable Map
		EditorGUILayout.PropertyField(mapDisabled,new GUIContent("Disable Map"));
		EditorGUILayout.Space();
		
		//3D Perspective Camera View
		EditorGUILayout.PropertyField(triDView,new GUIContent("3D View"));
		
		EditorGUI.indentLevel++;
		//3D and Camera Follows Player. Set Camera Position
		if(triDView.boolValue==true && freeCam.boolValue==false){
			EditorGUILayout.Slider(camDist,1,20,new GUIContent("Camera Dist"));	
			EditorGUILayout.IntSlider(camAngle,1,89,new GUIContent("Camera Angle"));
		}
		//3D and Free Camera
		else if(triDView.boolValue==true && freeCam.boolValue==true){
			//No options
		}
		//2D
		else{
			//Camera Height automatically set according to tile size in order to maximise map visible area
		}
		EditorGUI.indentLevel--; 
		
		//Camera default settings
		if(triDView.boolValue==true){
			EditorGUILayout.PropertyField(freeCam,new GUIContent("Free Camera"));
			Camera.main.orthographic=false;
			Camera.main.nearClipPlane=0.3F;
			Camera.main.farClipPlane=656100.0F;
		}else{
			Camera.main.orthographic=true;
			//Camera.main.orthographicSize=10.0F;
			Camera.main.nearClipPlane=0.1F;
			Camera.main.farClipPlane=11.0F;	
	
		}
		
		if(triDView.boolValue==true){
			//Auto Center 
			EditorGUILayout.PropertyField(autoCenter);
		}

		//User Interface Buttons
		EditorGUILayout.PropertyField(buttons,new GUIContent("GUI Buttons"));
		EditorGUILayout.Space();
		
		//Additional Config Options
		if(!simGPS.boolValue){	
			EditorGUILayout.PropertyField(updateRate,new GUIContent("Pointer Update Rate"),GUILayout.MaxWidth(200));
			EditorGUILayout.PropertyField(maxWait,new GUIContent("GPS Fix Timeout"),GUILayout.MaxWidth(200));	
		}	
		EditorGUILayout.PropertyField(initTime,new GUIContent("Init. Time"),GUILayout.MaxWidth(200));
		EditorGUILayout.Space();
		if(triDView.boolValue==false){
			showTouch = EditorGUILayout.Foldout(showTouch,"Touch interactions");
			if(showTouch){
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(dragToPan,new GUIContent("Drag to pan"));
				EditorGUILayout.PropertyField(pinchToZoom,new GUIContent("Pinch to zoom"));
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.Space();
		}
		
		if(simGPS.boolValue){
			EditorGUILayout.HelpBox("Deactivate the GPS emulator before building for mobile devices.",MessageType.Warning);
		}
		
		myLoc.ApplyModifiedProperties ();
	}
}