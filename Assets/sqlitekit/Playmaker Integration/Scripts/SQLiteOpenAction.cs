using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("SQLiteKit")]
	[Tooltip("Open SQLite database.")]
	public class SQLiteOpen : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.FsmString)]
		[Tooltip("Shortcut name of database")]
		public FsmString shortcutName;
		
		[RequiredField]
		[UIHint(UIHint.FsmString)]
		[Tooltip("Database filename")]
		public FsmString filename;	
		
		[Tooltip("Database is opened successfully")]
		public FsmEvent onSuccess;
		
		[Tooltip("Database opening is failed")]
		public FsmEvent onFail;
		
		private ThreadQueue.TaskControl taskCtrl = null;
		
		public override void Reset()
		{
			if( taskCtrl != null ){
				taskCtrl.Cancel();
				taskCtrl = null;
			}
		}
		
		public override void OnEnter()
		{
			string dbfilename = Application.persistentDataPath + "/" + filename.Value;
			taskCtrl = SQLiteManager.Instance.GetSQLiteAsync(shortcutName.Value).Open(dbfilename, OpenCallback, null);
		}
		
		public void OpenCallback(bool succeed, object state)
		{
			taskCtrl = null;
			
			if(succeed)
				Fsm.Event(onSuccess);
			else 
				Fsm.Event(onFail);
			
			Finish();
		}
	}
}