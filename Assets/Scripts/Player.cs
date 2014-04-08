using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public PlayerDB db;

//	public UISprite sprite;
//	private HUDText hudText;

	private int maxHP = 0, maxMP = 0;
//	private int initMod = 0, initiative = 0;
	private int hp = 0, mp = 0, r_atk = 0, m_atk = 0, def = 0, xp = 0, money = 0;
	private int lastXP = 0, lastMoney = 0;
	private float ratioHP = 0.0f, ratioMP = 0.0f;

	public string LastAreaVisited {
		get;
		set;
	}

	public int RoomsLeft {
		get;
		set;
	}

	public int RoomsTotal {
		get;
		set;
	}

	void Awake() {
		DontDestroyOnLoad (this.gameObject);
		this.gameObject.AddComponent<PlayerDB> ();
	}

	// Use this for initialization
	void Start () {
		// load the player stats
		hp = db.getHP ();
		mp = db.getMP ();

		maxHP = db.getMaxHP ();
		maxMP = db.getMaxMP ();

		// Create the initial ratios for use in the Combat UI
		OnHealthUpdate ();
		OnEnergyUpdate ();

		r_atk = db.getRangeAttack ();
		m_atk = db.getMeleeAttack ();

		def = db.getDefense ();
		xp = db.getXP ();
		money = db.getMoney ();

//		hudText = GetComponent<HUDText> ();
	}
	
	// Update is called once per frame
	void Update () {
	}
//
//	public void showPlayer() {
//		sprite.gameObject.SetActive (true);
//		
//	}
//
//	public void hidePlayer() {
//		sprite.gameObject.SetActive (true);
//		
//	}

#region Player Health and Mana Management Methods
	public void UpdateCurrentHP (int currHP) {
		//show the damage in red
//		hudText.Add (currHP, Color.red, 1f);
		hp += currHP;

		if (hp > maxHP)
			hp = maxHP;
		else if (hp < 0) {
			hp = 0;
		}
	}

	public void UpdateCurrentMP (int currMP) {
		mp += currMP;
		
		if (mp > maxMP)
			mp = maxMP;
		else if (mp < 0) {
			mp = 0;
		}
	}

	public void OnHealthUpdate() {
		ratioHP = (float)hp/(float)maxHP;
		if (ratioHP > 1) {
			ratioHP = 1f;
		} else if (ratioHP < 0) {
			ratioHP = 0f;
		}
	}

	public void OnEnergyUpdate() {
		ratioHP = (float)mp/(float)maxMP;

		if(ratioMP > 1) {
			ratioMP = 1f;
		} else if(ratioMP < 0) {
			ratioMP = 0f;
		}
	}
#endregion

#region Player Update Methods
	// save the XP gained from recent battle
	public void UpdateXP (int xp_recent)
	{
		lastXP = xp_recent;
		xp += xp_recent;
	}

	// save the Money earned from recent battle
	public void UpdateMoney (int money_recent)
	{
		lastMoney = money_recent;
		money += money_recent;
	}

	// update database with new values of XP, Money, health
	public void UpdateDB() {
		db.setCurrentHP_MP (hp, mp);
		db.setXP (xp);
		db.setMoney (money);
	}
#endregion

#region Get Methods
	public bool isDead ()
	{
		if (hp <= 0)
			return true;
		else
			return false;
	}
	
	public int getHP() { return hp; }
	public int getMP() { return mp; }

	public string getHPinfo() { return hp + "/" + maxHP; }
	public string getMPinfo() { return mp + "/" + maxMP; }

	public float getHPratio() { return ratioHP; }
	public float getMPratio() { return ratioMP; }

	public int getRangeAttack() { return r_atk; }
	public int getMeleeAttack() { return m_atk; }
	public int getDefense() { return def; }

	public int getLastXP() { return lastXP; }
	public int getLastMoney() { return lastMoney; }
#endregion
}