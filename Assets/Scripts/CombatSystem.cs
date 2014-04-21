using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NPack;
using URandom;

public class CombatSystem : MonoBehaviour {

	private MersenneTwister rand;
	private DiceRoll die;

	private const int DICE_VALUE_MIN = 1;  //The lowest value that can be rolled
	private const int D20_VALUE_MAX = 20; // the highest roll on a d20
	private const int D4_VALUE_MAX = 4; // the highest roll on a d4
	private const int COMBATANTS_MAX = 3; // Maximum number of Combatants

	private static Vector3 MOB1_POS = new Vector3(285f,-70f,0f);
	private static Vector3 PLAYER_POS = new Vector3(-400f,-45f, -0f);
	
	[SerializeField]private Player goPlayer;
	public GameObject playerPF;
	public GameObject mobCultist, mobFollower;
	private GameObject spawnedMob, spawnedPlayer;

	[SerializeField]private GameObject vitoryUI;
	[SerializeField]private UIProgressBar healthBar;
	[SerializeField]private UIProgressBar energyBar;
	[SerializeField]private UIPanel panel;
	
	void Awake() {
		// set local private player object to the singleton player.
		goPlayer = GameObject.FindWithTag ("Player").GetComponent<Player>();
	}

	// Use this for initialization
	void Start () {
		healthBar.value = goPlayer.getHPratio();
		energyBar.value = goPlayer.getMPratio();

		spawnedPlayer = NGUITools.AddChild (panel.gameObject, playerPF);
		spawnedPlayer.transform.localPosition = PLAYER_POS;

		rand = new MersenneTwister ();

		// create the ememies that the player will combat against
//		RandomEnemySpawn ();
		spawnFollowers ();

		Debug.Log ("Initializing MOBs");
		
		InitializeMOB ();
	}
	
	// Update is called once per frame
	void Update () {
		healthBar.value = goPlayer.getHPratio();
		healthBar.ForceUpdate ();
		energyBar.value = goPlayer.getMPratio();
		energyBar.ForceUpdate ();
	}

#region End Combat methods
	// Check to see if victory can be declared
	private void CheckPlayerCombatResults () {
		if (goPlayer.isDead ()) {
			EndCombat (2);
		}
	}

	private void CheckMobCombatResults() {
		if(spawnedMob.GetComponent<Mob> ().isDead ()){
			EndCombat (1);
		}
	}

	private void EndCombat(int value) {
		Debug.Log ("Ending Combat");
		
		// if player dies return to map screen
		switch(value) {
		case 1: // player won
			goPlayer.UpdateXP(spawnedMob.GetComponent<Mob> ().getXP ());
			goPlayer.UpdateMoney (spawnedMob.GetComponent<Mob> ().getMoney ());
			goPlayer.UpdateDB();
			vitoryUI.SetActive (true);
			break;
		case 2: // player defeated
//			goPlayer.hidePlayer();
			Application.LoadLevel("gameMap");
			break;
		default:
			break;
		}
	}
#endregion
	
#region DoCombat Methods
	private void DoMeleeCombat() {
		Debug.Log ("Starting Combat");
		// Battle Sequence (Demo: Initiative not calculated)
		Debug.Log ("goMOB !isDead: " + !spawnedMob.GetComponent<Mob> ().isDead ());
		if (!spawnedMob.GetComponent<Mob> ().isDead ()) {
			if (isHit (goPlayer.getMeleeAttack (), spawnedMob.GetComponent<Mob> ().getDefense ())) {
				dealDamage (2, goPlayer.getMeleeAttack ());
				Debug.Log("goMOB Health is " + spawnedMob.GetComponent<Mob> ().getCurrentHP() + "/8" );
			}
		}
		CheckPlayerCombatResults ();

		// Now monster attacks
		if (isHit (spawnedMob.GetComponent<Mob> ().getAttack (), goPlayer.getDefense ())) {
			dealDamage (1, spawnedMob.GetComponent<Mob> ().getAttack ());
			Debug.Log("Player Health is " + goPlayer.getHP() + "/13");
		}
		CheckMobCombatResults ();
	}
#endregion

#region OnClick Methods
	// Runs one round of combat based on the basic Attack selected
	// Player attack choice is calculated.
	public void OnBasicAttackClick () {
		DoMeleeCombat ();
	}

	public void OnSpecialAttackClick(){
	}

	public void OnInventoryClick() {
	}
#endregion

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
			spawnedMob = NGUITools.AddChild (panel.gameObject, mobCultist);
			spawnedMob.transform.localPosition = MOB1_POS;
	}
	
	private void spawnFollowers () {
		spawnedMob = NGUITools.AddChild (panel.gameObject, mobFollower); // Spawn and add to panel so it shows up in the scene on top
		Debug.Log ("Vector3 position of pfFollower (clone) was " + spawnedMob.transform.position);
//			GameObject.FindWithTag ("follower").transform.localPosition = MOB1_POS ; // Place newly spawned object at the correct spot on the screen.
		spawnedMob.transform.localPosition = MOB1_POS;
		Debug.Log ("Vector3 position of pfFollower (clone) is " + spawnedMob.transform.position);
	}
	
	// Initialize the gameobjext according to its tag
	private void InitializeMOB() {
		if(spawnedMob.tag == "cultist")
			spawnedMob.GetComponent<Mob>().Initialize (1);
		if(spawnedMob.tag == "follower")
			spawnedMob.GetComponent<Mob>().Initialize (2);
	}
#endregion

#region Cleanup Methods
	private void doCleanup() {
		Destroy(spawnedMob);
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
			healthBar.value = goPlayer.getHPratio();
			energyBar.value = goPlayer.getMPratio();
			break;
		case 2: // Mob
			Debug.Log ("goMOB was dealt " + dmg + " damage");
			spawnedMob.GetComponent<Mob> ().UpdateCurrentHP(-dmg);
			break;
		default:
			break;
		}
	}
#endregion
}