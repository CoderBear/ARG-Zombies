using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("SQLiteKit")]
	[Tooltip("Close SQLite database.")]
	public class SQLiteClose : FsmStateAction
	{

		[RequiredField]
		[UIHint(UIHint.FsmString)]
		[Tooltip("Shortcut name of database")]
		public FsmString shortcutName;
		
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
			taskCtrl = SQLiteManager.Instance.GetSQLiteAsync(shortcutName.Value).Close(CloseCallback, null);
		}
		
		public void CloseCallback(object state)
		{
			taskCtrl = null;
			Finish();
		}
	}
}