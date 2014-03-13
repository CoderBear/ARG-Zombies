using UnityEngine;
using System.Collections;

public class gotoExplore : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnClick() {
		// Future: will read the GO tag and load the correct building type
		Application.LoadLevel ("gameBuilding01");
	}
}