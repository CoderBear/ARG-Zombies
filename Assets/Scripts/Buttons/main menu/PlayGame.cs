using UnityEngine;
using System.Collections;

public class PlayGame : MonoBehaviour {

	// Update is called once per frame
	void Update () {
	
	}

	void OnClick() {
		Application.LoadLevel ("gameMap");
	}
}