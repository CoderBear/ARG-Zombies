#pragma strict
@script AddComponentMenu ("MAPNAV/SetGeolocation")
//MAPNAV Navigation ToolKit v.1.0
//Attention: This script uses a custom editor inspector: MAPNAV/Editor/SetGeoInspector.cs

var lat : float;
var lon : float;
var height : float;
var orientation : float;
var scaleX : float;
var scaleY : float;
var scaleZ : float;
private var initX : float;
private var initZ : float;
private var gps : MapNav;
private var gpsFix : boolean;
private var fixLat : float;
private var fixLon : float;

function Awake(){
	//Reference to the MapNav.js script and gpsFix variable. gpsFix will be true when a valid location data has been set.
	gps = GameObject.FindGameObjectWithTag("GameController").GetComponent(MapNav);
	gpsFix = gps.gpsFix;
}

function Start () {
	//Wait until the gps sensor provides a valid location.
	while(!gpsFix){
		gpsFix = gps.gpsFix;
		yield;
	}
	//Read initial position (used as a reference system)
	initX=gps.iniRef.x;
	initZ=gps.iniRef.z;
	//Set object geo-location
	GeoLocation();

}

//InvokeRepeating("GeoLocation",0.0,0.2);
//@ContextMenu ("GeoLocation")
function GeoLocation(){
		//Translate the geographical coordinate system used by gps mobile devices(WGS84), into Unity's Vector2 Cartesian coordinates(x,z).
		transform.position.x= ((lon*20037508.34)/18000) - initX;
		transform.position.z= ((Mathf.Log(Mathf.Tan((90 +lat) * Mathf.PI / 360)) /(Mathf.PI / 180))*1113.19490777778) - initZ;
		//Set object height and orientation
		transform.position.y= height/100; //1:100 scale
		transform.eulerAngles.y=orientation;
		//Set local object scale
		transform.localScale.x=scaleX;
		transform.localScale.y=scaleY;
		transform.localScale.z=scaleZ;
}
//This function is similar to GeoLocation() but is to be used by SetGeoInspector.cs
function EditorGeoLocation(){

		gps = GameObject.FindGameObjectWithTag("GameController").GetComponent(MapNav);
		fixLat=gps.fixLat;
		fixLon=gps.fixLon;
	
		initX = fixLon * 20037508.34 / 18000;
   		initZ = System.Math.Log(System.Math.Tan((90 + fixLat) * System.Math.PI / 360)) / (System.Math.PI / 180);
  		initZ = initZ * 20037508.34 / 18000;  
		
		transform.position.x= (lon*20037508.34/18000) - initX;
		transform.position.z= ((Mathf.Log(Mathf.Tan((90 +lat) * Mathf.PI / 360)) /(Mathf.PI / 180))*1113.19490777778) - initZ;
		transform.position.y= height/100; //1:100 scale
		transform.eulerAngles.y=orientation;
		
		transform.localScale.x=scaleX;
		transform.localScale.y=scaleY;
		transform.localScale.z=scaleZ;
}