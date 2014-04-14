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
	/// <summary>
	/// loads five random buildings, guaranteing that one is an empty building while the others could be healing or a shop.
	/// </summary>
	void loadRandomBuildings() {
		//testing purposes. load buildings manually into array.
		m_buildings[0] = NGUITools.AddChild(m_NGUIMissionparent, m_emptyBuilding);//(GameObject)Instantiate(m_emptyBuilding, Vector3.zero, Quaternion.identity);
		Vector3 a_position = new Vector3(Random.Range(-randomRange_x, randomRange_x), Random.Range(-randomRange_y, randomRange_y), 0);
		m_buildings[0].transform.localPosition = a_position;
		
		for(int range = 1; range < maxBuildings; ++range) {

			float a_buildingValue = Random.value;
			if( 0 <= a_buildingValue && a_buildingValue < 0.20) {//load healing building 20%
				m_buildings[range] = NGUITools.AddChild(m_NGUIMissionparent, m_healing);
				a_position = new Vector3(Random.Range(-randomRange_x, randomRange_x), Random.Range(-randomRange_y, randomRange_y), 0);
				m_buildings[range].transform.localPosition = a_position;
				checkBuildingPosition(range);
			}
			else if( 0.20 <= a_buildingValue && a_buildingValue < 0.40) {//load shop 20%
				m_buildings[range] = NGUITools.AddChild(m_NGUIMissionparent, m_shop);
				a_position = new Vector3(Random.Range(-randomRange_x, randomRange_x), Random.Range(-randomRange_y, randomRange_y), 0);
				m_buildings[range].transform.localPosition = a_position;
				checkBuildingPosition(range);
			}
			else {//load default empty building
				m_buildings[range] = NGUITools.AddChild(m_NGUIMissionparent, m_emptyBuilding);
				a_position = new Vector3(Random.Range(-randomRange_x, randomRange_x), Random.Range(-randomRange_y, randomRange_y), 0);
				m_buildings[range].transform.localPosition = a_position;
				checkBuildingPosition(range);
			}
		}
	}

	/// <summary>
	/// checks previous buildings in range against the current a_range (latest) instantiated building
	/// then moves the current building if there is any overlap
	/// </summary>
	/// <param name="a_range">A_range.</param>
	void checkBuildingPosition(int a_range) {

		for(int checker = 0; checker < a_range; ++checker) {
			if( isBuildingOverlaping(m_buildings[checker], m_buildings[a_range])) {
				Debug.Log("Building overlap on building create #"+ a_range);
				checker = -1;
				Vector3 newPos = new Vector3(Random.Range(-randomRange_x, randomRange_x), Random.Range(-randomRange_y, randomRange_y), 0);
				m_buildings[a_range].transform.localPosition = newPos;
			}
		}
	}

	/// <summary>
	/// Places the building at a_localPosition.
	/// </summary>
	/// <param name="a_localPosition">A_local position.</param>
	void placeBuildingAt(Vector3 a_localPosition, int a_building) {
		if(a_building >= 0 && a_building < maxBuildings) {
			m_buildings[a_building].transform.localPosition = a_localPosition;
		}
	}

	bool isBuildingOverlaping(GameObject buildingOne, GameObject buildingTwo) {
		BoxCollider bBoxOne = buildingOne.collider as BoxCollider;
		BoxCollider bBoxTwo = buildingTwo.collider as BoxCollider;
		double width	= (bBoxOne.size.x * 0.5) + (bBoxTwo.size.x * 0.5);
		double height	= (bBoxOne.size.y * 0.5) + (bBoxTwo.size.y * 0.5);
		if( buildingOne.transform.localPosition.x - width < buildingTwo.transform.localPosition.x
		   && buildingOne.transform.localPosition.x + width > buildingTwo.transform.localPosition.x) {
			//within range x, now check for y
			if( buildingOne.transform.localPosition.y - height < buildingTwo.transform.localPosition.y
			   && buildingOne.transform.localPosition.y + height > buildingTwo.transform.localPosition.y) {
				return true;
			}
		}

		return false;//if not overlapping. otherwise if statement will return true sooner
	}

	void Start () {
		m_buildings = new GameObject [maxBuildings];

		loadRandomBuildings();
	}



	// Update is called once per frame
	void Update () {

	}
}
