using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public PlayerDB db;

	private int maxHP, maxMP;
	private int hp, mp, r_atk, m_atk, def, xp, money;

	// Use this for initialization
	void Start () {
		// load the player stats
		hp = db.getHP ();
		mp = db.getHP ();

		maxHP = db.getMaxHP ();
		maxMP = db.getMaxMP ();

		r_atk = db.getRangeAttack ();
		m_atk = db.getMeleeAttack ();
		def = db.getDefense ();
		money = db.getMoney ();
	}
	
	// Update is called once per frame
	void Update () {
	}

#region Player Health and Mana Management Methods
	public void UpdateCurrentHP (int currHP) {
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
#endregion

#region Player Update Methods
	public void UpdateXP (int xp)
	{ 
			db.setXP (xp);
	}

	public void UpdateMoney (int money)
	{
			db.setMoney (money);
	}
#endregion

#region Get Methods
	public int getHP() { return hp; }
	public int getMP() { return mp; }

	public int getRangeAttack() { return r_atk; }
	public int getMeleeAttack() { return m_atk; }
	public int getDefense() { return def; }
#endregion
}