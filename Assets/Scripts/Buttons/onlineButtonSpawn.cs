using UnityEngine;
using System.Collections;
using SimpleJSON; 

public class OnlineButtonSpawn : MonoBehaviour {
	 	
	public struct ButtonData {
		public int Type;
		public Vector3 Position;
		public Vector3 DeltaPosition;
		public int DeltaX, DeltaY;
		public Vector3 MapPosition;
		public Vector3 LocalPosition;
		public enum BuildingType { Shop, Healing, Empty};
		public bool Visited;
	}
	
	public GameObject PlayerSprite;
	public GameObject MissionParent;
	public GameObject EmptyBuilding;
	public GameObject HealingBuilding;
	public GameObject ShopBuilding;
//	float randomRange_x = 300;
//	float randomRange_y = 225;
	
	public ButtonData [] m_buildings;
	GameObject [] m_instBuildings;
	int maxBuildings = 20;
	[SerializeField]int ZOOM = 13;
	[SerializeField]int SCALE = 3;
	
	Vector3 playerLoc = new Vector3(-121.891326f,37.6839f,0.0f);
	float center_avg_x, center_avg_y;
	
	Player player;
	
#region GooglePlaces Fields
	JSONNode pData;
	static int Neversleep;
	LocationInfo currentGPSPosition;
	float radarRadius;
	string APIkey;
	string googleRespStr;
#endregion

	void Awake() {
		player = GameObject.FindWithTag ("Player").GetComponent<Player>();
		// Initialize GooglePlaces Variables
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		radarRadius = 1000f;
		APIkey = "AIzaSyC0hSk_GN1skCDphwYspPdKs9e5GQ4-fbs";
		
		m_buildings = new ButtonData[maxBuildings];
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
//		PlayerSprite = GameObject.Find("Sprite - Player Icon");
		SetPlayerIcon();
		SetMissionParent();
		
		setBuildingData(pData);
		loadBuildingData();
	}
	
	// Update is called once per frame
	void Update () {
	}
	
#region Building Data Methods
	private void setBuildingData(JSONNode data) {
//		pData = JSON.Parse(googleRespStr);
		float x, y;
		for(int i = 0; i < maxBuildings; ++i) {
//			Debug.Log("Now Initialize buttonData #" + (i+1));
			m_buildings[i].Visited = false;
//			Debug.Log("parsed lat for buttonData #" + (i+1) + " is " + data["results"][i]["geometry"]["location"]["lat"].AsFloat);
			x = data["results"][i]["geometry"]["location"]["lng"].AsFloat;
//			Debug.Log("stored lng for buttonData #" + (i+1) + " is " + x);
			y = data["results"][i]["geometry"]["location"]["lat"].AsFloat;
//			Debug.Log("stored lat for buttonData #" + (i+1) + " is " + y);
			m_buildings[i].Position = new Vector3(x,y,0.0f);
//			m_buildings[i].m_deltaPosition = playerLoc - m_buildings[i].m_position;
#if UNITY_ANDROID&&!UNITY_EDITOR
			m_buildings[i].DeltaPosition = SetDeltaPositionAndroid(m_buildings[i].Position);
#else
			m_buildings[i].DeltaPosition = SetDeltaPosition(m_buildings[i].Position);
#endif
//			Debug.Log("DeltaPosition.X for buttonData #" + (i+1) + " is " + m_buildings[i].DeltaPosition.x);
			m_buildings[i].MapPosition = new Vector3(MapUtils.LonToX(m_buildings[i].Position.x), MapUtils.LatToY(m_buildings[i].Position.y),0.0f);
//			Debug.Log("Lat->Y for buttonData #" + (i+1) + " is " + m_buildings[i].m_mapPosition.y);
//			m_buildings[i].DeltaX = SetDeltaX(m_buildings[i].MapPosition,18);
			m_buildings[i].DeltaX = SetDeltaX(m_buildings[i].DeltaPosition,ZOOM);
//			Debug.Log("Delta X buttonData #" + (i+1) + " is " + m_buildings[i].DeltaX);
//			Debug.Log("Delta Y buttonData #" + (i+1) + " is " + m_buildings[i].delta_y);
//			m_buildings[i].delta_y = SetDeltaY(m_buildings[i].m_mapPosition,13);
			m_buildings[i].DeltaY = SetDeltaY(m_buildings[i].MapPosition,ZOOM);
//			m_buildings[i].m_localPosition = new Vector3(MapUtils.LonToX(data["results"][i]["geometry"]["location"]["lat"].AsFloat), MapUtils.LatToY(data["results"][i]["geometry"]["location"]["lng"].AsFloat), 0.0f);
//			m_buildings[i].LocalPosition = new Vector3(MapUtils.AdjustLonByPixels(m_buildings[i].Position.x, m_buildings[i].DeltaX, ZOOM), MapUtils.AdjustLatByPixels(m_buildings[i].MapPosition.y,m_buildings[i].DeltaY,18),0.0f);
			m_buildings[i].LocalPosition = new Vector3(MapUtils.AdjustLatByPixels(m_buildings[i].MapPosition.x, m_buildings[i].DeltaX, ZOOM), MapUtils.AdjustLatByPixels(m_buildings[i].MapPosition.y,m_buildings[i].DeltaY,ZOOM),0.0f);
//			m_buildings[i].m_localPosition.x -= (UICamera.mainCamera.rect.width/2);
//			Debug.Log("local X for buttonData #" + (i+1) + " is " + m_buildings[i].LocalPosition.x);
			m_buildings[i].LocalPosition.x = Mathf.RoundToInt(m_buildings[i].LocalPosition.x);
			m_buildings[i].LocalPosition.y = Mathf.RoundToInt(m_buildings[i].LocalPosition.y);
//			Debug.Log("local X for buttonData #" + (i+1) + " is " + m_buildings[i].LocalPosition.x + " after rounding to int");
			m_buildings[i].LocalPosition.x *= SCALE;
			m_buildings[i].LocalPosition.y *= SCALE;
//			Debug.Log("local X for buttonData #" + (i+1) + " is " + m_buildings[i].LocalPosition.x + " after scaling.");
//			Debug.Log("local Y for buttonData #" + (i+1) + " is " + m_buildings[i].m_localPosition.y);
//			Debug.Log("local position for buttonData #" + (i+1) + " is " + m_buildings[i].m_localPosition);
			m_buildings[i].Type = setBuildingType(data["results"][i]["types"][0].ToString());
//			Debug.Log("Type for buttonData #" + (i+1) + " is " + m_buildings[i].m_type);
		}
//		loadBuildingData();
	}
	
	private void loadBuildingData() {
		Vector3 pos = new Vector3();
		for(int i = 0; i < maxBuildings; ++i) {
//			Debug.Log("Now Initialize building Object #" + (i+1));
			switch(m_buildings[i].Type) {
			case 0: // shop
			m_instBuildings[i] = NGUITools.AddChild(MissionParent, ShopBuilding);
			m_instBuildings[i].GetComponent<UISprite>().depth = 15;
			pos.x = m_buildings[i].LocalPosition.x * m_instBuildings[i].transform.localScale.x;
			pos.y = m_buildings[i].LocalPosition.y * m_instBuildings[i].transform.localScale.y;
			m_instBuildings[i].transform.localPosition = pos;
//			m_instBuildings[i].transform.localPosition = UICamera.mainCamera.WorldToViewportPoint(m_buildings[i].m_localPosition);
			break;
			case 1: // healing
			m_instBuildings[i] = NGUITools.AddChild(MissionParent, HealingBuilding);
			pos.x = m_buildings[i].LocalPosition.x * m_instBuildings[i].transform.localScale.x;
			pos.y = m_buildings[i].LocalPosition.y * m_instBuildings[i].transform.localScale.y;
			m_instBuildings[i].transform.localPosition = pos;
			m_instBuildings[i].GetComponent<UISprite>().depth = 15;
			break;
			case 2: // empty building
			m_instBuildings[i] = NGUITools.AddChild(MissionParent, EmptyBuilding);
//			Debug.Log("X before scaling for buttonData #" + (i+1) + " is " + m_buildings[i].m_localPosition.x);
			pos.x = m_buildings[i].LocalPosition.x * m_instBuildings[i].transform.localScale.x;
//			Debug.Log("X after scaling for buttonData #" + (i+1) + " is " + pos.x);
			pos.y = m_buildings[i].LocalPosition.y * m_instBuildings[i].transform.localScale.y;
			m_instBuildings[i].transform.localPosition = pos;
			m_instBuildings[i].GetComponent<UISprite>().depth = 15;
			m_instBuildings[i].GetComponent<ExploredBuilding>().Index = i;
			m_instBuildings[i].GetComponent<ExploredBuilding>().Disabled = false;
//			Debug.Log("After position for buttonData #" + (i+1) + " is " + m_instBuildings[i].transform.localPosition.x);
			break;
			}
		}
	}
	
	public void ReloadBuildings() {
		// Mark a building as explored.
		if(player.ExploredBuildingComplete) {
			m_buildings[player.ExploredBuildingNumber].Visited = true;
			player.ExploredBuildingComplete = false;
		}
		SetMissionParent();
		SetPlayerIcon();
		for(int i = 0; i < maxBuildings; ++i) {
			switch(m_buildings[i].Type) {
				case 0: // shop
					m_instBuildings[i] = NGUITools.AddChild(MissionParent, ShopBuilding);
					m_instBuildings[i].GetComponent<ExploredBuilding>().Index = i;
					m_instBuildings[i].GetComponent<UISprite>().depth = 15;
					m_instBuildings[i].transform.localPosition = m_buildings[i].LocalPosition;
					break;
				case 1: // healing
					m_instBuildings[i] = NGUITools.AddChild(MissionParent, HealingBuilding);
					m_instBuildings[i].GetComponent<ExploredBuilding>().Index = i;
					m_instBuildings[i].GetComponent<UISprite>().depth = 15;
					m_instBuildings[i].transform.localPosition = m_buildings[i].LocalPosition;
					break;
				case 2: // empty building
					m_instBuildings[i] = NGUITools.AddChild(MissionParent, EmptyBuilding);
					m_instBuildings[i].GetComponent<ExploredBuilding>().Index = i;
					m_instBuildings[i].GetComponent<UISprite>().depth = 15;
					m_instBuildings[i].transform.localPosition = m_buildings[i].LocalPosition;
					if(m_buildings[i].Visited) {
						m_instBuildings[i].GetComponent<UISprite>().color = Color.gray;
						m_instBuildings[i].GetComponent<ExploredBuilding>().Disabled = true;
					}
					break;
			}
		}
	}
#endregion

#region Building Check Methods
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
			return (int)ButtonData.BuildingType.Shop;
		case "dentist":
		case "doctor":
		case "health":
		case "hospital":
		case "pharmacy":
		case "physiotherapist":
			return (int)ButtonData.BuildingType.Healing;
		default:
			return (int)ButtonData.BuildingType.Empty;
		}
	} 
#endregion

	public bool SetMissionParent() {
		MissionParent = GameObject.Find("Panel - Main Panel");
		if(MissionParent == null) { return false;}//returns false if mission parent fails to be set.
		return true;//else return true that mission parent is set
	}
	
	public void SetPlayerIcon() {
		PlayerSprite = GameObject.Find("Sprite - Player Icon");
	}
	
	public void DestroyThis() {
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
		Debug.Log("currentGPSPosition is (" + currentGPSPosition.latitude + " , " + currentGPSPosition.longitude + ")");
	}
	
	public void RefreshPlaces() {
		RetrieveGPSData();
		string radarURL;
#if UNITY_ANDROID && !UNITY_EDITOR
		radarURL = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + currentGPSPosition.latitude + "," + currentGPSPosition.longitude + "&radius=" + radarRadius + "&sensor=true&key=" + APIkey;
#else
		radarURL = string.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=" + playerLoc.y + "," + playerLoc.x + "&radius={0}&sensor=false&key={1}", radarRadius, APIkey);
#endif
		using (WWW googleResp = new WWW(radarURL))
		{
			while(!googleResp.isDone) {}
			
			googleRespStr = googleResp.text;
//			Debug.Log(googleRespStr);
			pData = JSON.Parse(googleRespStr);
		}
		
		int count = 0; float lng_sum = 0.0f, lat_sum = 0.0f;
		
		for(int i = 0; i < pData.Count; ++i) {
			lng_sum += pData["results"][i]["geometry"]["location"]["lng"].AsFloat;
			lat_sum += pData["results"][i]["geometry"]["location"]["lat"].AsFloat;
			count++;
		}
		
		center_avg_x = lng_sum / (float)count;
		center_avg_y = lat_sum / (float)count;
	}
	
	int SetDeltaX(Vector3 pos, int zoom) {
//		int xLoc = MapUtils.LonToX(playerLoc.x);
//		int xLoc = MapUtils.LonToX(center_avg_x);
//		int xLoc = 0;
//		return ((int)pos.x - xLoc) >> (21 - zoom);
		return (int)pos.x >> (21 - zoom);
	}
	
	int SetDeltaY(Vector3 pos, int zoom) {
		int yLoc = MapUtils.LatToY(playerLoc.y);
		return ((int)pos.y - yLoc) >> (21 - zoom);
	}
	
	Vector3 SetDeltaPosition(Vector3 dPos) {
		Vector3 pos = new Vector3();
		if(dPos.x < playerLoc.x) {
//			Debug.Log(MapUtils.LonToX(center_avg_x) + " - " + MapUtils.LonToX(dPos.x) + " = " + (MapUtils.LonToX(center_avg_x) - MapUtils.LonToX(dPos.x)));
			if(dPos.x < 0 && center_avg_x > 0) {
//				Debug.Log(center_avg_x + " + " + dPos.x + " = " + (center_avg_x + dPos.x));
//				Debug.Log("X is " + MapUtils.LonToX(center_avg_x + dPos.x));
//				pos.x = MapUtils.XToLon(MapUtils.LonToX(center_avg_x) + MapUtils.LonToX(dPos.x));
//				pos.x = MapUtils.LonToX(center_avg_x + dPos.x);
				pos.x = MapUtils.LonToX(center_avg_x + dPos.x);
			} else {
//				Debug.Log(center_avg_x + " - " + dPos.x + " = " + (center_avg_x - dPos.x));
//				Debug.Log("X is " + MapUtils.LonToX(center_avg_x - dPos.x));
//				pos.x = MapUtils.XToLon(MapUtils.LonToX(center_avg_x) - MapUtils.LonToX(dPos.x));
//				pos.x = MapUtils.LonToX(center_avg_x + dPos.x);
				pos.x = MapUtils.LonToX(center_avg_x + dPos.x);
			}
		} else {
			if(dPos.x > 0 && playerLoc.x < 0) {
//				Debug.Log(dPos.x + " + " + playerLoc.x + " = " + (dPos.x + playerLoc.x));
//				pos.x = MapUtils.XToLon(MapUtils.LonToX(dPos.x) + MapUtils.LonToX(playerLoc.x));
//				pos.x = MapUtils.LonToX(dPos.x + playerLoc.x);
				pos.x = MapUtils.LonToX(dPos.x + center_avg_x);
			} else {
//				Debug.Log(dPos.x + " - " + playerLoc.x + " = " + (dPos.x - playerLoc.x));
//				pos.x = MapUtils.XToLon(MapUtils.LonToX(dPos.x) - MapUtils.LonToX(playerLoc.x));
//				pos.x = MapUtils.LonToX(dPos.x - playerLoc.x);
				pos.x = MapUtils.LonToX(dPos.x - center_avg_x);
			}
		}
		
		if(dPos.y < center_avg_y) {
			if(dPos.y < 0  && center_avg_y > 0) {
//				pos.y = MapUtils.YToLat(MapUtils.LatToY(center_avg_y) + MapUtils.LatToY(dPos.y));
				pos.y = MapUtils.YToLat(MapUtils.LatToY(center_avg_y) + MapUtils.LatToY(dPos.y));
			} else {
//				pos.y = MapUtils.YToLat(MapUtils.LatToY(center_avg_y) - MapUtils.LatToY(dPos.y));
				pos.y = MapUtils.YToLat(MapUtils.LatToY(center_avg_y) - MapUtils.LatToY(dPos.y));
			}
		} else {
			if(dPos.y > 0 && center_avg_y < 0) {
//				pos.y = MapUtils.YToLat(MapUtils.LatToY(dPos.y) + MapUtils.LatToY(center_avg_y));
				pos.y = MapUtils.YToLat(MapUtils.LatToY(dPos.y) + MapUtils.LatToY(center_avg_y));
			} else {
//				pos.y = MapUtils.YToLat(MapUtils.LatToY(dPos.y) - MapUtils.LatToY(center_avg_y));
				pos.y = MapUtils.YToLat(MapUtils.LatToY(dPos.y) - MapUtils.LatToY(center_avg_y));
			}
		}
		
		return pos;
	}
	
	Vector3 SetDeltaPositionAndroid(Vector3 dPos) {
		Vector3 pos = new Vector3();
		if(dPos.x < center_avg_x) {
			if(dPos.x < 0 && center_avg_x > 0) {
//				pos.x = MapUtils.XToLon(MapUtils.LonToX(center_avg_x) + MapUtils.LonToX(dPos.x));
				pos.x = MapUtils.LonToX(center_avg_x + dPos.x);
			} else {
//				pos.x = MapUtils.XToLon(MapUtils.LonToX(center_avg_x) - MapUtils.LonToX(dPos.x));
				pos.x = MapUtils.LonToX(center_avg_x + dPos.x);
			}
		} else {
			if(dPos.x > 0 && center_avg_x < 0) {
//				pos.x = MapUtils.XToLon(MapUtils.LonToX(dPos.x) + MapUtils.LonToX(playerLoc.x));
//				pos.x = MapUtils.LonToX(dPos.x + playerLoc.x);
				pos.x = MapUtils.LonToX(dPos.x + center_avg_x);
			} else {
//				pos.x = MapUtils.XToLon(MapUtils.LonToX(dPos.x) - MapUtils.LonToX(playerLoc.x));
//				pos.x = MapUtils.LonToX(dPos.x - playerLoc.x);
				pos.x = MapUtils.LonToX(dPos.x - center_avg_x);
			}
		}
		
		if(dPos.y < center_avg_y) {
			if(dPos.y < 0  && center_avg_y > 0) {
				pos.y = MapUtils.YToLat(MapUtils.LatToY(center_avg_y) + MapUtils.LatToY(dPos.y));
			} else {
				pos.y = MapUtils.YToLat(MapUtils.LatToY(center_avg_y) - MapUtils.LatToY(dPos.y));
			}
		} else {
			if(dPos.y > 0 && center_avg_y < 0) {
				pos.y = MapUtils.YToLat(MapUtils.LatToY(dPos.y) + MapUtils.LatToY(center_avg_y));
			} else {
//				pos.y = MapUtils.YToLat(MapUtils.LatToY(dPos.y) - MapUtils.LatToY(center_avg_y));
				pos.y = MapUtils.YToLat(MapUtils.LatToY(dPos.y) - MapUtils.LatToY(center_avg_y));
			}
		}
		
		return pos;
	}
#endregion
}