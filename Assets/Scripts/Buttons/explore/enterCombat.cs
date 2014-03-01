using UnityEngine;
using System.Collections;
<<<<<<< HEAD
using NPack;

public class enterCombat : MonoBehaviour {

	private MersenneTwister rand;
	private const int COMBAT_VALUE = 75; // how often an encounter can occur (MAX=100)% of the time.

	public GameObject goCombat, goResult;

	// Use this for initialization
	void Start () {
		rand = new MersenneTwister ();
=======

public class enterCombat : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
>>>>>>> origin/master
	}
	
	// Update is called once per frame
	void Update () {
<<<<<<< HEAD
	}

	void OnClick() {
		int fofValue = rand.Next (COMBAT_VALUE);

		if(fofValue > COMBAT_VALUE ){ // An enounter occurs
			goCombat.SetActive(false);
			goResult.SetActive(true);
		} else { // the room is clear and player continues
			Application.LoadLevel ("gameMap");
		}
	}
}
=======
	
	}
}
>>>>>>> origin/master
