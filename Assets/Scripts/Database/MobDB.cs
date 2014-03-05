using UnityEngine;
using System.Collections;

public class MobDB : MonoBehaviour {
	
	public int getHP(int id) {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/mobDB.db";
		SQLiteDB db = new SQLiteDB ();

		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT hp FROM mob_info WHERE id=?");
		qr.Bind (id);
		qr.Step ();
		num = qr.GetInteger ("hp");
		qr.Release ();
		db.Close ();

		return num;
	}

	public int getMP(int id) {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/mobDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT mp FROM mob_info WHERE id=?");
		qr.Bind (id);
		qr.Step ();
		num = qr.GetInteger ("mp");
		qr.Release ();
		db.Close ();
		
		return num;
	}
	
	public int getDefense(int id) {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/mobDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT defense FROM mob_info WHERE id=?");
		qr.Bind (id);
		qr.Step ();
		num = qr.GetInteger ("defense");
		qr.Release ();
		db.Close ();
		
		return num;
	}
	
	public int getAttack(int id) {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/mobDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT attack FROM mob_info WHERE id=?");
		qr.Bind (id);
		qr.Step ();
		num = qr.GetInteger ("attack");
		qr.Release ();
		db.Close ();
		
		return num;
	}
	
	public int getMoney(int id) {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/mobDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT money FROM mob_info WHERE id=?");
		qr.Bind (id);
		qr.Step ();
		num = qr.GetInteger ("money");
		qr.Release ();
		db.Close ();
		
		return num;
	}
	
	public int getXP(int id) {
		int num = 0;
		string dbFile = Application.persistentDataPath + "/mobDB.db";
		SQLiteDB db = new SQLiteDB ();
		
		db.Open (dbFile);
		SQLiteQuery qr = new SQLiteQuery (db, "SELECT xp FROM mob_info WHERE id=?");
		qr.Bind (id);
		qr.Step ();
		num = qr.GetInteger ("xp");
		qr.Release ();
		db.Close ();
		
		return num;
	}
}