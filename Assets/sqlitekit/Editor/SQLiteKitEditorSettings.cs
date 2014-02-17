using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class SQLiteKitEditorSettings : ScriptableObject {
	
	[SerializeField] string database = "import_test.db";
	[SerializeField] List<Table> tables = new List<Table>();
	
	public string Database { get{ return database; } set {database = value;} }
	
	public int TableCount { 
		get{
			return tables.Count;
		} 
		set {
			// apply limits
			if(value > 100)
				value = 100;
			if(value < 0)
				value = 0;
			
			// add tables
			while( tables.Count < value )
			{
				tables.Add(new Table("",""));
			}
			
			// remove tables
			while( tables.Count > value )
			{
				tables.RemoveAt(tables.Count-1);
			}
		} 
	}
	
	public Table GetTableAt(int pos)
	{
		if(pos < 0 || pos >= tables.Count)
		{
			return null;
		}
		return tables[pos];
	}
	
	[System.Serializable]
	public class Table {
		
		[SerializeField] string url;
		[SerializeField] string tableName;
		
		
		public string Url { get{ return url; } set {url = value;} }
		public string Name { get{ return tableName; } set {tableName = value;} }
		
		public Table(string url, string name)
		{
			this.url = url;
			this.tableName = name;
		}
			
	}
	
	public void Import(SQLiteKitEditorSettings settings)
	{
		database = settings.database;
		tables = settings.tables;
	}
	
	
}

