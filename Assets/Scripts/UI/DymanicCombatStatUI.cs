using UnityEngine;
using System.Collections;

public class DymanicCombatStatUI : MonoBehaviour {

	private Player goPlayer;
	public UILabel label;

	void Awake() {
		// set local private player object to the singleton player.
		goPlayer = GameObject.FindWithTag ("Player").GetComponent<Player>();
	}

	// If the progress bar value changed reflect in the label
	public void OnHealthValueUpdated() {
		label.text = goPlayer.getHPinfo();
	}

	public void OnEnergyValueUpdate() {
		label.text = goPlayer.getMPinfo();
	}
}