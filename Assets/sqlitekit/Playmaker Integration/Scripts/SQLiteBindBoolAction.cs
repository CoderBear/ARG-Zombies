using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("SQLiteKit")]
	[Tooltip("Provide bind reference for SQLiteRead.")]
	public class SQLiteBindBool : FsmStateAction
	{
		
		public FsmBool boolValue;

		
		public override void Reset()
		{
		}

		public override void OnEnter()
		{
			Finish();
		}
		
	}
	

}