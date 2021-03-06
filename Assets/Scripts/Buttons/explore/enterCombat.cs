﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NPack;

public class enterCombat : MonoBehaviour {

	private MersenneTwister rand;
	private const int COMBAT_VALUE = 75; // how often an encounter can occur (MAX=100)% of the time
	private const int DICE_VALUE_MIN = 1;  //The lowest value that can be rolled
	private const int D20_VALUE_MAX = 20; // the highest roll on a d20
	private const int D4_VALUE_MAX = 4; // the highest roll on a d4
	private const int COMBATANTS_MAX = 3; // Maximum number of Combatants

	private const int MOB1_POS_X = 125;
	private const int MOB1_POS_Y = -24;
	private const int MOB2_POS_X = 125;
	private const int MOB2_POS_Y = -24;
	
	private static Vector3 MOB1_POS = new Vector3(125f,-24f,-1f);
	private static Vector3 MOB2_POS = new Vector3(275f,-136f, -1f);

	private Player goPlayer;
	public GameObject goMOB1, goMOB2;

	[SerializeField]
	public List<GameObject> spawnedMobs;

	public ExploreCombatUI uiObject;

	// Use this for initialization
	void Start () {
		rand = new MersenneTwister ();
		spawnedMobs = new List<GameObject> ();

		// set local private player object to the singleton player.
		goPlayer = GameObject.FindWithTag ("Player").GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnClick() {
		goPlayer.LastAreaVisited = Application.loadedLevelName;
//		Application.LoadLevel ("gameCombat");
//		Debug.Log ("Spawning MOBs");
		
		// create the ememies that the player will combat against
//		RandomEnemySpawn ();
//		for (int i = 0; i < 2; i++) {
//			spawnFollowers ();
//		};
//		spawnFollowers ();
//		spawnCultists ();

//		Debug.Log ("Initializing MOBs");
//
//		InitializeMOB ();
//		
//		Debug.Log ("goMob1 currentHealth is " + spawnedMobs[0].GetComponent<Mob>().getHP ());
//		Debug.Log ("goMOB1 Death is " + spawnedMobs [0].GetComponent<Mob> ().isDead());
//		Debug.Log ("goMob2 currentHealth is " + spawnedMobs[1].GetComponent<Mob>().getHP ());
//		Debug.Log ("goMOB2 Death is " + spawnedMobs [1].GetComponent<Mob> ().isDead());
//
//		Debug.Log ("Starting Auto-Combat");
//		DoAutoCombat ();
	}

	/* For Initial proof of concept, battle is automated
	 * - Initiative is not calculated, for battle order
	 * - Basic combat system and formulas are implemented
	 * - Only melee attack is used
	 * - Inital Foundations set for turn-based battle system.
	 * - Future: Turn Based Battle System
	 */
	private void DoAutoCombat() {
		bool done = false;
		int cIndex = 1; // The current combatant 1-3
		int victor = 0; // The winner, 0 = initial value | 1 = player | 2 = enemy

		Debug.Log ("Starting Combat");
		/* Battle Sequence (Demo: Initiative not calculated)
		 * 1st - Player | 2nd - MOB 1 | 3rd - MOB 2
		 */
		while(!done) {
			Debug.Log("current cIndex is " + cIndex);
			switch (cIndex) {
			case 1: // Player
				Debug.Log ("goMOB1 !isDead: " + !spawnedMobs [0].GetComponent<Mob> ().isDead ());
				if (!spawnedMobs [0].GetComponent<Mob> ().isDead ()) {
					if (isHit (goPlayer.getMeleeAttack (), spawnedMobs [0].GetComponent<Mob> ().getDefense ())) {
						dealDamage (2, goPlayer.getMeleeAttack ());
						Debug.Log("goMOB1 Health is " + spawnedMobs [0].GetComponent<Mob> ().getCurrentHP() + "/8" );
					}
				} else if(spawnedMobs [0].GetComponent<Mob> ().isDead ()) {
					if (isHit (goPlayer.getMeleeAttack (), spawnedMobs [1].GetComponent<Mob> ().getDefense ())) {
						dealDamage (3, goPlayer.getMeleeAttack ());
						Debug.Log("goMOB2 Health is " + spawnedMobs [1].GetComponent<Mob> ().getCurrentHP() + "/8" );
					}
				}
				break;
			case 2: // Mob 1
				Debug.Log("goMOB1 is attacking");
				if (isHit (spawnedMobs [0].GetComponent<Mob> ().getAttack (), goPlayer.getDefense ())) {
					dealDamage (1, spawnedMobs [0].GetComponent<Mob> ().getAttack ());
					Debug.Log("Player Health is " + goPlayer.getHP() + "/13" );
				}
				break;
			case 3: // Mob 2
				Debug.Log("goMOB2 is attacking");
				if (isHit (spawnedMobs [1].GetComponent<Mob> ().getAttack (), goPlayer.getDefense ())) {
					dealDamage (1, spawnedMobs [1].GetComponent<Mob> ().getAttack ());
					Debug.Log("Player Health is " + goPlayer.getHP() + "/13" );
				}
				break;
			default:
				break;
			}

			// check the health of the player and the mobs
			if (spawnedMobs [0].GetComponent<Mob> ().isDead ()) {
				cIndex += 2;
			} else if (spawnedMobs [1].GetComponent<Mob> ().isDead ()) {
				cIndex += 2;
			} else {
				cIndex++;

				// if we have gane through all the turns this round,
				// we reset the index.
				if (cIndex > 3)
					cIndex = 1;
			}

			// check if either player is dead or both enemies are
			// dead and declare a winner.
//			Debug.Log ("Player Death is " + goPlayer.isDead());
//			Debug.Log ("goMOB1 Death is " + spawnedMobs [0].GetComponent<Mob> ().isDead());
//			Debug.Log("goMOB2 Health is " + spawnedMobs [1].GetComponent<Mob> ().getCurrentHP() + "/8" );
//			Debug.Log ("goMOB2 Death is " + spawnedMobs [1].GetComponent<Mob> ().isDead());
			if (goPlayer.isDead ()) {
				victor = 2;
				done = true;
			} else if (spawnedMobs [0].GetComponent<Mob> ().isDead () && spawnedMobs [1].GetComponent<Mob> ().isDead ()) {
				victor = 1;
				done = true;
			}
		}
		Debug.Log ("Ending Combat");

		// if player dies return to map screen
		switch(victor) {
		case 1: // player won
			cIndex = 1;
			done = false;
			goPlayer.UpdateXP(spawnedMobs[0].GetComponent<Mob>().getXP () + spawnedMobs[1].GetComponent<Mob>().getXP ());
			goPlayer.UpdateMoney (spawnedMobs[0].GetComponent<Mob>().getMoney () + spawnedMobs[1].GetComponent<Mob>().getMoney ());
			goPlayer.UpdateDB();

			doCleanup ();
			uiObject.SwitchScreenUI (3);
			break;
		case 2: // player defeated
			Application.LoadLevel("gameMap");
			break;
		default:
			break;
		}
	}

#region Monster Generation Methods

	private void RandomEnemySpawn() {
		int n1 = 0, n2 = 0;

		// generate a number between 1 & 2)
		n1 = rand.Next(DICE_VALUE_MIN);
		chooseSpawn(n1);
		n2 = rand.Next(DICE_VALUE_MIN);
		chooseSpawn(n2);

		InitializeMOB();
	}

	private void chooseSpawn(int index) {
		switch(index) {
		case 0: //spawn follower
			spawnFollowers ();
			break;
		case 1: //spawn cultist
			spawnCultists();
			break;
		default:
			break;
		}
	}

	private void spawnCultists() {
		if(spawnedMobs.Count == 0) {
			Instantiate(goMOB1, MOB1_POS, Quaternion.identity);
			spawnedMobs.Add (GameObject.FindWithTag ("cultist"));
		} else {
			Instantiate(goMOB1, MOB2_POS, Quaternion.identity);
			spawnedMobs.Add (GameObject.FindWithTag ("cultist"));
		}
	}

	private void spawnFollowers () {
		if(spawnedMobs.Count == 0) {
			Instantiate(goMOB2, MOB1_POS, Quaternion.identity);
			spawnedMobs.Add (GameObject.FindWithTag ("follower"));
		} else {
			Instantiate(goMOB2, MOB2_POS, Quaternion.identity);
			spawnedMobs.Add (GameObject.FindWithTag ("follower"));
		}
	}

	// Initialize the gameobjext according to its tag
	private void InitializeMOB() {
		foreach (var mob in spawnedMobs) {
			if(mob.tag == "cultist")
				mob.GetComponent<Mob>().Initialize (1);
			if(mob.tag == "follower")
				mob.GetComponent<Mob>().Initialize (2);
		}
	}
#endregion

#region Cleanup Methods
	private void doCleanup() {

		for (int i = 0; i < spawnedMobs.Count; i++) {
			Destroy(spawnedMobs[i]);
		}
		spawnedMobs.Clear ();

		Destroy (GameObject.FindWithTag ("follower"));
		Destroy (GameObject.FindGameObjectWithTag ("cultist"));
	}
#endregion

#region Combat Check/Damage Methods
	// check if a hit lands on the defender
	// attack - modifier of the attacker
	// defense - the attacke's total defense
	// Future: turned into an int to pass CONSTANTS for hits, misses, and criticals
	private bool isHit (int attack, int defense)
	{
		int roll = rand.Next (DICE_VALUE_MIN, D20_VALUE_MAX);

		// we chack if the roll was an Auto Miss or a Critical Hit (Auto-Hit)
		if (roll == D20_VALUE_MAX || roll == DICE_VALUE_MIN) {
			if (roll == D20_VALUE_MAX)
				return true;
			else if (roll == DICE_VALUE_MIN)
				return false;
		}

		if ((roll + attack) > defense) {
			return true;
		} else {
			return false;
		}
	}

	private void dealDamage(int index, int attack) {
		int dmg = 0;
		if(attack > 0)
			dmg = attack + rand.Next (DICE_VALUE_MIN, D4_VALUE_MAX);
		else
			dmg = rand.Next (DICE_VALUE_MIN, D4_VALUE_MAX);

		switch (index) {
		case 1: //Player
			Debug.Log ("Player was dealt " + dmg + " damage");
			goPlayer.UpdateCurrentHP(-dmg);
			break;
		case 2: // Mob 1
			Debug.Log ("goMOB1 was dealt " + dmg + " damage");
			spawnedMobs [0].GetComponent<Mob> ().UpdateCurrentHP(-dmg);
			break;
		case 3: // Mob 2
			Debug.Log ("goMOB2 was dealt " + dmg + " damage");
			spawnedMobs [1].GetComponent<Mob> ().UpdateCurrentHP(-dmg);
			break;
		default:
			break;
		}
	}
#endregion
}