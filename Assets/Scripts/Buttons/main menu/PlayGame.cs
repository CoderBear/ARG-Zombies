using UnityEngine;
using System.Collections;

public class PlayGame : MonoBehaviour {

	private Player player;
	void Awake() {
		// set local private player object to the singleton player.
		player = GameObject.FindWithTag ("Player").GetComponent<Player>();
	}

	// Update is called once per frame
	void Update () {
	}

	void OnClick() {
		if(player.OfflineMode)
			Application.LoadLevel ("gameMap");
		else
			Application.LoadLevel("gameMapOnline");
	}
}