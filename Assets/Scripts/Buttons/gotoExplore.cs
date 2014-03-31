using UnityEngine;
using System.Collections;

public class gotoExplore : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnClick() {
		// Future: will read the GO tag and load the correct building type
		//string tag = this.tag
		string tag = this.tag;
		switch(tag){
		case "Shop":
			//load shop
			Debug.Log("level Shop loaded from tag");
			break;
		case "Healing":
			//load healing level
			Debug.Log("Level Healing loaded from tag");
			break;
		case "Empty Building":
			//load random empty building
			Debug.Log("Level Empty Building loaded from tag");
			Application.LoadLevel ("gameBuilding01");//can be changed later if there are different empty buildings to go through.
			break;
		default:
			Debug.Log("Level load error: Tag "+tag+" non case match. default used");
			Application.LoadLevel ("gameBuilding01");
			break;
		}
	}
}