using UnityEngine;
using System.Collections;

public class combatResultsButton : MonoBehaviour {
	public GameObject goExplore, goResults;
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
		goExplore.SetActive (true);
		goResults.SetActive (false);
	}
}