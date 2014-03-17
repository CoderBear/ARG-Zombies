using UnityEngine;
using System.Collections;

public class combatResultsButton : MonoBehaviour {
	public ExploreCombatUI uiObject;
	public UILabel labelXP, labelMoney;

	public Player player;

	// Use this for initialization
	void Start () {
		labelXP.text = player.getLastXP().ToString ();
		labelMoney.text = player.getLastMoney().ToString();
	}	
	// Update is called once per frame
	void Update () {
	}

	void OnClick() {
		uiObject.SwitchScreenUI (0);
	}
}