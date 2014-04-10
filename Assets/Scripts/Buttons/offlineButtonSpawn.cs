using UnityEngine;
using System.Collections;

public class offlineButtonSpawn : MonoBehaviour {

	public GameObject m_emptyBuilding;
	public GameObject m_healing;
	public GameObject m_shop;
	public GameObject m_NGUIMissionparent;
	float randomRange_x = 300;
	float randomRange_y = 225;

	GameObject [] m_buildings;
	int maxBuildings = 5;

	void loadRandomBuildings() {
		//testing purposes. load buildings manually into array.
		m_buildings[0] = NGUITools.AddChild(m_NGUIMissionparent, m_emptyBuilding);//(GameObject)Instantiate(m_emptyBuilding, Vector3.zero, Quaternion.identity);
		Vector3 a_position = new Vector3(Random.Range(-randomRange_x, randomRange_x), Random.Range(-randomRange_y, randomRange_y), 0);
		m_buildings[0].transform.localPosition = a_position;
		
		for(int range = 1; range < maxBuildings; ++range) {
			switch( (int)Random.value%3) {
				
			}
		}
	}

	void Start () {
		m_buildings = new GameObject [maxBuildings];
	}



	// Update is called once per frame
	void Update () {

	}
}
