using UnityEngine;
using System.Collections;

public class gotoMainMenu : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape))
			Application.LoadLevel ("mainMenu");
	}

	void OnClick() {
		if(GameObject.Find("Game_Building_Controller(Clone)").GetComponent<offlineButtonSpawn>() != null) {
			offlineButtonSpawn obs = GameObject.Find("Game_Building_Controller(Clone)").GetComponent<offlineButtonSpawn>();
			obs.destroyThis();
		}
		Application.LoadLevel ("mainMenu");
	}
}