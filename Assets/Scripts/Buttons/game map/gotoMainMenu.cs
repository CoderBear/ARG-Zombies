using UnityEngine;
using System.Collections;

public class gotoMainMenu : MonoBehaviour {

	private Player player;
	void Awake() {
		// set local private player object to the singleton player.
		player = GameObject.FindWithTag ("Player").GetComponent<Player>();
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape))
			Application.LoadLevel ("mainMenu");
	}

	void OnClick() {
		if (player.OfflineMode)
		{
			if (GameObject.Find("Game_Building_Controller(Clone)").GetComponent<offlineButtonSpawn>() != null)
			{
				offlineButtonSpawn obs = GameObject.Find("Game_Building_Controller(Clone)").GetComponent<offlineButtonSpawn>();
				obs.destroyThis();
			}
		} else {
			if (GameObject.Find("Online_Building_Controller(Clone)").GetComponent<onlineButtonSpawn>() != null)
			{
				onlineButtonSpawn obs = GameObject.Find("Online_Building_Controller(Clone)").GetComponent<onlineButtonSpawn>();
				obs.destroyThis();
			}
		}
		Application.LoadLevel ("mainMenu");
	}
}