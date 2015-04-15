using UnityEngine;
using System.Collections;

public class ExploredBuilding : MonoBehaviour {

	public int Index {
		get;
		set;
	}
	
	public bool Disabled
	{
		get;
		set;
	}
	
	Player player;
	
	void Awake () {
		// set local private player object to the singleton player.
		player = GameObject.FindWithTag ("Player").GetComponent<Player>();
	}
	
	// Use this for initialization
	void Start () {
	}
	
	void OnClick() {
		player.ExploredBuildingNumber = Index;
	}
}