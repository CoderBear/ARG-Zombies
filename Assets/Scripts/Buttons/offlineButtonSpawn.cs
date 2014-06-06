using UnityEngine;
using System.Collections;

struct buttonData {
	public int m_type;
	public Vector3 m_localPosition;
	public enum buildingType { shop, healing, empty};
}

public class offlineButtonSpawn : MonoBehaviour {
	private Player player;
	void Awake() {
		// set local private player object to the singleton player.
		player = GameObject.FindWithTag ("Player").GetComponent<Player>();
	}

	public GameObject m_missionParent;
	public GameObject m_emptyBuilding;
	public GameObject m_healing;
	public GameObject m_shop;
	float randomRange_x = 300;
	float randomRange_y = 225;

	buttonData [] m_buildings;
	GameObject [] m_instBuildings;
	int maxBuildings = 5;

	void setRandomBuildingData() {
		m_buildings[0].m_type = (int)buttonData.buildingType.empty;//first building gets instantiated as empty
		m_buildings[0].m_localPosition = new Vector3(Random.Range(-randomRange_x, randomRange_x), Random.Range(-randomRange_y, randomRange_y), 0);
		for(int i = 1; i < maxBuildings; ++i) {
			float a_buildValue = Random.value;
			if( 0 <= a_buildValue && a_buildValue < 0.20) {//set healing
				//TODO
				//Debug.log the values recieved from converting the enum over to an int to see what that value is.
				m_buildings[i].m_type = (int)buttonData.buildingType.healing;
				m_buildings[i].m_localPosition = new Vector3(Random.Range(-randomRange_x, randomRange_x), Random.Range(-randomRange_y, randomRange_y), 0);
			}
			else if( 0.20 <= a_buildValue && a_buildValue < 0.40) {//load shop 20%
				m_buildings[i].m_type = (int)buttonData.buildingType.shop;
				m_buildings[i].m_localPosition = new Vector3(Random.Range(-randomRange_x, randomRange_x), Random.Range(-randomRange_y, randomRange_y), 0);
			}
			else { //load default empty building
				m_buildings[i].m_type = (int)buttonData.buildingType.empty;
				m_buildings[i].m_localPosition = new Vector3(Random.Range(-randomRange_x, randomRange_x), Random.Range(-randomRange_y, randomRange_y), 0);
			}

		}
	}
	/// <summary>
	/// loads five random buildings, guaranteing that one is an empty building while the others could be healing or a shop.
	/// </summary>
	void loadRandomBuildings() {
		for(int range = 0; range < maxBuildings; ++range) {
			if(m_buildings[range].m_type == (int)buttonData.buildingType.empty) {
				m_instBuildings[range] = NGUITools.AddChild(m_missionParent, m_emptyBuilding);
				m_instBuildings[range].transform.localPosition = m_buildings[range].m_localPosition;
				if(range != 0) { checkBuildingPosition(range);}
			}
			else if(m_buildings[range].m_type == (int)buttonData.buildingType.healing) {
				m_instBuildings[range] = NGUITools.AddChild(m_missionParent, m_healing);
				m_instBuildings[range].transform.localPosition = m_buildings[range].m_localPosition;
				if(range != 0) { checkBuildingPosition(range);}
			}
			else {
				m_instBuildings[range] = NGUITools.AddChild(m_missionParent, m_shop);
				m_instBuildings[range].transform.localPosition = m_buildings[range].m_localPosition;
				if(range != 0) { checkBuildingPosition(range);}
			}
		}
	}

	/// <summary>
	/// loads all buildings into the scene again. called after object is created but only after the first scene change returns back to the offline map
	/// </summary>
	public void reloadBuildings() {
		setMissionParent();//re-set's the mission parent. not doing so reloads all buttons at 0,0,0 local position in random ngui object
		for(int range = 0; range < maxBuildings; ++range) {
			if(m_buildings[range].m_type == (int)buttonData.buildingType.empty) {
				m_instBuildings[range] = NGUITools.AddChild(m_missionParent, m_emptyBuilding);
				m_instBuildings[range].transform.localPosition = m_buildings[range].m_localPosition;
			}
			else if(m_buildings[range].m_type == (int)buttonData.buildingType.healing) {
				m_instBuildings[range] = NGUITools.AddChild(m_missionParent, m_healing);
				m_instBuildings[range].transform.localPosition = m_buildings[range].m_localPosition;
			}
			else {
				m_instBuildings[range] = NGUITools.AddChild(m_missionParent, m_shop);
				m_instBuildings[range].transform.localPosition = m_buildings[range].m_localPosition;
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
			if( isBuildingOverlaping(m_instBuildings[checker], m_instBuildings[a_range])) {
				Debug.Log("Building overlap on building create #"+ a_range);
				checker = -1;
				Vector3 newPos = new Vector3(Random.Range(-randomRange_x, randomRange_x), Random.Range(-randomRange_y, randomRange_y), 0);
				m_instBuildings[a_range].transform.localPosition = newPos;
				m_buildings[a_range].m_localPosition = newPos;
			}
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

	public bool setMissionParent() {
		m_missionParent = GameObject.Find("Panel - Missions");
		if(m_missionParent == null) { return false;}//returns false if mission parent fails to be set.
		return true;//else return true that mission parent is set
	}

	public void destroyThis() {
		for(int i = 0; i < maxBuildings; ++i) {
			Destroy( m_instBuildings[i]);
			m_instBuildings[i] = null;
		}
		Destroy(gameObject);
	}

	void Start () {
		if (player.OfflineMode)
		{
			//scene loads with this object every time. create a check to make sure there's no otehrs and if there is, have start delete this one.
			DontDestroyOnLoad(this.gameObject);//ensures this game object stays throughout the session of the game after it reaches this point

			m_buildings = new buttonData [maxBuildings];
			m_instBuildings = new GameObject [maxBuildings];
			setMissionParent();

			setRandomBuildingData();
			loadRandomBuildings();
		}
	}

	// Update is called once per frame
	void Update () {
//		if(Input.GetKeyDown(KeyCode.Space)) {//testing button stuff
//			toggleButtonRender();
//		}
	}
}