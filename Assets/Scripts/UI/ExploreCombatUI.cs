using UnityEngine;
using System.Collections;

public class ExploreCombatUI : MonoBehaviour {

	public GameObject goExplore, goCombat, goResults, goBattle;

	// make sure the right screen is shown for the active
	// action being taken
	public void SwitchScreenUI(int i) {
		switch (i) {
		case 0: // Explore UI
			goExplore.SetActive (true);
			goResults.SetActive (false);
			break;
		case 1: // Combat UI
			goExplore.SetActive(false);
			goCombat.SetActive(true);
			break;
		case 2: // Battle UI
			goCombat.SetActive(false);
			goBattle.SetActive(true);
			break;
		case 3: // Results UI
			goResults.SetActive(true);
			goCombat.SetActive (false);
			break;
		default:
			break;
		}
	}
}