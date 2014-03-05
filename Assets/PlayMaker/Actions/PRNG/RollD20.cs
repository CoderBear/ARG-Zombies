using UnityEngine;
using HutongGames.PlayMaker;
using NPack;

[ActionCategory("PRNG")]
[Tooltip("Rolls a 20 sided dice using the Mersenne Twister.")]
public class RollD20 : FsmStateAction
{
	private const int DICE_VALUE_MIN = 1;
	private const int DICE_VALUE_MAX = 20;

	[RequiredField]
	[UIHint(UIHint.Variable)]
	public FsmInt storeResult;

	// Code that runs on entering the state.
	public override void OnEnter()
	{	
		MersenneTwister rand = new MersenneTwister ();
		storeResult = rand.Next (DICE_VALUE_MIN, DICE_VALUE_MAX);
	}
}