using UnityEngine;
using System.Collections;

public class PlayerDB : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

#region SQL Lite Query Functions
	public int getHP() {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/playerDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT hp FROM player_info WHERE id=1");
		qr.Step ();
		num = qr.GetInteger ("hp");
		qr.Release ();
		db.Close ();
		
		return num;
	}
	
	public int getMP() {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/playerDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT mp FROM player_info WHERE id=1");
		qr.Step ();
		num = qr.GetInteger ("mp");
		qr.Release ();
		db.Close ();
		
		return num;
	}
	
	public int getDefense() {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/playerDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT defense FROM player_info WHERE id=1");
		qr.Step ();
		num = qr.GetInteger ("defense");
		qr.Release ();
		db.Close ();
		
		return num;
	}
	
	public int getAttack() {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/playerDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT attack FROM player_info WHERE id=1");
		qr.Step ();
		num = qr.GetInteger ("attack");
		qr.Release ();
		db.Close ();
		
		return num;
	}
	
	public int getMoney() {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/playerDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT money FROM player_info WHERE id=1");
		qr.Step ();
		num = qr.GetInteger ("money");
		qr.Release ();
		db.Close ();
		
		return num;
	}
	
	public int getXP() {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/playerDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT xp FROM player_info WHERE id=1");
		qr.Step ();
		num = qr.GetInteger ("xp");
		qr.Release ();
		db.Close ();
		
		return num;
	}
#endregion

#region SQL Lite Storage Functions
#endregion
}