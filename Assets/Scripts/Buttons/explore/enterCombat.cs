using UnityEngine;
using System.Collections;
using NPack;

public class enterCombat : MonoBehaviour {

	private MersenneTwister rand;
	private const int COMBAT_VALUE = 75; // how often an encounter can occur (MAX=100)% of the time
	private const int DICE_VALUE_MIN = 1;  //The lowest value that can be rolled
	private const int D20_VALUE_MAX = 20; // the highest roll on a d20
	private const int D4_VALUE_MAX = 4; // the highest roll on a d4

	public GameObject goCombat, goResult;

	// Use this for initialization
	void Start () {
		rand = new MersenneTwister ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnClick() {
		goCombat.SetActive(false);
		goResult.SetActive(true);

//		DoAutoCombat ();
	}

	private void DoAutoCombat() {

	}
}