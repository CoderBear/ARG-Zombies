using UnityEngine;
using System.Collections;

public class offline_Object_Loader : MonoBehaviour {

	public GameObject buildingController;
	
	// Use this for initialization
	void Start () {
		/*
		 * if buildingController is found in scene
		 * -> don't load up building controller but instead use the function call in it's script to turn on all the buttons and mission panel
		 * if buildingcontroller is not found in scene
		 * -> instantiate the buildingController. building controller will load what else it needs from there.
		 * 
		 * destroy this object once that is done
		 */
		GameObject a_bController = GameObject.Find("Game_Building_Controller(Clone)");
		if(a_bController == null) {
			GameObject abc = (GameObject)Instantiate(buildingController, Vector3.zero, Quaternion.identity);
			///abc.
		}
		else {
			offlineButtonSpawn obs = a_bController.GetComponent<offlineButtonSpawn>();
			obs.reloadBuildings();
			//TODO
			//call game building controller's script to place the buildings back in the scene.
		}
		Destroy(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}