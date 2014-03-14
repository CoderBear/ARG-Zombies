using UnityEngine;
using System.Collections;

public class PlayerDB : MonoBehaviour {
	
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

	public int getMaxHP() {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/playerDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT maxhp FROM player_info WHERE id=1");
		qr.Step ();
		num = qr.GetInteger ("maxhp");
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

	public int getMaxMP() {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/playerDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT maxmp FROM player_info WHERE id=1");
		qr.Step ();
		num = qr.GetInteger ("maxmp");
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
	
	public int getRangeAttack() {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/playerDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT r_atk FROM player_info WHERE id=1");
		qr.Step ();
		num = qr.GetInteger ("r_atk");
		qr.Release ();
		db.Close ();
		
		return num;
	}

	public int getMeleeAttack() {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/playerDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT m_atk FROM player_info WHERE id=1");
		qr.Step ();
		num = qr.GetInteger ("m_atk");
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

	public int getInit(int id) {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/playerDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT init FROM player_info WHERE id=?");
		qr.Bind (id);
		qr.Step ();
		num = qr.GetInteger ("init");
		qr.Release ();
		db.Close (); 
		
		return num;
	}
#endregion

#region SQL Lite Storage Functions
	public void setXP(int xp) {
		string dbFile = Application.persistentDataPath + "/playerDB.db";
		SQLiteDB db = new SQLiteDB ();

		string query = "UPDATE player_info SET xp = " + xp.ToString() + " WHERE id = 1";
		db.Open (dbFile);

		SQLiteQuery qr = new SQLiteQuery (db, query);
		qr.Step ();
		qr.Release ();
		db.Close ();
	}
	public void setMoney(int money) {
		string dbFile = Application.persistentDataPath + "/playerDB.db";
		SQLiteDB db = new SQLiteDB ();

		string query = "UPDATE player_info SET money = " + money.ToString () + " WHERE id = 1";
		db.Open (dbFile);
		
		SQLiteQuery qr = new SQLiteQuery (db, query);
		qr.Step ();
		qr.Release ();
		db.Close ();
	}

	// Updates variable player stats
	public void setCurrentHP_MP(int hp, int mp) {
		string dbFile = Application.persistentDataPath + "/playerDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		string query = "UPDATE player_info SET hp = " + hp.ToString () + ", mp = " + mp.ToString () + " WHERE id = 1";
		db.Open (dbFile);
		
		SQLiteQuery qr = new SQLiteQuery (db, query);
		qr.Step ();
		qr.Release ();
		db.Close ();
	}

	public void setMaxHP_MP(int maxHP, int maxMP) {
		string dbFile = Application.persistentDataPath + "/playerDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		string query = "UPDATE player_info SET maxhp = " + maxHP.ToString () + ", maxmp = " + maxMP.ToString () + " WHERE id = 1";
		db.Open (dbFile);
		
		SQLiteQuery qr = new SQLiteQuery (db, query);
		qr.Step ();
		qr.Release ();
		db.Close ();
	}
	public void setAttackDefense(int r_atk, int m_atk, int def) {}
#endregion
}