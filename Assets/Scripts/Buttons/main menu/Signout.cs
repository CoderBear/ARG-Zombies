using UnityEngine;
using System.Collections;

public class Signout : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape))
			Application.LoadLevel ("mainLogin");
	}

	void OnClick() {
		Application.LoadLevel ("mainLogin");
	}
}