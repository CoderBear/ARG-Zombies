using UnityEngine;
using System.Collections;

public class Mob : MonoBehaviour
{

	public MobDB db = null;
	private int hp = 0, mp = 0, atk = 0, def = 0, xp = 0, money = 0;
//	private int initMod = 0, initiative = 0;
	private int currentHP;

	[SerializeField]private HUDText hudText;

	// Use this for initialization
	void Start ()
	{
		db = gameObject.AddComponent<MobDB> ();
		hudText = GetComponent<HUDText> ();
	}

	public void Initialize (int id)
	{
		// load the player stats
		currentHP = hp = db.getHP (id);
		mp = db.getHP (id);

		atk = db.getAttack (id);
		def = db.getDefense (id);

		xp = db.getXP (id);
		money = db.getMoney (id);
	}

	// Update is called once per frame
	void Update ()
	{
	}

#region MOB update methods
	public void UpdateCurrentHP (int currHP)
	{
		//show the damage in red
		hudText.Add (currHP, Color.red, 1f);
		currentHP += currHP;

		if (currentHP > hp)
				currentHP = hp;
		else if (currentHP < 0) {
				currentHP = 0;
		}
	}
#endregion

#region Get Methods
	public bool isDead ()
	{
		if (currentHP <= 0)
			return true;
		else
			return false;
	}

	public int getCurrentHP() {
		return currentHP;
	}

	public int getHP ()
	{
		return hp;
	}

	public int getMP ()
	{
		return mp;
	}

	public int getAttack ()
	{
		return atk;
	}

	public int getDefense ()
	{
		return def;
	}

	public int getXP ()
	{
		return xp;
	}

	public int getMoney ()
	{
		return money;
	}
#endregion
}