#pragma strict
private var target : Transform;
private var mapnav : MapNav;
private var screenX : float;
private var screenY : float;
private var dot : float;

function Awake(){
	target= transform.parent.transform;
	mapnav=GameObject.FindGameObjectWithTag("GameController").GetComponent(MapNav);
	screenX=Screen.width;
	screenY=Screen.height;
	if(screenY>=screenX){
		dot=screenY/800;
	}else{
		dot=screenX/800;
	}
}

function Start(){
	transform.parent.renderer.enabled=true;
}

function Update () {
	var screenPos : Vector3 = Camera.main.WorldToViewportPoint (target.position);
	if(!float.IsNaN(screenPos.x) && !float.IsNaN(screenPos.y)){
		transform.position.x = screenPos.x;
		transform.position.y = screenPos.y;
	}
	if(mapnav.mapping==false){
		if(mapnav.gpsFix && !guiText.enabled)
			guiText.enabled=true;	
		guiText.fontSize=180*dot/Camera.main.orthographicSize;
	}
}

