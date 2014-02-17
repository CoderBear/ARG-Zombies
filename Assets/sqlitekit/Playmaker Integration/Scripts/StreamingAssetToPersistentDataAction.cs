using UnityEngine;
using System;
using System.IO;
using System.Collections;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("SQLiteKit")]
	[Tooltip("Open SQLite database.")]
	public class StreamingAssetToPersistentData : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.FsmString)]
		[Tooltip("StreamingAsset relative filename of database")]
		public FsmString streamingAssetFilename;
		
		[RequiredField]
		[UIHint(UIHint.FsmString)]
		[Tooltip("Copied database file full path")]
		public FsmString persistentFilename;	
		
		[Tooltip("Overwrite destivation file.")]
		[RequiredField]
		public FsmBool overwrite;
		
		[Tooltip("On succeeded file copy.")]
		public FsmEvent onSuccess;
		
		[Tooltip("On file copy fail.")]
		public FsmEvent onFail;
		
		WWW www;
		byte[] bytes;
		string filename;
		
		public override void Reset()
		{
			overwrite = false;
			onSuccess = null;
			onFail = null;
		}
		
		public override void OnUpdate ()
		{
			base.OnUpdate ();
			
			if(www != null)
			{
				if(www.isDone)
				{
					if(www.error != null)
					{
						Fsm.Event(onFail);
					}
					else
					{
						bytes = www.bytes;
					}
					www = null;
				}
			}
			
			if ( bytes != null )
			{
				try{	
					
					//
					//
					// copy database to real file into cache folder
					using( FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write) )
					{
						fs.Write(bytes,0,bytes.Length);             
					}
					
					Fsm.Event(onSuccess);
					
				} catch (Exception e){
					Debug.LogError(e.ToString());
					
					Fsm.Event(onFail);
				}
			}
		
		}

		public override void OnEnter()
		{
			
			// persistant database path.
			filename = Application.persistentDataPath + "/" + persistentFilename;
				
			if(overwrite.Value && File.Exists(filename))
			{
				File.Delete(filename);
			}
			
			// check if database already exists.
			if(!File.Exists(filename))
			{
				string dbfilename = streamingAssetFilename.Value;				
	#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
				string dbpath = "file://" + Application.streamingAssetsPath + "/" + dbfilename;
				www = new WWW(dbpath);
	#elif UNITY_WEBPLAYER
				string dbpath = "StreamingAssets/" + dbfilename;
				www = new WWW(dbpath);
	#elif UNITY_IPHONE
				string dbpath = Application.dataPath + "/Raw/" + dbfilename;				
				try{	
					using ( FileStream fs = new FileStream(dbpath, FileMode.Open, FileAccess.Read, FileShare.Read) ){
						bytes = new byte[fs.Length];
						fs.Read(bytes,0,(int)fs.Length);
					}			
				} catch (Exception e){
					Debug.LogError(e.ToString());
					Fsm.Event(onFail);
				}
	#elif UNITY_ANDROID
				string dbpath = Application.streamingAssetsPath + "/" + dbfilename;	
				www = new WWW(dbpath);
	#endif			
			}
			else
			{
				Fsm.Event(onFail);
				Finish();
			}
		}
		
	}
}