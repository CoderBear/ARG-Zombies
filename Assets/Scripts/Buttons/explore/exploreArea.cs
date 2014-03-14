using UnityEngine;
using System.Collections;
using NPack;

public class exploreArea : MonoBehaviour {

	private MersenneTwister rand;
	private const int ENCOUNTER_VALUE_MAX = 100; // max perctange is 100%
	private const int ENCOUNTER_VALUE = 65; // how often an encounter can occur (MAX=100)% of the time.
	private const int NUM_ROOMS_MIN = 3; // how few rooms an area has
	private const int NUM_ROOMS_MAX = 5; // max number of rooms an area has,

	public GameObject goExplore, goCombat;

	public UILabel labelRoomClear;
	public UISprite spriteAlert;

	private int roomsCleared = 0, roomsTotal = 0;

	// Use this for initialization
	void Start () {
		rand = new MersenneTwister ();

		roomsTotal = rand.Next (NUM_ROOMS_MIN, NUM_ROOMS_MAX);
		labelRoomClear.text = "0 / " + roomsTotal.ToString ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnClick() {
		if (roomsCleared < roomsTotal) {
			int fofValue = rand.Next (ENCOUNTER_VALUE_MAX);

			if (fofValue > ENCOUNTER_VALUE) { // An enounter occurs
				spriteAlert.gameObject.SetActive (true);
				goExplore.SetActive (false);
				goCombat.SetActive (true);
			} else { // the room is clear and player continues
				labelRoomClear.gameObject.SetActive (true);
			}

			// updates total and display on screen
			roomsCleared++;
			labelRoomClear.text = roomsCleared.ToString () + " / " + roomsTotal.ToString ();
		} else {
			Application.LoadLevel("gameMap");
		}
	}
}