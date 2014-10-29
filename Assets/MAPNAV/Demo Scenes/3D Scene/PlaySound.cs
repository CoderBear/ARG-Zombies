using UnityEngine;
using System.Collections;

public class PlaySound : MonoBehaviour {

	void OnTriggerEnter (Collider other) {
		if(other.tag == "Player"){
			audio.Play();
		}
	}
	void OnTriggerExit (Collider other) {
		if(other.tag == "Player"){
			audio.Stop();
		}
	}
}
