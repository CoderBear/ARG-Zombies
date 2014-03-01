using UnityEngine;
using System.Collections;

public class combatResultsButton : MonoBehaviour {

	public GameObject goExplore, goResults;
	public UILabel labelXP, labelMoney;

	// Use this for initialization
	void Start () {
		labelXP.text = "0";
		labelMoney.text = "0";
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnClick() {
		goExplore.SetActive (true);
		goResults.SetActive (false);
	}
}