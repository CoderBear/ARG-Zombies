using UnityEngine;
using System.Collections;

public class mode_Obect_Loader : MonoBehaviour {

	public GameObject onlineLoader, offlineLoader;
	public UITexture mapTex;
	public UISprite sprite;
	private Player player;
	void Awake() {
		// set local private player object to the singleton player.
		player = GameObject.FindWithTag ("Player").GetComponent<Player>();
	}
	
	// Use this for initialization
	void Start () {
		if(player.OfflineMode) {
			offlineLoader.SetActive(true);
//			sprite.gameObject.SetActive(true);
//			mapTex.gameObject.SetActive(false);
		} else {
			onlineLoader.SetActive(true);
			sprite.gameObject.SetActive(false);
			mapTex.gameObject.SetActive(true);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}