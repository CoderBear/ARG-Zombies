#pragma strict
@script AddComponentMenu ("MAPNAV/GPS-Status")
//MAPNAV Navigation ToolKit v.1.0
//This script is for illustrative purposes only. Feel free to modify, extend or customize it to fit your own needs.

var refreshRate : float=0.2;
var style : GUIStyle;
var style2 : GUIStyle;
private var gps : MapNav;
private var ddLat : String;
private var ddLon : String;
private var dmsLat : String;
private var dmsLon : String;
private var heading : float;
private var error : float;
private var status : String;
private var screenX : int;
private var screenY : int;
private var zoom : int;
private var altitude :float;
private var info : boolean;

function Awake(){
	//Reference to MapNav.js script. Make sure that the map object containing the MapNav.js script is tagged as "GameController"
	gps = GameObject.FindGameObjectWithTag("GameController").GetComponent(MapNav);
	screenX=Screen.width;
	screenY=Screen.height;
}

//Get gps Status Data every "refreshRate" seconds
InvokeRepeating("GetData",1.0,refreshRate);

function GetData(){
	//Current latitude (decimal)
	ddLat = gps.userLat.ToString();
	//Current longitude (decimal)
	ddLon = gps.userLon.ToString();
	//Current latitude (degrees, minutes, seconds)
	dmsLat = gps.dmsLat;
	//Current longitude (degrees, minutes, seconds)
	dmsLon = gps.dmsLon;
	//Current heading/orientation
	heading=gps.heading;
	//Current GPS sensor accuracy
	error=gps.accuracy;
	//Current Zoom Level
	zoom=gps.zoom;
	//Current altitude(meters)
	altitude=gps.altitude;
}

function Update(){
	//Reference to MapNav.js "status" variable  
	status=gps.status;
	//Reference to MapNav.js "info" variable. Used to activate/de-activate the GUI elements.
	info=gps.info;
}

function OnGUI () {
	if(info){
		//These GUI Styles can be modified using the inspector
		style.fontSize=(screenX+screenY)*0.015;
		style2.fontSize=(screenX+screenY)*0.015;
		//Display current gps Status data
		GUI.BeginGroup (Rect (0,0, screenX, screenY/4));
		GUI.Box (Rect (0,0,screenX, screenY/4), "");
		GUI.Label(Rect(screenX/40,screenY/50,screenX-screenX/20,screenY/50),"Latitude: "+ dmsLat,style);
		GUI.Label(Rect(screenX/40,3*screenY/50,screenX-screenX/20,screenY/50),"Longitude: "+ dmsLon,style);
		GUI.Label(Rect(screenX/40,5*screenY/50,screenX-screenX/20,screenY/50),"Altitude(m): "+ altitude,style);
		
		GUI.Label(Rect(screenX/40,screenY/50,screenX-screenX/20,screenY/50),"Heading: "+ Mathf.Round(heading),style2);
		GUI.Label(Rect(screenX/40,3*screenY/50,screenX-screenX/20,screenY/50),"Zoom Level: "+ zoom,style2);
		GUI.Label(Rect(screenX/40,5*screenY/50,screenX-screenX/20,screenY/50),"Error(m): "+ error,style2);
		
		GUI.Label(Rect(screenX/40,7*screenY/50,screenX-screenX/20,screenY/25), "Status: "+status,style);
		GUI.EndGroup ();
	}
}