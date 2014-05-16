using UnityEngine;
using System.Collections;

public class SmoothFollow2D : MonoBehaviour {

	[SerializeField] private Transform target;
	[SerializeField] private float smoothTime = 0.3f;

	private tk2dCamera tk2DCamera;
	private Transform thisTransform;
	private Vector3 velocity;
	// Use this for initialization
	void Start () {
		tk2DCamera = GetComponent<tk2dCamera> ();
		thisTransform = tk2DCamera.transform;
	}
	
	// Update is called once per frame
	void Update () {
		float x = Mathf.SmoothDamp (thisTransform.position.x, target.position.x, ref velocity.x, smoothTime);
		float y = Mathf.SmoothDamp (thisTransform.position.y, target.position.y, ref velocity.y, smoothTime);

		thisTransform.position.Set (x, y, thisTransform.position.z);
	}
}