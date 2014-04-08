using UnityEngine;
using System.Collections;

public class offlineButtonSpawn : MonoBehaviour {

	public GameObject m_emptyBuilding;
	public GameObject m_healing;
	public GameObject m_shop;
	public GameObject m_NGUIMissionparent;

	GameObject [] m_buildings;
	int maxBuildings = 5;
	//bool ranOnce = false;
	// Use this for initialization
	void Start () {
		m_buildings = new GameObject [maxBuildings];

		//testing purposes. load buildings manually into array.
		Vector3 a_position = new Vector3(Random.Range(-300, 300), Random.Range(-225, 225), 0);
		//Debug.Log("a_position's random X: "+ a_position.x +" random Y: "+ a_position.y);
		m_buildings[0] = NGUITools.AddChild(m_NGUIMissionparent, m_emptyBuilding);//(GameObject)Instantiate(m_emptyBuilding, Vector3.zero, Quaternion.identity);
		//a_position = new Vector3(Random.Range(-300, 300), Random.Range(-225, 225), 0);
		//m_buildings[1] = (GameObject)Instantiate(m_healing, a_position, Quaternion.identity);
		m_buildings[0].transform.localPosition = a_position;
	}

//	void runOnce() {
//		m_buildings[0].transform.position = new Vector3(Random.Range(-300, 300), Random.Range(-225, 225), 0);
//
//		ranOnce = true;
//	}

	// Update is called once per frame
	void Update () {
		//Debug.Log("building 0 transform X: "+ m_buildings[0].transform.position.x +" Y: "+ m_buildings[0].transform.position.y);
		//if(!ranOnce) {runOnce();}
	}
}
