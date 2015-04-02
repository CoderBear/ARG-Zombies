using UnityEngine;
using System.Collections;

public class mode_Obect_Loader : MonoBehaviour {

	public GameObject OnlineLoader, OfflineLoader;
	public UITexture MapTex;
	public UISprite SpriteOnline, SpriteOffline;
	private Player player;
	void Awake() {
		// set local private player object to the singleton player.
		player = GameObject.FindWithTag ("Player").GetComponent<Player>();
	}
	
	// Use this for initialization
	void Start () {
		if(player.OfflineMode) {
			OfflineLoader.SetActive(true);
		} else {
			OnlineLoader.SetActive(true);
			SpriteOffline.gameObject.SetActive(false);
			SpriteOnline.gameObject.SetActive(true);
			MapTex.gameObject.SetActive(true);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}