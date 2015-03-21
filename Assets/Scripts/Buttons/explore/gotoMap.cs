using UnityEngine;
using System.Collections;

public class gotoMap : MonoBehaviour {
	Player player;

	void Awake () {
		// set local private player object to the singleton player.
		player = GameObject.FindWithTag ("Player").GetComponent<Player>();
	}

	void OnClick() {
//		if(Application.loadedLevelName == "gameCombat") {
		if(Application.loadedLevelName == "combatBuilding") {
			player.LastAreaVisited = "";
//			player.hidePlayer();
		}
		//player.RoomsLeft = player.RoomsTotal = 0;
		Application.LoadLevel ("gameMap");
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
//			if(Application.loadedLevelName == "gameCombat") {
			if(Application.loadedLevelName == "combatBuilding") {
				player.LastAreaVisited = "";
//				player.hidePlayer();
				player.RoomsLeft = player.RoomsTotal = 0;
				Application.LoadLevel ("gameMap");
			}
			if(Application.loadedLevelName == "gameBuilding01") {
				player.RoomsLeft = player.RoomsTotal = 0;
				Application.LoadLevel ("gameMap");
			}
		}
	}
}