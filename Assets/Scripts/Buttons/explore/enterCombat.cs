using UnityEngine;
using System.Collections;
using NPack;

public class enterCombat : MonoBehaviour {

	private MersenneTwister rand;
	private const int COMBAT_VALUE = 75; // how often an encounter can occur (MAX=100)% of the time
	private const int DICE_VALUE_MIN = 1;  //The lowest value that can be rolled
	private const int D20_VALUE_MAX = 20; // the highest roll on a d20
	private const int D4_VALUE_MAX = 4; // the highest roll on a d4
	private const int COMBATANTS_MAX = 3; // Maximum number of Combatants
	
	private Player goPlayer;
	private Mob goMOB1, goMOB2;
	
	public GameObject goCombat, goResult;

	// Use this for initialization
	void Start () {
		rand = new MersenneTwister ();

		// set local private player object to the singleton player.
		goPlayer = gameObject.AddComponent<Player> ();

		// creat the ememies that the player will combat against
		goMOB1 = new Mob ();
		goMOB2 = new Mob ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnClick() {
		DoAutoCombat ();
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

		/* Battle Sequence (Demo: Initiative not calculated)
		 * 1st - Player | 2nd - MOB 1 | 3rd - MOB 2
		 */
		while(!done) {
			switch (cIndex) {
			case 1: // Player
				if(!goMOB1.isDead()) {
					if(isHit (goPlayer.getMeleeAttack(), goMOB1.getDefense ()))
						dealDamage(2,goPlayer.getMeleeAttack());
				} else {
					if(isHit (goPlayer.getMeleeAttack(), goMOB2.getDefense ()))
						dealDamage(3,goPlayer.getMeleeAttack());
				}
				break;
			case 2: // Mob 1
				if(isHit(goMOB1.getAttack(),goPlayer.getDefense()))
					dealDamage(1, goMOB1.getAttack ());
				break;
			case 3: // Mob 2
				if(isHit(goMOB2.getAttack(),goPlayer.getDefense()))
					dealDamage(1, goMOB2.getAttack ());
				break;
			default:
			break;
			}

			// check the health of the player and the mobs
			if(goMOB1.isDead ()){
				cIndex += 2;
			} else if(goMOB2.isDead()) {
				cIndex += 2;
			} else {
				cIndex++;

				// if we have gane through all the turns this round,
				// we reset the index.
				if(cIndex < 3)
					cIndex = 1;
			}

			// check if either player is dead or both enemies are
			// dead and declare a winner.

			if (goPlayer.isDead()) {
				victor = 2;
				done = true;
			} else if( goMOB1.isDead () && goMOB2.isDead ()) {
				victor = 1;
				done = true;
			}
		}

		// if player dies return to map screen
		switch(victor) {
		case 1: // player won
			goPlayer.UpdateXP(goMOB1.getXP () + goMOB2.getXP ());
			goPlayer.UpdateMoney (goMOB1.getMoney () + goMOB2.getMoney ());
			goPlayer.UpdateDB();

			doCleanup ();

			goCombat.SetActive(false);
			goResult.SetActive(true);
			break;
		case 2: // player defeated
			Application.LoadLevel("gameMap");
			break;
		default:
			break;
		}
	}

#region Cleanup Methods
	private void doCleanup() {
		Destroy (goMOB1);
		Destroy (goMOB2);
		goMOB1 = null;
		goMOB2 = null;
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
		int dmg = attack + rand.Next (DICE_VALUE_MIN, D4_VALUE_MAX);

		switch (index) {
		case 1: //Player
			goPlayer.UpdateCurrentHP(-dmg);
			break;
		case 2: // Mob 1
			goMOB1.UpdateCurrentHP(-dmg);
			break;
		case 3: // Mob 2
			goMOB2.UpdateCurrentHP(-dmg);
			break;
		default:
			break;
		}
	}
#endregion
}