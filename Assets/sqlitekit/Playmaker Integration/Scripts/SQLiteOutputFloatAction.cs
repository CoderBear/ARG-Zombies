using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory("SQLiteKit")]
	[Tooltip("Result string value for selection.")]
	public class SQLiteOutputFloat : FsmStateAction
	{

		[UIHint(UIHint.FsmString)]
		[Tooltip("Field name")]
		public FsmString fieldName;
		
		[UIHint(UIHint.FsmString)]
		[Tooltip("Result value")]
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