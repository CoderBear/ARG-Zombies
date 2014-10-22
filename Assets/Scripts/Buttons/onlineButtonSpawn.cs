using UnityEngine;
using System.Collections;
using SimpleJSON; 

public class onlineButtonSpawn : MonoBehaviour {
	 	
	struct buttonData {
		public int m_type;
		public Vector3 m_position;
//		public Vector3 m_deltaPosition;
		public int delta_x, delta_y;
		public Vector3 m_mapPosition;
		public Vector3 m_localPosition;
		public enum buildingType { shop, healing, empty};
		public bool visited;
	}
	
	public GameObject m_playerSprite;
	public GameObject m_missionParent;
	public GameObject m_emptyBuilding;
	public GameObject m_healing;
	public GameObject m_shop;
//	float randomRange_x = 300;
//	float randomRange_y = 225;
	
	buttonData [] m_buildings;
	GameObject [] m_instBuildings;
	int maxBuildings = 20;
	
	Vector3 playerLoc = new Vector3(37.68774f,-121.8961f,0.0f);
	float center_avg_x;
	
#region GooglePlaces Fields
	JSONNode pData;
	static int Neversleep;
	LocationInfo currentGPSPosition;
	float radarRadius;
	string APIkey, radarSensor;
	string googleRespStr;
#endregion

	void Awake() {
		// Initialize GooglePlaces Variables
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		radarRadius = 500f;
		APIkey = "AIzaSyC0hSk_GN1skCDphwYspPdKs9e5GQ4-fbs";
		radarSensor = "false";
		
		m_buildings = new buttonData[maxBuildings];
		m_instBuildings = new GameObject[maxBuildings];
		RefreshPlaces();
	}
	
	// Use this for initialization
	void Start() {
		//scene loads with this object every time. create a check to make sure there's no otehrs and if there is, have start delete this one.
		DontDestroyOnLoad(this.gameObject);//ensures this game object stays throughout the session of the game after it reaches this point
		
		// Initialize GooglePlaces Variables
//		Screen.sleepTimeout = SleepTimeout.NeverSleep;
//		radarRadius = 100f;
//		APIkey = "AIzaSyC0hSk_GN1skCDphwYspPdKs9e5GQ4-fbs";
//		radarSensor = "false";
//		
//		m_buildings = new buttonData[maxBuildings];
//		m_instBuildings = new GameObject[maxBuildings];
		m_playerSprite = GameObject.Find("Sprite - Player Icon");
		setMissionParent();
		
//		RefreshPlaces();
//		pData = new JSONNode();
//		StopCoroutine(Radar());
		
//		Debug.Log(pData.ToString());
//		Debug.Log(pData["results"][0]["geometry"]["location"]["lat"].AsFloat.ToString());

//		Invoke("setBuildingData",1f);
//		Invoke("loadBuildingData",1f);
		setBuildingData(pData);
		loadBuildingData();
//		yield return null;
	}
	
	// Update is called once per frame
	void Update () {
	}
	
#region Building Data Methods
	private void setBuildingData(JSONNode data) {
//		pData = JSON.Parse(googleRespStr);
		float x = 0.0f, y = 0.0f;
		for(int i = 0; i < maxBuildings; ++i) {
//			Debug.Log("Now Initialize buttonData #" + (i+1));
			m_buildings[i].visited = false;
//			Debug.Log("parsed lat for buttonData #" + (i+1) + " is " + data["results"][i]["geometry"]["location"]["lat"].AsFloat);
			x = data["results"][i]["geometry"]["location"]["lng"].AsFloat;
//			Debug.Log("stored lat for buttonData #" + (i+1) + " is " + x);
			y = data["results"][i]["geometry"]["location"]["lat"].AsFloat;
			m_buildings[i].m_position = new Vector3(x,y,0.0f);
//			m_buildings[i].m_deltaPosition = playerLoc - m_buildings[i].m_position;
			m_buildings[i].m_mapPosition = new Vector3(MapUtils.LonToX(m_buildings[i].m_position.x), MapUtils.LatToY(m_buildings[i].m_position.y),0.0f);
			m_buildings[i].delta_x = SetDeltaX(m_buildings[i].m_mapPosition,13);
			m_buildings[i].delta_y = SetDeltaY(m_buildings[i].m_mapPosition,13);
//			m_buildings[i].m_localPosition = new Vector3(MapUtils.LonToX(data["results"][i]["geometry"]["location"]["lat"].AsFloat), MapUtils.LatToY(data["results"][i]["geometry"]["location"]["lng"].AsFloat), 0.0f);
			m_buildings[i].m_localPosition = new Vector3(MapUtils.adjustLonByPixels(m_buildings[i].m_position.x, m_buildings[i].delta_x, 18), MapUtils.adjustLatByPixels(m_buildings[i].m_mapPosition.y,m_buildings[i].delta_y,18),0.0f);
//			m_buildings[i].m_localPosition.x -= (UICamera.mainCamera.rect.width/2);
			Debug.Log("local X for buttonData #" + (i+1) + " is " + m_buildings[i].m_localPosition.x);
			Debug.Log("local position for buttonData #" + (i+1) + " is " + m_buildings[i].m_localPosition);
			m_buildings[i].m_type = setBuildingType(data["results"][i]["types"][0].ToString());
//			Debug.Log("Type for buttonData #" + (i+1) + " is " + m_buildings[i].m_type);
		}
//		loadBuildingData();
	}
	
	private void loadBuildingData() {
		Vector3 pos = new Vector3();
		for(int i = 0; i < maxBuildings; ++i) {
			Debug.Log("Now Initialize building Object #" + (i+1));
			switch(m_buildings[i].m_type) {
			case 0: // shop
			m_instBuildings[i] = NGUITools.AddChild(m_missionParent, m_shop);
			m_instBuildings[i].GetComponent<UISprite>().depth = 15;
			pos.x = m_buildings[i].m_localPosition.x * m_instBuildings[i].transform.localScale.x;
			pos.y = m_buildings[i].m_localPosition.y * m_instBuildings[i].transform.localScale.y;
			m_instBuildings[i].transform.localPosition = pos;
//			m_instBuildings[i].transform.localPosition = UICamera.mainCamera.WorldToViewportPoint(m_buildings[i].m_localPosition);
			break;
			case 1: // healing
			m_instBuildings[i] = NGUITools.AddChild(m_missionParent, m_healing);
			pos.x = m_buildings[i].m_localPosition.x * m_instBuildings[i].transform.localScale.x;
			pos.y = m_buildings[i].m_localPosition.y * m_instBuildings[i].transform.localScale.y;
			m_instBuildings[i].transform.localPosition = pos;
			m_instBuildings[i].GetComponent<UISprite>().depth = 15;
			break;
			case 2: // empty building
			m_instBuildings[i] = NGUITools.AddChild(m_missionParent, m_emptyBuilding);
			Debug.Log("Before position for buttonData #" + (i+1) + " is " + m_buildings[i].m_localPosition);
			pos.x = m_buildings[i].m_localPosition.x * m_instBuildings[i].transform.localScale.x;
			Debug.Log("X after scaling for buttonData #" + (i+1) + " is " + pos.x);
			pos.y = m_buildings[i].m_localPosition.y * m_instBuildings[i].transform.localScale.y;
			m_instBuildings[i].transform.localPosition = pos;
			m_instBuildings[i].GetComponent<UISprite>().depth = 15;
			Debug.Log("After position for buttonData #" + (i+1) + " is " + m_instBuildings[i].transform.localPosition);
			break;
			}
		}
	}
	
	public void reloadBuildings() {
		setMissionParent();
		for(int i = 0; i < maxBuildings; ++i) {
			switch(m_buildings[i].m_type) {
				case 0: // shop
					m_instBuildings[i] = NGUITools.AddChild(m_missionParent, m_shop);
					m_instBuildings[i].transform.localPosition = m_buildings[i].m_localPosition;
					break;
				case 1: // healing
					m_instBuildings[i] = NGUITools.AddChild(m_missionParent, m_healing);
					m_instBuildings[i].transform.localPosition = m_buildings[i].m_localPosition;
					break;
				case 2: // empty building
					m_instBuildings[i] = NGUITools.AddChild(m_missionParent, m_emptyBuilding);
					m_instBuildings[i].transform.localPosition = m_buildings[i].m_localPosition;
					break;
			}
		}
	}
#endregion

#region Buolding Check Methods
	private int setBuildingType(string type) {
		switch(type) {
		case "clothing_store":
		case "convenience_store":
		case "department_store":
		case "electronics_store":
		case "grocery_or_supermarket":
		case "hardware_store":
		case "shopping_mall":
		case "store":
			return (int)buttonData.buildingType.shop;
		case "dentist":
		case "doctor":
		case "health":
		case "hospital":
		case "pharmacy":
		case "physiotherapist":
			return (int)buttonData.buildingType.healing;
		default:
			return (int)buttonData.buildingType.empty;
		}
	} 
#endregion

	public bool setMissionParent() {
		m_missionParent = GameObject.Find("Panel - Main Panel");
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

#region GooglePlaces Methods
	void RetrieveGPSData()
	{
		currentGPSPosition = Input.location.lastData;
	}
	
	public void RefreshPlaces() {
		RetrieveGPSData();
		var radarURL = string.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=37.68774,-121.8961&radius={0}&sensor=false&key={1}", radarRadius, APIkey);
		using (WWW googleResp = new WWW(radarURL))
		{
			while(!googleResp.isDone) {}
			
			googleRespStr = googleResp.text;
			Debug.Log(googleRespStr);
			pData = JSON.Parse(googleRespStr);
		}
		
		int count = 0; float lng_sum = 0.0f;
		
		for(int i = 0; i < pData.Count; ++i) {
			lng_sum += pData["results"][i]["geometry"]["location"]["lng"].AsFloat;
			count++;
		}
		
		center_avg_x = lng_sum/(float)count;
	}
	
	int SetDeltaX(Vector3 pos, int zoom) {
//		int xLoc = MapUtils.LonToX(playerLoc.x);
		int xLoc = MapUtils.LonToX(center_avg_x);
		return ((int)pos.x - xLoc) >> (21 - zoom);
	}
	
	int SetDeltaY(Vector3 pos, int zoom) {
		int yLoc = MapUtils.LatToY(playerLoc.y);
		return ((int)pos.y - yLoc) >> (21 - zoom);
	}
#endregion
			
#region Coroutine Methods
	IEnumerator Radar ()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		string radarURL = "https://maps.googleapis.com/maps/api/place/radarsearch/json?location=" + currentGPSPosition.latitude + "," + currentGPSPosition.longitude + "&radius=" + radarRadius + "&types=" + radarType + "&sensor=false" + radarSensor + "&key=" + APIkey;
#else
		string radarURL = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=37.68774,-121.8961&radius=" + radarRadius + "&sensor=false&key=" + APIkey;
#endif
		WWW googleResp = new WWW(radarURL);
		yield return googleResp;
		googleRespStr = googleResp.text;
//		Debug.Log(googleRespStr);
//		pData = JSON.Parse(googleResp.text);
//		Debug.Log(pData.ToString());
//		Debug.Log(pData["results"].ToString());
//		Debug.Log(pData["results"][0]["geometry"].ToString());
//		Debug.Log(pData["results"][0]["geometry"]["location"]["lat"].ToString());
//		Debug.Log(pData["results"][0]["geometry"]["location"]["lat"].AsFloat.ToString());
//		setBuildingData(pData);
//		StopCoroutine(Radar());
//		yield return null;
	}
#endregion
}