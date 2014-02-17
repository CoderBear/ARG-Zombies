using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	
	[ActionCategory("SQLiteKit")]
	[Tooltip("Provide bind reference for SQLiteRead.")]
	public class SQLiteBindFloat : FsmStateAction
	{
		
		public FsmFloat floatValue;

		
		public override void Reset()
		{
		}

		public override void OnEnter()
		{
			Finish();
		}
		
	}
	

}