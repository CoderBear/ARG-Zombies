﻿using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	public PlayerDB db = null;

	private int maxHP = 0, maxMP = 0;
	private int initMod = 0, initiative = 0;
	private int hp = 0, mp = 0, r_atk = 0, m_atk = 0, def = 0, xp = 0, money = 0;
	private int lastXP = 0, lastMoney = 0;

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

	public int getRangeAttack() { return r_atk; }
	public int getMeleeAttack() { return m_atk; }
	public int getDefense() { return def; }

	public int getLastXP() { return lastXP; }
	public int getLastMoney() { return lastMoney; }
#endregion
}