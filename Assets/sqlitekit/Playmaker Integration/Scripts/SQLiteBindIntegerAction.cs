using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("SQLiteKit")]
	[Tooltip("Provide bind reference for SQLiteRead.")]
	public class SQLiteBindInteger : FsmStateAction
	{
		
		public FsmInt integerValue;

		
		public override void Reset()
		{
		}

		public override void OnEnter()
		{
			Finish();
		}
		
	}
	

}