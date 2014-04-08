using UnityEngine;
using System.Collections;

public class PlayerVictoryUI : MonoBehaviour {
	public UILabel labelXP, labelMoney;
	
	private Player player;

	// Use this for initialization
	void Start () {
		player = GameObject.FindWithTag ("Player").GetComponent<Player>();
		
		labelXP.text = player.getLastXP().ToString ();
		labelMoney.text = player.getLastMoney().ToString();
	}
	
	// Update is called once per frame
	void Update () {
	}
}