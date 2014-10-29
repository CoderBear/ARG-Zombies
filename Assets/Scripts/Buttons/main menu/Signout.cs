using UnityEngine;
using System.Collections;

public class Signout : MonoBehaviour {
	private string infoURL = "http://192.185.41.34/~codebear/argz/login.php";
	private const string verifyDB = "&dbuser=codebear_coder&dbpass=J29kMMX&dbtable=codebear_argz";

	private Player player;
	void Awake() {
		// set local private player object to the singleton player.
		player = GameObject.FindWithTag ("Player").GetComponent<Player>();
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (!player.OfflineMode) {
//			StartCoroutine(updateGameServerInfo());
				Input.location.Stop();
				Application.LoadLevel ("mainLogin");
			} else {
				Application.LoadLevel("mainLogin");
			}
		}
	}

	void OnClick() {
		if (!player.OfflineMode) {
//			StartCoroutine(updateGameServerInfo());
			Input.location.Stop();
			Application.LoadLevel ("mainLogin");
		} else {
			Application.LoadLevel ("mainLogin");
		}
	}

	IEnumerator updateGameServerInfo(){
		string info_URL = infoURL + verifyDB;
		Debug.Log (info_URL);
		WWW infoReader = new WWW (info_URL);
		yield return infoReader;
		Application.LoadLevel ("mainLogin");
	}
}