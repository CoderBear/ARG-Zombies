//MAPNAV Navigation ToolKit v.1.3.2

//MapNav 2D GameObject Resize Tool (Fixes object screen aspect regardless of zoom level)
//Use only if this object mesh is a QUAD on 2D view mode.

using UnityEngine;
using System.Collections;

[AddComponentMenu("MAPNAV/FixQuadAspect")]

public class FixQuadAspect : MonoBehaviour
{
    private Camera mycam;
    private Vector3 initScale;
    private Transform mytransform;
    private float lastOrthoSize;

    void Awake()
    {
        mycam = GameObject.FindGameObjectWithTag("MainCamera").camera;
        initScale = transform.localScale;
        mytransform = transform;
    }

	void Update () 
    {
		if(mycam.orthographicSize != lastOrthoSize)
        {
	 		//Resize game object according to camera orthographic size (zoom level).
			// Set initScale using the transform Scale properties in the inspector
            mytransform.localEulerAngles = new Vector3(90, mytransform.localEulerAngles.y, 0);
			mytransform.localScale = new Vector3(initScale.x/9.594413f*mycam.orthographicSize, initScale.y/9.594413f*mycam.orthographicSize, mytransform.localScale.z);
		}
		lastOrthoSize = mycam.orthographicSize;
	}
}