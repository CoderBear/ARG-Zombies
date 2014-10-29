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
		else {
#if UNITY_ANDROID && !UNITY_EDITOR
			StartCoroutine("CheckLocationServices");
#else
			Application.LoadLevel("gameMapOnline");
#endif
		}
	}
	
	IEnumerator CheckLocationServices() {
		if(Input.location.status == LocationServiceStatus.Stopped) {
			Input.location.Start();
			while(Input.location.status == LocationServiceStatus.Initializing) {
				yield return new WaitForSeconds(1);
			}
			if(Input.location.status == LocationServiceStatus.Running) {
				print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
				Application.LoadLevel("gameMapOnline");
			}
			else if (Input.location.status == LocationServiceStatus.Failed) {
				print("Unable to determine device location");
				yield return null;
			}
		} else {
			Application.LoadLevel("gameMapOnline");
		}
	}
}