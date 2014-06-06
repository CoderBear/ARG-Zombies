#pragma strict
@script AddComponentMenu ("MAPNAV/MapNav")
//MAPNAV Navigation ToolKit v.1.0
//Attention: This script uses a custom editor inspector: MAPNAV/Editor/MapNavInspector.cs

var user : Transform;									 //User(Player) transform
var simGPS : boolean=true;								 //True when the GPS Emulator is enabled
var userSpeed : float = 5.0;							 //User speed when using the GPS Emulator (keyboard input)
var realSpeed : boolean = false;						 //If true, the perceived player speed depends on zoom level(realistic behaviour)
var fixLat : float=42.3627;					   	 		 //Latitude
var fixLon : float=-71.05686;							 //Longitude
var altitude : float;									 //Current GPS altitude
var heading : float;									 //Last compass sensor reading (Emulator disabled) or user's eulerAngles.y (Emulator enabled)
var accuracy : float;									 //GPS location accuracy (error)
var maxZoom : int=18;									 //Maximum zoom level available. Set according to your maps provider
var minZoom : int=1;									 //Minimum zoom level available
var zoom : int=17;										 //Current zoom level
private var multiplier:float; 							 //1 for a size=640x640 tile, 2 for size=1280*1280 tile, etc. Automatically set when selecting tile size
var key : String="Fmjtd%7Cluur29072d%2Cbg%3Do5-908s00"; //AppKey (API key) code obtained from your maps provider (MapQuest, Google, etc.). 
														 //Default MapQuest key for demo purposes only (with limitations). Please get your own key before you start yout project.
														 
var maptype : String[];									 //Array including available map types
var mapSize : int[];									 //Array including available map sizes(pixels)
var index :int;											 //maptype array index. 
var indexSize :int;										 //mapsize array index. 
var camDist : float=15.0;								 //Camera distance(3D) or height(2D) to user
var camAngle : int=40;									 //Camera angle from horizontal plane
var initTime : int = 3;									 //Hold time after a successful GPS fix in order to improve location accuracy
var maxWait : int = 30;									 //GPS fix timeout
var buttons : boolean=true;								 //Enables GUI sample control buttons 
var dmsLat : String;									 //Latitude as degrees, minutes and seconds
var dmsLon : String;							 		 //Longitude as degrees, minutes and seconds
var updateRate : float = 0.1;							 //User's position update rate
var autoCenter : boolean=true;							 //Autocenter and refresh map
var status : String;								     //GPS and other status messages
var gpsFix : boolean;								     //True after a successful GPS fix 
var iniRef : Vector3;							         //First location data retrieved on Start	 
var info : boolean;									     //Used by GPS-Status.js to enable/disable the GPS information window.
var triDView : boolean = false;						     //2D/3D modes toggle
var ready : boolean;								     //true when the map texture has been successfully loaded
var freeCam : boolean = false;							 //when true, MainCamera follows and looks at Player (3D mode only)
var pinchToZoom : boolean = true;						 //Enables Pinch to Zoom interaction on mobile devices
var dragToPan : boolean = true;							 //Enables Drag to Pan interaction on mobile devices
var mapDisabled: boolean;								 //Disables online maps
var mapping : boolean = false;							 //true while map is being downloaded
var cam : Transform;									 //Reference to the Main Camera transform
var userLon : float;									 //Current user position longitude
var userLat : float;									 //Current user position latitude


private var levelHeight : float;
private var smooth = 1.3;	 						    
private var yVelocity = 0.0;  
private var speed : float;
private var mycam: Camera;
private var currentOrtoSize : float;
private var loc : LocationInfo;
private var currentPosition : Vector3;
private var newUserPos : Vector3; 
private var currentUserPos : Vector3;
private var download : float;
private var www : WWW;
private var url = ""; 
private var longitude : double;
private var latitude : double;
private var rect : Rect;
private var screenX : float;
private var screenY : float;
private var maprender : Renderer;
private var mymap : Transform;
private var initPointerSize :float;
private var tempLat :double;
private var tempLon :double;
private var touchZoom : boolean;
private var centre: String;
private var centering : boolean;
private var centerIcon : Texture;
private var topIcon : Texture;
private var bottomIcon : Texture;
private var leftIcon : Texture;
private var rightIcon : Texture;
private var arrowIcon : GUIStyle;
private var dot : float;
private var centered : boolean = true;
private var borderTile: int =0;
private var tileLeft : boolean;
private var tileRight : boolean;
private var tileTop : boolean;
private var tileBottom : boolean;
private var topCursorPos: Rect;
private var rightCursorPos: Rect;
private var bottomCursorPos: Rect;
private var leftCursorPos: Rect;

//Touch Screen Variables
private var prevDist : Vector2;
private var actualDist : float;
private var target : Transform;
private var touch : Touch;
private var touch2 : Touch;
private var curDist : Vector2;
private var dragSpeed : float;
private var viewArea : Rect;
private var targetOrtoSize : float;
private var firstTime : boolean = true;
private var focusScreenPoint : Vector2 ;
private var focusWorldPoint : Vector3; 


function Awake(){
	//Set the map's tag to GameController
	transform.tag="GameController";
	
	//References to the Main Camera and Player. 
	//Please make sure your camera is tagged as "MainCamera" and your user visualization/character as "Player"
	cam=Camera.main.transform;
	mycam=Camera.main;
	user= GameObject.FindGameObjectWithTag("Player").transform;
	
	//Store most used components and values into variables for faster access.
	mymap=transform;
	maprender=renderer;
	screenX=Screen.width;
	screenY=Screen.height;	
	
	//Add possible values to maptype and mapsize arrays 
	//ATENTTION: Modify if using a maps provider other than MapQuest Open Static Maps.
	maptype = ["map","sat","hyb"];
	mapSize = [640,1280,1920,2560]; //in pixels
	
	//Set GUI "center" button label
	if(triDView){
		centre="refresh";
	}
	//Enable autocenter on 2D-view (default)
	else{
		autoCenter=true;
	}
	
	//Load required interface textures
	centerIcon=Resources.Load("centerIcon") as Texture2D;
	topIcon=Resources.Load("cursorTop") as Texture2D;
	bottomIcon=Resources.Load("cursorBottom") as Texture2D;
	leftIcon=Resources.Load("cursorLeft") as Texture2D;
	rightIcon=Resources.Load("cursorRight") as Texture2D;
	
	//Resize GUI according to screen size/orientation 
	if(screenY>=screenX){
		dot=screenY/800;
	}else{
		dot=screenX/800;
	}
}

function Start () {

	//Setting variables values on Start
	gpsFix=false;
	rect = Rect (screenX/10, screenY/10,8*screenX/10, 8*screenY/10);
	topCursorPos = Rect(screenX/2-25*dot,0,50*dot,50*dot);
	rightCursorPos = Rect(screenX-50*dot,screenY/2-25*dot,50*dot,50*dot);
	if(!buttons)
		bottomCursorPos = Rect(screenX/2-25*dot,screenY-50*dot,50*dot,50*dot);
	else
		bottomCursorPos = Rect(screenX/2-25*dot,screenY-50*dot-screenY/12,50*dot,50*dot);
	leftCursorPos = Rect(0,screenY/2-25*dot,50*dot,50*dot);
	mymap.eulerAngles.y=180;
	initPointerSize=user.localScale.x;
	user.position=Vector3(0,user.position.y,0);
	
	//Initial Camera Settings
	//3D 
	if(triDView){
		mycam.orthographic=false;
		pinchToZoom=false;
		dragToPan=false;
		//Set the camera's field of view according to Screen size so map's visible area is maximized.
		if(screenY>screenX){
			mycam.fieldOfView=72.5;
		}else{
			mycam.fieldOfView=95-(28*(screenX*1.0/screenY*1.0));
		}
	}
	//2D
	else{
		mycam.orthographic=true;
		mycam.nearClipPlane=0.1;
		mycam.farClipPlane=cam.position.y+1;	
		if(screenY>=screenX){
			mycam.orthographicSize=mymap.localScale.z*5.0;
		}else{
			mycam.orthographicSize=(screenY/screenX)*mymap.localScale.x*5.0;		
		}
	}
	
	//The "ready" variable will be true when the map texture has been successfully loaded.
	ready=false; 
	
	//STARTING LOCATION SERVICES
    // First, check if user has location service enabled
    if (!Input.location.isEnabledByUser){
    	//This message prints to the Editor Console
    	print("Please enable location services and restart the App");
    	//You can use this "status" variable to show messages in your custom user interface (GUIText, etc.)
    	status="Please enable location services\n and restart the App";
		yield WaitForSeconds(4);
		Application.Quit();
		return;
    }

    // Start service before querying location
    Input.location.Start (3,3); 
    Input.compass.enabled=true;
	print("Initializing Location Services..");
	status="Initializing Location Services..";

    // Wait until service initializes
    while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
        yield WaitForSeconds (1);
        maxWait--;
    }

    // Service didn't initialize in 30 seconds
    if (maxWait < 1) {
    	print("Unable to initialize location services.\nPlease check your location settings and restart the App");
		status="Unable to initialize location services.\nPlease check your location settings\n and restart the App";
		yield WaitForSeconds(4);
		Application.Quit();
        return;
    }

    // Connection has failed
    if (Input.location.status == LocationServiceStatus.Failed) {
    	print("Unable to determine your location.\nPlease check your location setting and restart this App");
		status="Unable to determine your location.\nPlease check your location settings\n and restart this App";
		yield WaitForSeconds(4);
		Application.Quit();
        return;
    }
    
    // Access granted and location value could be retrieved
    else {
    	if(!mapDisabled){
    		print("GPS Fix established. Setting position..");
			status="GPS Fix established!\n Setting position ...";
		}
		else{
			print("GPS Fix established.");
			status="GPS Fix established!";
		}
				
        if(!simGPS){
        	//Wait in order to find enough satellites and increase GPS accuracy
        	yield WaitForSeconds(initTime);
        	//Set position
        	loc  = Input.location.lastData;          
        	iniRef.x = ((loc.longitude * 20037508.34 / 180)/100);
   			iniRef.z = System.Math.Log(System.Math.Tan((90 + loc.latitude) * System.Math.PI / 360)) / (System.Math.PI / 180);
  			iniRef.z = ((iniRef.z * 20037508.34 / 180)/100);  
  			iniRef.y = 0;
  			fixLon=loc.longitude;
    		fixLat=loc.latitude; 
    		//Successful GPS fix
    		gpsFix=true;
    		//Update Map for the current location
    		MapPosition();
  		}  
  		else{
  			//Simulate initialization time
  			yield WaitForSeconds(initTime);
  			//Set Position
  			iniRef.x = ((fixLon * 20037508.34 / 180)/100);
   			iniRef.z = System.Math.Log(System.Math.Tan((90 + fixLat) * System.Math.PI / 360)) / (System.Math.PI / 180);
  			iniRef.z = ((iniRef.z * 20037508.34 / 180)/100);  
  			iniRef.y = 0;
  			//Simulated successful GPS fix
  			gpsFix=true;
  			//Update Map for the current location
  			MapPosition();
  		}    
    }
    //Rescale map, set new camera height, and resize user pointer according to new zoom level
    ReScale(); 
}

//Set player's position using new location data (every "updateRate" seconds)
//Default value for updateRate is 0.1. Increase if necessary to improve performance
InvokeRepeating("MyPosition",1,updateRate); 

function MyPosition(){
	if(gpsFix){
		if(!simGPS){
			loc  = Input.location.lastData;
			newUserPos.x = ((loc.longitude * 20037508.34 / 180)/100)-iniRef.x;
			newUserPos.z = System.Math.Log(System.Math.Tan((90 + loc.latitude) * System.Math.PI / 360)) / (System.Math.PI / 180);
	    	newUserPos.z = ((newUserPos.z * 20037508.34 / 180)/100)-iniRef.z;   
	    	dmsLat=convertdmsLat(loc.latitude);
			dmsLon=convertdmsLon(loc.longitude);
			userLon=loc.longitude;
			userLat=loc.latitude;
		}
		else{
			userLon= (18000 * (user.position.x+iniRef.x))/20037508.34;
		    userLat= ((360/Mathf.PI)*Mathf.Atan(Mathf.Exp(0.00001567855943*(user.position.z+iniRef.z))))-90;
			dmsLat=convertdmsLat(userLat);
			dmsLon=convertdmsLon(userLon);
		}
	}	
} 

//Read incoming compass data (every 0.1s)
InvokeRepeating("Orientate",1,0.1);
function Orientate(){
	if(!simGPS && gpsFix){
		heading=Input.compass.trueHeading;
	}
	else{
		heading=user.eulerAngles.y;
	}
}
 
//Get altitude and horizontal accuracy readings using new location data (Default: every 2s)
InvokeRepeating("AccuracyAltitude",1,2);
function AccuracyAltitude(){
	if(gpsFix)
		altitude=loc.altitude;
		accuracy=loc.horizontalAccuracy;
}

//Auto-Center Map on 2D View Mode 
InvokeRepeating("Check",1,0.2);
function Check(){
	if(autoCenter && triDView==false){
		if(ready==true && mapping==false && gpsFix){
			if (rect.Contains(Vector2.Scale(mycam.WorldToViewportPoint (user.position),Vector2(screenX,screenY)))){
				//DoNothing
			}
			else{
				centering=true;
				MapPosition();
				ReScale();	
			}
		}
	}
}

//Auto-Center Map on 3D View Mode when exiting map's collider
function OnTriggerExit(other:Collider){
	if(other.tag=="Player" && autoCenter && triDView){
		MapPosition();
		ReScale();
	}
}

//Update Map with the corresponding map images for the current location ============================================
function MapPosition(){

	//The mapping variable will only be true while the map is being updated
	mapping=true;
	
	CursorsOff();
	
	//CHECK GPS STATUS AND RESTART IF NEEDED
	
	if (Input.location.status == LocationServiceStatus.Stopped || Input.location.status == LocationServiceStatus.Failed){
   		// Start service before querying location
   		Input.location.Start (3,3);

    	// Wait until service initializes
   		var maxWait : int = 20;
   		while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
   			yield WaitForSeconds (1);
   			maxWait--;
    	}

    	// Service didn't initialize in 20 seconds
    	if (maxWait < 1) {
        	print ("Timed out");
        	//use the status string variable to print messages to your own user interface (GUIText, etc.)
        	status="Timed out";
        	return;
    	}

    	// Connection has failed
    	if (Input.location.status == LocationServiceStatus.Failed) {
        	print ("Unable to determine device location");
        	//use the status string variable to print messages to your own user interface (GUIText, etc.)
        	status="Unable to determine device location";
        	return;
    	}
    
	}
	
   //------------------------------------------------------------------	//
   
	www=null; 
	//Get last available location data
	loc  = Input.location.lastData;
	//Make player invisible while updating map
	user.gameObject.renderer.enabled=false;
	
	
	//Set target latitude and longitude
	if(triDView){
		if(simGPS){
			fixLon= (18000 * (user.position.x+iniRef.x))/20037508.34;
		    fixLat= ((360/Mathf.PI)*Mathf.Atan(Mathf.Exp(0.00001567855943*(user.position.z+iniRef.z))))-90;	
		}else{
			fixLon=loc.longitude;
    		fixLat=loc.latitude;
    	}
	}else{
		if(centering){
			if(simGPS){
				fixLon= (18000 * (user.position.x+iniRef.x))/20037508.34;
		    	fixLat= ((360/Mathf.PI)*Mathf.Atan(Mathf.Exp(0.00001567855943*(user.position.z+iniRef.z))))-90;	
			}else{
				fixLon=loc.longitude;
		    	fixLat=loc.latitude;
			}
		}
		else{
			if(borderTile==0){
				fixLat= ((360/Mathf.PI)*Mathf.Atan(Mathf.Exp(0.00001567855943*(cam.position.z+iniRef.z))))-90;	
				fixLon= (18000 * (cam.position.x+iniRef.x))/20037508.34;
			}
			//North tile
			if (borderTile==1){
				fixLat= ((360/Mathf.PI)*Mathf.Atan(Mathf.Exp(0.00001567855943*(cam.position.z+3*mycam.orthographicSize/2+iniRef.z))))-90;	
				fixLon= (18000 * (cam.position.x+iniRef.x))/20037508.34;
				borderTile=0;	
				tileTop=false;
			}
			//East Tile
			if (borderTile==2){
				fixLat= ((360/Mathf.PI)*Mathf.Atan(Mathf.Exp(0.00001567855943*(cam.position.z+iniRef.z))))-90;	
				fixLon= (18000 * (cam.position.x+3*(screenX*mycam.orthographicSize/screenY)/2+iniRef.x))/20037508.34;
				borderTile=0;
			}
			//South Tile
			if (borderTile==3){
				fixLat= ((360/Mathf.PI)*Mathf.Atan(Mathf.Exp(0.00001567855943*(cam.position.z-3*mycam.orthographicSize/2+iniRef.z))))-90;	
				fixLon= (18000 * (cam.position.x+iniRef.x))/20037508.34;
				borderTile=0;
			}
			//West Tile
			if (borderTile==4){
				fixLat= ((360/Mathf.PI)*Mathf.Atan(Mathf.Exp(0.00001567855943*(cam.position.z+iniRef.z))))-90;	
				fixLon= (18000 * (cam.position.x-3*(screenX*mycam.orthographicSize/screenY)/2+iniRef.x))/20037508.34;
				borderTile=0;
			}
		}
	}
	
	//MAPQUEST=========================================================================================

	//Build a valid MapQuest OpenMaps tile request for the current location
	multiplier=mapSize[indexSize]/640.0;  //Tile Size= 640*multiplier
	//ATENTTION: If you want to implement maps from a different tiles provider, modify the following url accordingly to create a valid request
	url="http://open.mapquestapi.com/staticmap/v4/getmap?key="+key+"&size="+mapSize[indexSize].ToString()+","+mapSize[indexSize].ToString()+"&zoom="+zoom+"&type="+maptype[index]+"&center="+fixLat+","+fixLon+"&scalebar=false";
	tempLat = fixLat; 
	tempLon = fixLon;

	//=================================================================================================

	//Proceed with download if an Wireless internet connection is available 
	if(Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork){
		Online();
	}	
  	//Proceed with download if a 3G/4G internet connection is available 
	else if(Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork){
   		Online();
	}
	//No internet connection is available. Switching to Offline mode.	 
	else{
		Offline();
	}	
}

//ONLINE MAP DOWNLOAD
function Online(){
	if(!mapDisabled){
		// Start a download of the given URL
		www = new WWW(url); 
		// Wait for download to complete
		download = (www.progress);
		while(!www.isDone){
			print("Updating map "+System.Math.Round(download*100)+" %");
			//use the status string variable to print messages to your own user interface (GUIText, etc.)
			status="Updating map "+System.Math.Round(download*100)+" %";
			yield;
		}
		//Show download progress and apply texture
		if(www.error==null){
			print("Updating map 100 %");
			print("Map Ready!");
			//use the status string variable to print messages to your own user interface (GUIText, etc.)
			status="Updating map 100 %\nMap Ready!";
			yield WaitForSeconds (0.5);
			maprender.material.mainTexture=null;
			var tmp : Texture2D;
			tmp = new Texture2D(1280,1280,TextureFormat.RGB24,false);
			maprender.material.mainTexture = tmp;
			www.LoadImageIntoTexture(tmp); 	
		}
		//Download Error. Switching to offline mode
		else{
			print("Map Error:"+www.error);
			//use the status string variable to print messages to your own user interface (GUIText, etc.)
			status="Map Error:"+www.error;
			yield WaitForSeconds (1);
			maprender.material.mainTexture=null;
			Offline();
		}
		maprender.enabled=true;
	}
	ReSet();
	user.gameObject.renderer.enabled=true;
	ready=true;
	mapping=false;
	
}

//USING OFFLINE BACKGROUND TEXTURE
function Offline(){
	if(!mapDisabled){
		maprender.material.mainTexture=Resources.Load("offline") as Texture2D;
		maprender.enabled=true;
	}
	ReSet();
	ready=true;
	mapping=false;
	user.gameObject.renderer.enabled=true;
	
}


//Re-position map and camera using updated data
function ReSet(){
	transform.position.x = ((tempLon * 20037508.34 / 180)/100)-iniRef.x;
	transform.position.z = System.Math.Log(System.Math.Tan((90 + tempLat) * System.Math.PI / 360)) / (System.Math.PI / 180);
	transform.position.z = ((transform.position.z * 20037508.34 / 180)/100)-iniRef.z; 
	if(!freeCam){
		cam.position.x = transform.position.x;
		cam.position.z = transform.position.z;
	}
	if(triDView==false && centering){
		centered=true;
		autoCenter=true;
		centering=false;
	}
}

//RE-SCALE =========================================================================================================
function ReScale(){
	while(mapping){
		yield;
	}
	//Rescale map according to new zoom level to maintain 1:100 scale
	mymap.localScale.x=multiplier*100532.244/(Mathf.Pow(2,zoom));
	mymap.localScale.z=transform.localScale.x;
	
	//3D View. Free/custom camera
	if(triDView && freeCam){
		//Do Nothing
	}
	
	//3D View and Camera follows player. Set camera position
	else if(triDView && !freeCam){
		cam.localPosition.z=-(65536*camDist*Mathf.Cos(camAngle*Mathf.PI/180))/Mathf.Pow(2,zoom);
		cam.localPosition.y=65536*camDist*Mathf.Sin(camAngle*Mathf.PI/180)/Mathf.Pow(2,zoom);	
	}
	
	//2D View. Set camera position 
	else{
		if(firstTime){
			cam.localEulerAngles=Vector3(90,0,0);
			if(screenY>=screenX){
				mycam.orthographicSize=mymap.localScale.z*5.0*0.75;
			}else{
				mycam.orthographicSize=(screenY/screenX)*mymap.localScale.x*5.0*0.75;		
			}
		}
		firstTime=false;
		if(screenY>=screenX){
			targetOrtoSize= Mathf.Round(mymap.localScale.z*5.0*100.0)/100.0;
		}else{
			targetOrtoSize=Mathf.Round((screenY/screenX)*mymap.localScale.x*5.0*100.0)/100.0;		
		}
		
		while(Mathf.Abs(mycam.orthographicSize-targetOrtoSize*0.625)>0.01){
		currentOrtoSize = mycam.orthographicSize;
		currentOrtoSize = Mathf.MoveTowards (currentOrtoSize,targetOrtoSize*0.625,2.5*32768*Time.deltaTime/Mathf.Pow(2,zoom));
		mycam.orthographicSize = currentOrtoSize;
		yield;
		}
		
		//Drag to pan speed according to zoom level
		dragSpeed=mycam.orthographicSize/10;
	}
	

}

function Update(){

	//Rename GUI "center" button label
	if(!triDView){
		if(cam.position.x!=user.position.x || cam.position.z != user.position.z)
			centre="center";
		else
			centre="refresh";
	}
	
    //User pointer speed
    if(realSpeed){
		speed = userSpeed*0.05;
	}
	else{
		speed = speed=userSpeed*10000/(Mathf.Pow(2,zoom)*1.0);
	}
	
	//3D-2D View Camera Toggle (use only while game is stopped) 
	if(triDView && !freeCam){
		cam.parent=user;
		if(ready)
			cam.LookAt(user);
	}	
    
    if(ready){	
    	if(!simGPS){
    		//Smoothly move pointer to updated position
    		currentUserPos.x = user.position.x;
			currentUserPos.x = Mathf.Lerp (user.position.x, newUserPos.x, 2.0 * Time.deltaTime);
			user.position.x = currentUserPos.x;
         
    		currentUserPos.z = user.position.z;
			currentUserPos.z = Mathf.Lerp (user.position.z, newUserPos.z, 2.0 * Time.deltaTime);
			user.position.z = currentUserPos.z; 
			
			//Update rotation
			if(System.Math.Abs(user.eulerAngles.y-heading)>=5){
    			var newAngle : float = Mathf.SmoothDampAngle(user.eulerAngles.y,heading,yVelocity, smooth);
				user.eulerAngles.y = newAngle;
			}
		}
		
		else{
			//When GPS Emulator is enabled, user position is controlled by keyboard input.
			if(mapping==false){
				//Use keyboard input to move the player
			    if (Input.GetKey ("up") || Input.GetKey ("w")){
					user.transform.Translate(Vector3.forward * speed * Time.deltaTime);
				}
				if (Input.GetKey ("down") || Input.GetKey ("s")){
					user.transform.Translate(-Vector3.forward * speed * Time.deltaTime);
				}
				//rotate pointer when pressing Left and Right arrow keys
				user.Rotate(Vector3.up, Input.GetAxis("Horizontal") * 80 * Time.deltaTime);
			}
		}	
	}
	
	if(mapping && !mapDisabled){
		//get download progress while images are still downloading
		if(www!=null){download = www.progress;}
	}	
	
	//Enable/Disable map renderer 
	if(mapDisabled){
		maprender.enabled=false;
	}else{
		maprender.enabled=true;
	}	
	
	//PINCH TO ZOOM ================================================================================================
	if(pinchToZoom){
		if(Input.touchCount == 2 && mapping==false){
			touch = Input.GetTouch(0);
			touch2 = Input.GetTouch(1);
			
			if(touch.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began){
				focusScreenPoint = (touch.position+touch2.position)/2;
				focusWorldPoint = mycam.ScreenToWorldPoint(Vector3(focusScreenPoint.x,focusScreenPoint.y,cam.position.y));
			}
			
			if(touch.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved){
				touchZoom=true;
				curDist = touch.position - touch2.position;
				prevDist = (touch.position - touch.deltaPosition) - (touch2.position - touch2.deltaPosition);
				actualDist = prevDist.magnitude - curDist.magnitude;
			}else{
				touchZoom=false;
			}
		}
	}
	if(touchZoom){								
																
		//Modify camera orthographic size
		mycam.orthographicSize = mycam.orthographicSize + actualDist*Time.deltaTime*mycam.orthographicSize/30;
		mycam.orthographicSize=Mathf.Clamp(mycam.orthographicSize,3*targetOrtoSize/8,targetOrtoSize);
		
		if(actualDist<0){
			currentPosition.x = cam.position.x;
			currentPosition.x = Mathf.MoveTowards (currentPosition.x,focusWorldPoint.x,-actualDist*0.7*32768*Time.deltaTime/Mathf.Pow(2,zoom));
			cam.position.x = currentPosition.x;
			currentPosition.z = cam.position.z;
			currentPosition.z = Mathf.MoveTowards (currentPosition.z,focusWorldPoint.z,-actualDist*0.7*32768*Time.deltaTime/Mathf.Pow(2,zoom));
			cam.position.z = currentPosition.z;
		}
		else if (actualDist==0){
			//Do nothing
		}
		else{
			currentPosition.x = cam.position.x;
			currentPosition.x = Mathf.MoveTowards (currentPosition.x,mymap.position.x,actualDist*0.7*32768*Time.deltaTime/Mathf.Pow(2,zoom));
			cam.position.x = currentPosition.x;
			currentPosition.z = cam.position.z;
			currentPosition.z = Mathf.MoveTowards (currentPosition.z,mymap.position.z,actualDist*0.7*32768*Time.deltaTime/Mathf.Pow(2,zoom));
			cam.position.z = currentPosition.z;
		}
		
		
		//Get touch drag speed for new zoom level
		dragSpeed=mycam.orthographicSize/10;
		
		//Clamp the camera position to avoid displaying any off the map areas
       	ClampCam();
       	CursorsOff();
				
		//Decrease zoom level
		if(Mathf.Round(mycam.orthographicSize*1000.0)/1000.0 >= Mathf.Round(targetOrtoSize*1000.0)/1000.0 && zoom>minZoom){
			if(!mapping){
				touchZoom=false;
				zoom=zoom-1;
				MapPosition();
				ReScale();
			}
		}
		//Increase zoom level
		if(Mathf.Round(mycam.orthographicSize*1000.0)/1000.0 <= Mathf.Round((3*targetOrtoSize/8)*1000.0)/1000.0 && zoom<maxZoom){
			if(!mapping){
				touchZoom=false;
				zoom=zoom+1;
				MapPosition();
				ReScale();
			}
		}
	}
	
	//DRAG TO PAN ==================================================================================================
	if(dragToPan){
		if(!mapping && ready){
			if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved) {
				autoCenter=false;
				centered=false;
				if(Input.GetTouch(0).position.y>screenY/12){
		        	var touchDeltaPosition : Vector2  = Input.GetTouch(0).deltaPosition;
		        	
		        	//Reached left tile border
		            if(Mathf.Round((mycam.ScreenToWorldPoint(Vector3(0,0.5,cam.position.y)).x)*100.0)/100.0 <= Mathf.Round((mymap.position.x-mymap.localScale.x*5)*100.0)/100.0){
		            	//show button for borderTile=4;
		            	tileLeft=true;
		            }else{
		            	//hide button
		            	tileLeft=false;
		            }
		            //Reached right tile border
		            if(Mathf.Round((mycam.ScreenToWorldPoint(Vector3(mycam.pixelWidth,0.5,cam.position.y)).x)*100.0)/100.0 >= Mathf.Round((mymap.position.x+mymap.localScale.x*5)*100.0)/100.0){
		            	//show button for borderTile=2;
		            	tileRight=true;
		            }else{
		            	//hide button
		            	tileRight=false;
		            }
		            //Reached bottom tile border
		            if(Mathf.Round((mycam.ScreenToWorldPoint(Vector3(0.5,0,cam.position.y)).z)*100.0)/100.0 <= Mathf.Round((mymap.position.z-mymap.localScale.z*5)*100.0)/100.0){
		            	//show button for borderTile=3;
		            	tileBottom=true;
		            }else{
		            	//hide button
		            	tileBottom=false;
		            }
		            //Reached top tile border
		            if(Mathf.Round((mycam.ScreenToWorldPoint(Vector3(0.5,mycam.pixelHeight,cam.position.y)).z)*100.0)/100.0 >= Mathf.Round((mymap.position.z+mymap.localScale.z*5)*100.0)/100.0){
		            	//show button for borderTile=1;
		            	tileTop=true;
		            }else{
		            	//hide button
		            	tileTop=false;
		            }
		            
		           	cam.Translate(-touchDeltaPosition.x * dragSpeed * Time.deltaTime, -touchDeltaPosition.y * dragSpeed * Time.deltaTime, 0);
		           	
		           	//Clamp the camera position to avoid displaying any off the map areas
		           	ClampCam();
				}
			}
		}	
	}																																
}

//Disable surrounding tiles cursors
function CursorsOff(){
	tileTop=false;
    tileBottom=false;
	tileLeft=false;
	tileRight=false;
}

//Clamp the camera position
function ClampCam(){
	cam.position.x=Mathf.Clamp(cam.position.x,
							mymap.position.x-(mymap.localScale.x*5)+(mycam.ScreenToWorldPoint(Vector3(mycam.pixelWidth,0.5,cam.position.y)).x - mycam.ScreenToWorldPoint(Vector3(0,0.5,cam.position.y)).x)/2,
							mymap.position.x+(mymap.localScale.x*5)-(mycam.ScreenToWorldPoint(Vector3(mycam.pixelWidth,0.5,cam.position.y)).x - mycam.ScreenToWorldPoint(Vector3(0,0.5,cam.position.y)).x)/2 );
	cam.position.z=Mathf.Clamp(cam.position.z,
							mymap.position.z-(mymap.localScale.z*5)+(mycam.ScreenToWorldPoint(Vector3(0.5,mycam.pixelHeight,cam.position.y)).z - mycam.ScreenToWorldPoint(Vector3(0.5,0,cam.position.y)).z)/2,
							mymap.position.z+(mymap.localScale.z*5)-(mycam.ScreenToWorldPoint(Vector3(0.5,mycam.pixelHeight,cam.position.y)).z - mycam.ScreenToWorldPoint(Vector3(0.5,0,cam.position.y)).z)/2 );				
}

//SAMPLE USER INTERFACE. MODIFY OR EXTEND IF NECESSARY =============================================================
function OnGUI () {
	GUI.skin.box.alignment = TextAnchor.MiddleCenter;
	GUI.skin.box.font= Resources.Load("Neuropol");
	GUI.skin.box.normal.background=Resources.Load("grey");
	if(Screen.width>=Screen.height){
		GUI.skin.button.fontSize=Mathf.Round(10*Screen.width/480);
		GUI.skin.box.fontSize=Mathf.Round(10*Screen.width/320);
	}
	else{	
		GUI.skin.button.fontSize=Mathf.Round(10*Screen.height/480);
		GUI.skin.box.fontSize=Mathf.Round(10*Screen.height/320);
	}	
	
	//Display Updating Map message
	if(ready && mapping){
		GUI.Box (Rect (0,screenY-screenY/12,screenX,screenY/12), "Updating...");
	}
	
	//Display button to center camera at user position if GUI buttons are not enabled
	if (ready && !mapping && !buttons && !centered){	
		if (GUI.Button(Rect(10*dot,screenY-74*dot,64*dot,64*dot), centerIcon)){
			centering=true;
			MapPosition();
			ReScale();
		}
	}
	
	//Display surrounding tiles buttons 
	if (ready && !mapping){	
		if(tileTop){
			GUI.DrawTexture(topCursorPos , topIcon);
			if (GUI.Button(topCursorPos, "","label")){
				borderTile=1;
				MapPosition();
				ReScale();
			}
		}
		if(tileRight){
			GUI.DrawTexture(rightCursorPos , rightIcon);
			if (GUI.Button(rightCursorPos, "","label")){
				borderTile=2;
				MapPosition();
				ReScale();
			}
		} 
		if(tileBottom){
			GUI.DrawTexture(bottomCursorPos , bottomIcon);
			if (GUI.Button(bottomCursorPos, "","label")){
				borderTile=3;
				MapPosition();
				ReScale();
			}
		} 
		if(tileLeft){
			GUI.DrawTexture(leftCursorPos , leftIcon);
			if (GUI.Button(leftCursorPos, "","label")){
				borderTile=4;
				MapPosition();
				ReScale();
			}
		} 
		
	}
	
	if (ready && !mapping && buttons){
		GUI.BeginGroup (Rect (0,screenY-screenY/12, screenX, screenY/12));
			
		GUI.Box (Rect (0,0,screenX,screenY/12), "");
		
		//Map type toggle button
		if (GUI.Button(Rect(0,0,screenX/5,screenY/12), maptype[index])){
			if(mapping==false){
				if(index<maptype.Length-1)
		    		index=index+1;
		    	else
		    		index=0;	
		     	MapPosition();
		     	ReScale();
			}    
		}
		//3D Zoom Buttons
		if(triDView){
			//Zoom In button
			if(GUI.Button(Rect(2*screenX/5,0,screenX/5,screenY/12), "zoom +")){
				if(zoom<maxZoom){
					zoom=zoom+1;
					MapPosition();
					ReScale();
				}
			}
			//Zoom Out button
			if(GUI.Button(Rect(screenX/5,0,screenX/5,screenY/12), "zoom -")){
				if(zoom>minZoom){
					zoom=zoom-1;
					MapPosition();
					ReScale();
				}
			}
		//2D Zoom Buttons
		}else{
			//Zoom In button
			if(GUI.RepeatButton(Rect(2*screenX/5,0,screenX/5,screenY/12), "zoom +")){
				if(Input.GetMouseButton(0)){
					currentOrtoSize = mycam.orthographicSize;
					currentOrtoSize = Mathf.MoveTowards (currentOrtoSize,3*targetOrtoSize/8,5*32768*Time.deltaTime/Mathf.Pow(2,zoom));
					mycam.orthographicSize = currentOrtoSize;
					
					//Clamp the camera position to avoid displaying any off the map areas
					ClampCam();
					CursorsOff();
					
					//Get touch drag speed for new zoom level
					dragSpeed=mycam.orthographicSize/10;
					//Increase zoom level
					if(Mathf.Round(mycam.orthographicSize*1000.0)/1000.0 <= Mathf.Round((3*targetOrtoSize/8)*1000.0)/1000.0 && zoom<maxZoom){
						if(!mapping){
							zoom=zoom+1;
							MapPosition();
							ReScale();
						}
					}
				}
			}
			//Zoom Out button
			if (GUI.RepeatButton(Rect(screenX/5,0,screenX/5,screenY/12), "zoom -")){
				if(Input.GetMouseButton(0)){
					currentOrtoSize = mycam.orthographicSize;
					currentOrtoSize = Mathf.MoveTowards (currentOrtoSize,targetOrtoSize,5*32768*Time.deltaTime/Mathf.Pow(2,zoom));
					mycam.orthographicSize = currentOrtoSize;
					
					//Center camera on map as we zoom out
					currentPosition.x = cam.position.x;
					currentPosition.x = Mathf.MoveTowards (currentPosition.x,mymap.position.x,10*32768*Time.deltaTime/Mathf.Pow(2,zoom));
					cam.position.x = currentPosition.x;
					currentPosition.z = cam.position.z;
					currentPosition.z = Mathf.MoveTowards (currentPosition.z,mymap.position.z,10*32768*Time.deltaTime/Mathf.Pow(2,zoom));
					cam.position.z = currentPosition.z;
					
					//Clamp the camera position to avoid displaying any off the map areas
					ClampCam();
					CursorsOff();
					//Get touch drag speed for new zoom level
					dragSpeed=mycam.orthographicSize/10;
						
					//Decrease zoom level
					if(Mathf.Round(mycam.orthographicSize*1000.0)/1000.0 >= Mathf.Round(targetOrtoSize*1000.0)/1000.0 && zoom>minZoom){
						if(!mapping){
							zoom=zoom-1;
							MapPosition();
							ReScale();
						}
					}
				}
			}	
		}
		//Update map and center user position 
		if (GUI.Button(Rect(3*screenX/5,0,screenX/5,screenY/12), centre)){
			centering=true;
			MapPosition();
			ReScale();
		}
		//Show GPS Status info. Please make sure the GPS-Status.js script is attached and enabled in the map object.
		if (GUI.Button(Rect(4*screenX/5,0,screenX/5,screenY/12), "info")){
			if(info)
				info=false;
			else
				info=true;
		}
		GUI.EndGroup ();
	}
}

//Translate decimal latitude to Degrees Minutes and Seconds
function convertdmsLat( lat : float) : String{
	var latAbs = Mathf.Abs(Mathf.Round(lat * 1000000));
    var result : String;
    result = (Mathf.Floor(latAbs / 1000000) + '° '
    		 + Mathf.Floor(((latAbs/1000000) - Mathf.Floor(latAbs/1000000)) * 60)  + '\' '
    	     + (Mathf.Floor(((((latAbs/1000000) - Mathf.Floor(latAbs/1000000)) * 60) - Mathf.Floor(((latAbs/1000000) - Mathf.Floor(latAbs/1000000)) * 60)) * 100000) *60/100000 ).ToString("F2") + '" ')+ ((lat > 0) ? "N" : "S");
	return result;
}   
//Translate decimal longitude to Degrees Minutes and Seconds  
function convertdmsLon( lon:float):String{
	var lonAbs = Mathf.Abs(Mathf.Round(lon * 1000000));
    var result:String; 
    result = (Mathf.Floor(lonAbs / 1000000) + '° ' 
      		 + Mathf.Floor(((lonAbs/1000000) - Mathf.Floor(lonAbs/1000000)) * 60)  + '\' ' 
      		 + (Mathf.Floor(((((lonAbs/1000000) - Mathf.Floor(lonAbs/1000000)) * 60) - Mathf.Floor(((lonAbs/1000000) - Mathf.Floor(lonAbs/1000000)) * 60)) * 100000) *60/100000 ).ToString("F2") + '" ' + ((lon > 0) ? "E" : "W") );
	return result;
}   