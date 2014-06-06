#pragma strict
@script AddComponentMenu ("MAPNAV/GetGeolocation")
//MAPNAV Navigation ToolKit v.1.0
//Attention: This script uses a custom editor inspector: MAPNAV/Editor/GetGeoInspector.cs

var lat: float;
var lon: float;
var height: float;
var orientation: float;
var scaleX: float;
var scaleY: float;
var scaleZ: float;
private var posX: float;
private var posY: float;
private var posZ: float;
private var initX: float;
private var initZ: float;
private var gps: MapNav;
private var gpsFix: boolean;

function Awake(){
	gps = GameObject.FindGameObjectWithTag("GameController").GetComponent(MapNav);
	gpsFix = gps.gpsFix;
}

function Start () {
	while(!gpsFix){
		gpsFix = gps.gpsFix;
		yield;
	}
	initX=gps.iniRef.x;
	initZ=gps.iniRef.z;

}

function Update(){
	if(gpsFix){
		orientation=transform.eulerAngles.y;
		posX=transform.position.x;
		posZ=transform.position.z;
		height=transform.position.y*100; //1:100 scale (1 Unity world unit = 100 real world meters)
		scaleX=transform.localScale.x;
		scaleY=transform.localScale.y;
		scaleZ=transform.localScale.z;
		lat= ((360/Mathf.PI)*Mathf.Atan(Mathf.Exp(0.00001567855943*(posZ+initZ))))-90;	
		lon= (18000 * (posX+initX))/20037508.34;
	}
	else{
		lat= 0;
		lon= 0;
	}
}