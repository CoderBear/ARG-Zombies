#pragma strict
@script AddComponentMenu ("MAPNAV/FixQuadAspect")
//MapNav 2D GameObject Resize Tool (Fixes object screen aspect regardless of zoom level)
//Use only if this object mesh is a QUAD on 2D view mode.

private var mycam : Camera;
private var initScale : Vector3;
private var mytransform : Transform;
private var lastOrthoSize : float;

function Awake () {
	mycam = GameObject.FindGameObjectWithTag("MainCamera").camera;
	initScale = transform.localScale;
	mytransform = transform;
}

function Update () {
	if(mycam.orthographicSize!=lastOrthoSize){
	 	
	 	//Resize game object according to camera orthographic size (zoom level).
		// Set initScale using the transform Scale properties in the inspector
	 	mytransform.localEulerAngles.x = 90;
	 	mytransform.localEulerAngles.z = 0;
		mytransform.localScale.x = initScale.x/9.594413 * mycam.orthographicSize;
		mytransform.localScale.y = initScale.y/9.594413 * mycam.orthographicSize;
	}
	lastOrthoSize = mycam.orthographicSize;
}