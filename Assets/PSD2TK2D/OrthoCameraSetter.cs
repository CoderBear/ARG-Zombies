using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class OrthoCameraSetter : MonoBehaviour
{
	private new Camera camera;
	private new Transform transform;
	
	private static Vector2 screenSize_;
	public static Vector2 screenSize
	{
		get { return screenSize_; }
	}
	
	public int targetWidth = 1024;
	public int targetHeight = 768;
	
	void Start()
	{
		screenSize_ = new Vector2(this.targetWidth, this.targetHeight);
		var size = OrthoCameraSetter.screenSize;
		
		this.camera = base.camera;
		this.camera.orthographic = true;
		this.camera.far = 10000;
		this.camera.orthographicSize = size.y/2;
		
		this.transform = base.transform;
		this.transform.position = 
			new Vector3(size.x/2, size.y/2, -1500);
	}
	
#if UNITY_EDITOR
	void Update()
	{
		screenSize_ = new Vector2(this.targetWidth, this.targetHeight);
		var size = OrthoCameraSetter.screenSize;
		this.camera.orthographicSize = size.y/2;
		
		var pos = this.transform.position;
		pos.x = size.x/2;
		pos.y = size.y/2;
		this.transform.position = pos;
	}
#endif
};