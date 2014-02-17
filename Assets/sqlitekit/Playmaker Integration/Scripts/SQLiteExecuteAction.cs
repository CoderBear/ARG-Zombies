using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("SQLiteKit")]
	[Tooltip("Execute query for selected database.")]
	public class SQLiteExecute : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.FsmString)]
		[Tooltip("Shortcut name of database")]
		public FsmString shortcutName;

		[RequiredField]
		[UIHint(UIHint.FsmString)]
		[Tooltip("SQL Query")]
		public FsmString sqlQuery;
		
		[Tooltip("Query step has result")]
		public FsmEvent onStep;
		
		[Tooltip("Can't execute query")]
		public FsmEvent onFail;

		private ThreadQueue.TaskControl taskCtrl = null;
		
		SQLiteQuery query = null;
		
		public override void Reset()
		{
			if( taskCtrl != null ){
				taskCtrl.Cancel();
				taskCtrl = null;
			}
		}

		public override void OnEnter()
		{
			if(query==null)
			{
				taskCtrl = SQLiteManager.Instance.GetSQLiteAsync(shortcutName.Value).Query(sqlQuery.Value, QueryComplete,null);
			}
			else
			{
				taskCtrl = SQLiteManager.Instance.GetSQLiteAsync(shortcutName.Value).Step(query,StepComplete,null);
			}
		}
		
		void QueryComplete(SQLiteQuery qr, object state)
		{
			query = qr;
			if(qr == null)
			{
				Fsm.Event(onFail);
			}
			else
			{
				foreach( FsmStateAction action in State.Actions )
				{
					SQLiteBindInteger bindInt = action as SQLiteBindInteger;
					if(bindInt != null)
					{
						query.Bind(bindInt.integerValue.Value);
					}
					
					SQLiteBindBool bindBool = action as SQLiteBindBool;
					if(bindBool != null)
					{
						query.Bind(bindBool.boolValue.Value ? 1:0);
					}
					
					SQLiteBindFloat bindFloat = action as SQLiteBindFloat;
					if(bindFloat != null)
					{
						query.Bind((double)bindFloat.floatValue.Value);
					}
					
					SQLiteBindString bindStr = action as SQLiteBindString;
					if(bindStr != null)
					{
						query.Bind(bindStr.stringValue.Value);
					}
				}
				
				taskCtrl = SQLiteManager.Instance.GetSQLiteAsync(shortcutName.Value).Step(query,StepComplete,null);
			}
		}
		
		void StepComplete(SQLiteQuery qr, bool rv, object state)
		{
			taskCtrl = null;
			if(rv){
				foreach( FsmStateAction action in State.Actions )
				{
					SQLiteOutputString outString = action as SQLiteOutputString;
					if(outString != null)
					{
						outString.stringValue.Value = query.GetString(outString.fieldName.Value);
					}
					SQLiteOutputBool outBool = action as SQLiteOutputBool;
					if(outBool != null)
					{
						outBool.boolValue.Value = query.GetInteger(outBool.fieldName.Value) > 0 ? true : false;
					}
					SQLiteOutputInteger outInt = action as SQLiteOutputInteger;
					if(outInt != null)
					{
						outInt.integerValue.Value = query.GetInteger(outInt.fieldName.Value);
					}
					SQLiteOutputFloat outFloat = action as SQLiteOutputFloat;
					if(outFloat != null)
					{
						outFloat.floatValue.Value = (float)query.GetDouble(outFloat.fieldName.Value);
					}
				}
				
				Fsm.Event(onStep);
			}
			else
			{
				query = null;
				Finish();
			}
		}
	}
}