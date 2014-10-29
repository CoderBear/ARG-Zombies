using UnityEngine;
using System.Collections;

public class offlineButton : MonoBehaviour {

	private Player player;
	void Awake() {
		// set local private player object to the singleton player.
		player = GameObject.FindWithTag ("Player").GetComponent<Player>();
	}

	void OnClick() {
		player.OfflineMode = true;
		Application.LoadLevel ("mainMenu");
	}
}