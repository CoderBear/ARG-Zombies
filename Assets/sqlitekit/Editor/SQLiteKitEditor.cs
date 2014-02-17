using UnityEditor;
using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class SQLiteKitEditor : EditorWindow {

	const string default_url = "https://docs.google.com/spreadsheet/pub?key=0AhKBA3PeUOmddDhQNno2S09VYlpvUC1XdXdMb1NjeHc&output=html";
	const string defautl_database = "import_test.db";
	const string settings_path = "Assets/sqlitekit/Resources/editor_settings.asset";
	const string settings_name = "editor_settings";
	
	SQLiteKitEditorSettings settings;
	string lastError = "";
	Vector2 scrollView = new Vector2(600,400);
	Vector2 scrollView2 = new Vector2(600,200);
	WWW www;
	SQLiteDB db;
	bool import;
	int tableIndex = 0;
	String log;
	
	
	
	[MenuItem( "SQLiteKit/import database ...")]
	static void OpenWindow()
	{
		EditorWindow.GetWindow(typeof(SQLiteKitEditor));
	}

	void LoadSettings()
	{
		if(settings == null)
		{
			settings = AssetDatabase.LoadAssetAtPath(settings_path, typeof(SQLiteKitEditorSettings)) as SQLiteKitEditorSettings;
			if(settings == null)
			{
				settings = ScriptableObject.CreateInstance(typeof(SQLiteKitEditorSettings)) as SQLiteKitEditorSettings;
				AssetDatabase.CreateAsset(settings,settings_path);
			}
		}
	}
	
	void SaveSettings()
	{
		if(settings != null)
		{
			SQLiteKitEditorSettings new_settings = ScriptableObject.CreateInstance(typeof(SQLiteKitEditorSettings)) as SQLiteKitEditorSettings;
			new_settings.Import(settings);
			AssetDatabase.MoveAssetToTrash(settings_path);
			settings = new_settings;
			AssetDatabase.CreateAsset(settings,settings_path);
		}
		AssetDatabase.Refresh();
		AssetDatabase.SaveAssets();
	}
	
	void Log(string message)
	{
		log += message+"\n";
		Debug.Log(message);
	}
	
	void OnGUI()
    {
		
        if (EditorApplication.isPlaying)
        {
			EditorUtility.DisplayDialog("Error", "Editor is in play mode.", "OK");
            return;
        }
		
		LoadSettings();
		
        scrollView = GUILayout.BeginScrollView(scrollView);
		
		
		if( import )
		{
			
			if(db == null)
			{
				string databasePath = Application.streamingAssetsPath +"/"+ settings.Database;
				db = new SQLiteDB();
				try{
					db.Open(databasePath);
					Log("Open database: "+databasePath);
				}catch(Exception e)
				{
					Debug.LogError("Fail to Open database: "+databasePath+"\nWith exception:"+e.Message);
					EditorUtility.DisplayDialog("Error", "Fail to Open database: "+databasePath+"\nWith exception:"+e.Message, "OK");
					import = false;
				}
			}
			
			if(import)
			{
						
				try{
					
					if(www!=null)
					{
						if(www.error != null)
						{
							EditorUtility.DisplayDialog("Error", "Can't download spreadsheet from URL: "+www.error, "OK");
							import = false;
						}
						else
						if(www.isDone)
						{
							SQLiteKitEditorSettings.Table table = settings.GetTableAt(tableIndex);
							SQLiteQuery qr = null;
							
							try{
								
								List<string> rows = GetCVSLines(www.text);
								www = null;
	
								
								// delete old table
								qr = new SQLiteQuery(db,"DROP TABLE IF EXISTS "+table.Name);
								qr.Step();
								qr.Release();
								qr = null;
	
								// read columns
								List<Column> columns = new List<Column>();
								// import header
								List<string> headers = GetCVSLine(rows[0]);
								for( int i = 0; i < headers.Count; i++ )
								{
									string column = headers[i];
									if(Column.IsColumnHeader(column))
									{
										columns.Add(new Column(column,i));
										Log ("  Column: "+column);
									}
								}
								
								if(columns.Count>0)
								{
								
									// compile create table sql
									string createTableSQL = "CREATE TABLE "+table.Name+"( id INTEGER PRIMARY KEY";
									foreach( Column column in columns )
									{
										createTableSQL += ", "+column.Definition;
									}
									createTableSQL += ");";
									qr = new SQLiteQuery(db,createTableSQL);
									qr.Step();
									qr.Release();
									qr = null;
									
		
									
									// compile insert sql
									string names = "";
									string quest = "";
									foreach( Column column in columns )
									{
										if(names.Length == 0){
											names = column.Name;
											quest = "?";
										}
										else{
											names += ","+column.Name;
											quest += ",?";
										}
									}
									
									string insertSQL = "INSERT INTO "+table.Name+" ("+names+") VALUES("+quest+");";
								
									// import data
									for (int i=1; i<rows.Count; i++) {
										
										List<string> row_columns = GetCVSLine(rows[i]);
									
										qr = new SQLiteQuery(db,insertSQL);
										foreach( Column column in columns )
										{
											column.Bind(qr,row_columns[column.Pos]);
										}
										qr.Step();
										qr.Release();
										qr = null;
										
									}
								}
								else
								{
									import = false;
									EditorUtility.DisplayDialog("Error", "Fail to import table '"+table.Name+"' - No columns! Please check source file", "OK");
								}
							}
							catch(Exception e)
							{
								if(qr!=null)
									qr.Release();
								
								EditorUtility.DisplayDialog("Error", "Fail to import table '"+table.Name+"' with Exception: "+e.Message, "OK");
								import = false;
								
							}
							
							tableIndex++;
						}
					}
					else
					if( tableIndex < settings.TableCount )
					{
						SQLiteKitEditorSettings.Table table = settings.GetTableAt(tableIndex);
						
						string url = table.Url;
			            url = url.Trim();
			            if (!url.Contains("&single="))
			                url += "&single=true";
			            if (url.Contains("#gid="))
			                url = url.Replace("#gid=", "&gid=");
			            if (!url.Contains("&gid="))
			                url += "&gid=0";
			            if (url.Contains("&output=html"))
			                url = url.Replace("&output=html", "&output=csv");
			            if (!url.Contains("&output=csv"))
			                url += "&output=csv";
						if (url.Contains("/ccc?"))
			                url = url.Replace("/ccc?", "/pub?");
						
						table.Url = url;
						SaveSettings();
						
			            if (!url.Contains(".google.com"))
			            {
							import = false;
			                EditorUtility.DisplayDialog("Error", "You have entered an incorrect spreadsheet URL. Please read the manuals instructions (See readme.txt)", "OK");
			            }
						else{
							Log ("Import table: "+table.Name +" from URL: "+table.Url);
							www = new WWW(table.Url);
						}
						
					}
					else
					{
						import = false;
						EditorUtility.DisplayDialog("Succeed", "All tables was succefully updated!", "OK");
					}
					
				}
				catch( Exception e )
				{
					Debug.LogError(e);
					EditorUtility.DisplayDialog("Error", "Unexpected exception! Please read log for more information.", "OK");
					import = false;
				}
				
			}
			
	        if (GUILayout.Button("Terminate import"))
	        {
				import = false;
	        }
			
			GUILayout.Space(25);
			
			
			
			GUI.Label(new Rect(0,40,1600,400-40), log);
			
			
			if(import == false)
			{
				if(www!=null)
					www = null;
				
				if(db!=null)
					db.Close();
				db = null;
			}
		}
		else
		{
			
	        GUILayout.Label("Settings", EditorStyles.boldLabel);
			
	        settings.Database = EditorGUILayout.TextField("Database", settings.Database);
			settings.TableCount = EditorGUILayout.IntField("Table Count", settings.TableCount);
			
			GUILayout.Space(20);
			
			scrollView2 = GUILayout.BeginScrollView(scrollView2);
			SQLiteKitEditorSettings.Table table;
			for(int pos = 0; pos < settings.TableCount; pos++)
			{
				GUILayout.Space(10);
				table = settings.GetTableAt(pos);
	        	table.Url = EditorGUILayout.TextField("Table #"+pos+" Url:", table.Url);
				table.Name = EditorGUILayout.TextField("Table #"+pos+" Name:", table.Name);
			}
			
			GUILayout.EndScrollView();
	        if (GUI.changed)
	        {
	            SaveSettings();
	        }
			
	        if (GUILayout.Button("Import database from google spreadsheets"))
	        {
				tableIndex = 0;
				import = true;
				log = "";
	        }
	        if (!string.IsNullOrEmpty(lastError))
	        {
	            Rect rec = GUILayoutUtility.GetLastRect();
	            GUI.color = Color.red;
	            EditorGUI.DropShadowLabel(new Rect(0, rec.yMin + 15, 200, 20), "Last error: " + lastError);
	            GUI.color = Color.white;
	        }
	
	
	        GUILayout.Space(25);
	        GUILayout.Label("For more information please visit SQLiteKit forum.", EditorStyles.miniLabel);
	        if (GUILayout.Button("Open forum"))
	        {
	            Application.OpenURL("http://forum.unity3d.com/threads/150778-SQLiteKit");
	        }
	        GUILayout.Label("Add Example setup.", EditorStyles.miniLabel);
	        if (GUILayout.Button("Add Example setup"))
	        {
	            int pos = settings.TableCount;
				settings.TableCount++;
				table = settings.GetTableAt(pos);
				if(table!=null)			
				{
		        	table.Url = default_url;
					table.Name = "test_table_"+pos;
				}
				SaveSettings();
	        }
	        if (GUILayout.Button("Open Example Google Spreadsheet"))
	        {
	            Application.OpenURL("https://docs.google.com/spreadsheet/ccc?key=0AhKBA3PeUOmddDhQNno2S09VYlpvUC1XdXdMb1NjeHc#gid=0");
	        }

		}

        GUILayout.EndScrollView();
    }
	


    static List<string> GetCVSLines(string data)
    {
        List<string> lines = new List<string>();
        int i = 0;
        int searchCloseTags = 0;
        int lastSentenceStart = 0;
        while (i < data.Length)
        {
            if (data[i] == '"')
            {
                if (searchCloseTags == 0)
                    searchCloseTags++;
                else
                    searchCloseTags--;
            }
            else if (data[i] == '\n')
            {
                if (searchCloseTags == 0)
                {
                    lines.Add(data.Substring(lastSentenceStart, i - lastSentenceStart));
                    lastSentenceStart = i + 1;
                }
            }
            i++;
        }
        if (i - 1 > lastSentenceStart)
        {
            lines.Add(data.Substring(lastSentenceStart, i - lastSentenceStart));
        }
        return lines;
    }


    static List<string> GetCVSLine(string line)
    {
        List<string> list = new List<string>();
        int i = 0;
        int searchCloseTags = 0;
        int lastEntryBegin = 0;
        while (i < line.Length)
        {
            if (line[i] == '"')
            {
                if (searchCloseTags == 0)
                    searchCloseTags++;
                else
                    searchCloseTags--;
            }
            else if (line[i] == ',')
            {
                if (searchCloseTags == 0)
                {
                    list.Add(StripQuotes(line.Substring(lastEntryBegin, i - lastEntryBegin)));
                    lastEntryBegin = i + 1;
                }
            }
            i++;
        }
        if (line.Length > lastEntryBegin)
        {
            list.Add(StripQuotes(line.Substring(lastEntryBegin)));//Add last entry
        }
        return list;
    }

    //Remove the double " that CVS adds inside the lines, and the two outer " as well
    static string StripQuotes(string input)
    {
        if (input.Length < 1 || input[0] != '"') return input;//Not a " formatted line

        string output = ""; ;
        int i = 1;
        bool allowNextQuote = false;
        while (i < input.Length - 1)
        {
            string curChar = input[i] + "";
            if (curChar == "\"")
            {
                if (allowNextQuote)
                    output += curChar;
                allowNextQuote = !allowNextQuote;
            }
            else
            {
                output += curChar;
            }
            i++;
        }
        return output;
    }
	
	public class Column
	{
		enum Type {
			Undefined,
			Text,
			Integer
		}
		
		Type type = Type.Undefined;
		string name;
		int clmnPos;
		
		public static bool IsColumnHeader(string header)
		{
			return Regex.IsMatch(header, @"\s*(:string|:integer)",
                                     RegexOptions.IgnoreCase);	
		}
		
		public string Name {get {return name;}}
		public int Pos {get {return clmnPos;}}
		
		public string Definition { 
			get
			{
				string def = "";
				switch(type)
				{
				case Type.Text:
					def = name + " TEXT";
					break;
				case Type.Integer:
					def = name + " INT";
					break;
				}
				return def;
			}
		}
		
		public Column(string def, int pos)
		{
			string typedef = def.Split(':')[1];
			if(String.Compare(typedef,"integer")==0)
			{
				type = Type.Integer;
			}
			else if(String.Compare(typedef,"string")==0)
			{
				type = Type.Text;
			}

			name = def.Split(':')[0];
			clmnPos = pos;
		}
		
		public bool Bind( SQLiteQuery qr, string strValue )
		{
			if(String.Compare(strValue,":null",true)==0)
			{
				qr.BindNull();
			}
			else
			{
				switch(type)
				{
				case Type.Text:
					qr.Bind(strValue);
					break;
				case Type.Integer:
					qr.Bind(int.Parse(strValue));
					break;
				default:
					return false;
				}
			}
			return true;
		}
	}
	
}
