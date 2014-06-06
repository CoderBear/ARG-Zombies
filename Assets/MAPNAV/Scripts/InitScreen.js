#pragma strict
//MAPNAV Navigation ToolKit v.1.0

private var mapnav: MapNav;
private var initText: Transform;
private var initBackg: Transform; 

function Awake(){
	//Reference to the MapNav.js script and GUI elements
	mapnav=GameObject.FindGameObjectWithTag("GameController").GetComponent(MapNav);
	initText=transform.Find("GUIText");
	initBackg=transform.Find("GUITexture");
	//Set GUIText font size according to our device screen size
	initText.guiText.fontSize=Mathf.Round(15*Screen.width/320);
}

function Start(){
	//Initialization message
	initText.guiText.text= "Searching for satellites ...";
	
	//Enable initial screen
	initText.gameObject.SetActive(true);
	initBackg.gameObject.SetActive(true);
	initBackg.guiTexture.pixelInset.height=Screen.height;
	initBackg.guiTexture.pixelInset.width=Screen.width;
}

function Update () {
	if(!mapnav.ready){
		//Display GPS fix and maps download progress
		initText.guiText.text= mapnav.status;
	}
	else{
		//Clear messages once the map is ready
		initText.guiText.text= "";	
		
		//Disable initial screen
		initBackg.gameObject.SetActive(false);
		
		//Disable this script (no longer needed)
		this.enabled=false;
	}
}
