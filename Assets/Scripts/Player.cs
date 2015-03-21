using UnityEngine;
using System;
using System.Collections;

public class Player : MonoBehaviour {

	public PlayerDB db;

//	private HUDText hudText;

	private int maxHP = 0, maxMP = 0;
//	private int initMod = 0, initiative = 0;
	private int hp = 0, mp = 0, r_atk = 0, m_atk = 0, def = 0, xp = 0, money = 0;
	private int lastXP = 0, lastMoney = 0;
	private float ratioHP = 0.0f, ratioMP = 0.0f;
	
//	Renderer[] renderers;
	Component[] renderers = new Component[36];
	GameObject animationObject;
	
	string path_to_body = "Animations/Character1.5/Body/";
	string path_to_faces = "Animations/Character1.5/NewFaces/";

	public bool OfflineMode {
		get;
		set;
	}

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
		
//		InitAnimations();

//		hudText = GetComponent<HUDText> ();
	}
	
	// Update is called once per frame
	void Update () {
	}

#region Player Animation Methods
	public void InitAnimations() {
		animationObject = LoadAnimations("Animations/Character1.5/Char");
		renderers = animationObject.GetComponentsInChildren<Renderer>();
		DontDestroyOnLoad(animationObject);
		
		Renderer sr =  (Renderer)renderers[5];
		sr.materials[0].mainTexture = (Texture2D)Resources.Load(path_to_body + "chilot", typeof(Texture2D));// as Texture2D);
		sr.materials[1].mainTexture = (Texture2D)Resources.Load(path_to_body + "corp", typeof(Texture2D));
		sr.materials[2].mainTexture = (Texture2D)Resources.Load(path_to_faces + "cap1", typeof(Texture2D));
		sr.materials[3].mainTexture = (Texture2D)Resources.Load(path_to_body + "pantalon_drept", typeof(Texture2D));
		sr.materials[4].mainTexture = (Texture2D)Resources.Load(path_to_body + "pantalon_stang", typeof(Texture2D));
		sr.materials[5].mainTexture = (Texture2D)Resources.Load(path_to_body + "picior_drept", typeof(Texture2D));
		sr.materials[6].mainTexture = (Texture2D)Resources.Load(path_to_body + "picior_stang", typeof(Texture2D));
		sr.materials[7].mainTexture = (Texture2D)Resources.Load(path_to_body + "mana_dreapta", typeof(Texture2D));
		sr.materials[8].mainTexture = (Texture2D)Resources.Load(path_to_body + "mana_stanga", typeof(Texture2D));
	}

	GameObject LoadAnimations(string path) {
		GameObject go = (GameObject)Instantiate(Resources.Load(path));
		Hide(go);
		go.SetActive(false);
		return go;
	}
	
	public void PlayAnimations(string name){
		animationObject.animation.CrossFade(name);
		animationObject.animation.Play("idle");
	}
	
	public void Hide() {
		renderers = animationObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer r in renderers)
		{
			r.enabled = false;
		}
		animationObject.SetActive(false);
	}
	
	public void Hide(GameObject gObject) {
		renderers = gObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer r in renderers)
		{
			r.enabled = false;
		}
		gObject.SetActive(false);
	}
	
	public void Show() {
		animationObject.SetActive(true);
		renderers = animationObject.GetComponentsInChildren<Renderer>();
		foreach (Renderer r in renderers)
		{
			r.enabled = true;
		}
	}
	
	public void Show(GameObject gObject) {
		if (animationObject != gObject)
		{
			gObject.SetActive(false);
			Hide();
			renderers = gObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer r in renderers)
			{
				r.enabled = true;
			}
			animationObject = gObject;
		}
	}
#endregion

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
		float hpF = Convert.ToSingle (hp), maxHPf = Convert.ToSingle(maxHP);
		ratioHP = hpF / maxHPf;
		if (ratioHP > 1f) {
			ratioHP = 1f;
		} else if (ratioHP < 0f) {
			ratioHP = 0f;
		}
	}

	public void OnEnergyUpdate() {
		float mpF = Convert.ToSingle (mp), maxMPf = Convert.ToSingle (maxMP);
		ratioMP = mpF / maxMPf;

		if(ratioMP > 1f) {
			ratioMP = 1f;
		} else if(ratioMP < 0f) {
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