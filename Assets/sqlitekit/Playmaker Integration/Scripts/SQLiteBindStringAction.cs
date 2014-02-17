using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("SQLiteKit")]
	[Tooltip("Provide bind reference for SQLiteRead.")]
	public class SQLiteBindString : FsmStateAction
	{
		
		public FsmString stringValue;

		
		public override void Reset()
		{
		}

		public override void OnEnter()
		{
			Finish();
		}
		
	}
	

}