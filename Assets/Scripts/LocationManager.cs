using UnityEngine;
using System.Collections;

public class LocationManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Input.location.Start ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void onApplicationQuit() {
		Input.location.Stop ();
	}
}