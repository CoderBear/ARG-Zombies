using UnityEngine;
using System.Collections;
using NPack;

public class exploreArea : MonoBehaviour {

	private MersenneTwister rand;
	private const int ENCOUNTER_VALUE = 65; // how often an encounter can occur (MAX=100)% of the time.

	public GameObject goExplore, goCombat;

	public UILabel labelRoomClear;
	public UISprite spriteAlert;

	// Use this for initialization
	void Start () {
		rand = new MersenneTwister ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnClick() {
		int fofValue = rand.Next (ENCOUNTER_VALUE);
		
		if(fofValue > ENCOUNTER_VALUE ){ // An enounter occurs
			spriteAlert.gameObject.SetActive(true);
			goExplore.SetActive(false);
			goCombat.SetActive(true);
		} else { // the room is clear and player continues
			labelRoomClear.gameObject.SetActive(true);
		}
	}
}