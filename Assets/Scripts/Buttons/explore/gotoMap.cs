using UnityEngine;
using System.Collections;

public class gotoMap : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	void OnClick() {
		Application.LoadLevel ("gameMap");
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape))
			Application.LoadLevel ("gameMap");
	}
}