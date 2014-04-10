using UnityEngine;
using System.Collections;

public class AcceptResultsButton : MonoBehaviour {
	Player player;

	void Awake() {
		// set local private player object to the singleton player.
		player = GameObject.FindWithTag ("Player").GetComponent<Player>();
	}
	void OnClick() {
//		player.hidePlayer ();
		Application.LoadLevel (player.LastAreaVisited);
	}
}