using UnityEngine;
using System.Collections;
using NPack;

public class exploreArea : MonoBehaviour {

	private MersenneTwister rand;
	private const int ENCOUNTER_VALUE_MAX = 100; // max perctange is 100%
	private const int ENCOUNTER_VALUE = 65; // how often an encounter can occur (MAX=100)% of the time.
	private const int NUM_ROOMS_MIN = 3; // how few rooms an area has
	private const int NUM_ROOMS_MAX = 5; // max number of rooms an area has,

	public ExploreCombatUI uiObject;

	public UILabel labelRoomClear;
//	public UISprite spriteAlert;

	private int roomsCleared = 0, roomsTotal = 0;

	private Player player;
	void Awake() {
		// set local private player object to the singleton player.
		player = GameObject.FindWithTag ("Player").GetComponent<Player>();
	}

	// Use this for initialization
	void Start () {
		rand = new MersenneTwister ();

		if(player.RoomsTotal == 0) {
			roomsTotal = rand.Next (NUM_ROOMS_MIN, NUM_ROOMS_MAX);
			player.RoomsTotal = roomsTotal;
			labelRoomClear.text = "0 / " + roomsTotal.ToString ();
		} else {
			roomsTotal = player.RoomsTotal;
			roomsCleared = player.RoomsLeft;
			labelRoomClear.text = roomsCleared.ToString () + " / " + roomsTotal.ToString ();
		}

	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnClick() {
		if (roomsCleared < roomsTotal) {
			int fofValue = rand.Next (ENCOUNTER_VALUE_MAX);
			Debug.Log ("Encounter(" + roomsCleared.ToString() + ") Value: " + fofValue.ToString ());

			if (fofValue <= ENCOUNTER_VALUE) { // An enounter occurs
//				spriteAlert.gameObject.SetActive (true);

				// updates total and display on screen
				roomsCleared++;
				labelRoomClear.text = roomsCleared.ToString () + " / " + roomsTotal.ToString ();

				player.RoomsLeft = roomsCleared;

				uiObject.SwitchScreenUI (1);
			} else { // the room is clear and player continues
				// updates total and display on screen
				roomsCleared++;
				labelRoomClear.text = roomsCleared.ToString () + " / " + roomsTotal.ToString ();
				labelRoomClear.gameObject.SetActive (true);
			}

		} else {
			player.RoomsLeft = player.RoomsTotal = 0;
			if(player.OfflineMode) {
				Application.LoadLevel ("gameMap");
			} else {
//				Application.LoadLevel("gameMapOnline");
				Application.LoadLevel("gameMapOnlineOld");
			}
		}
	}
}