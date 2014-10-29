#define ANDROID
using UnityEngine;
using System.Collections;
using System.Timers;
using SimpleJSON;

public class GooglePlaces : MonoBehaviour {
	static int Neversleep;
	LocationInfo currentGPSPosition;
//	string gpsString;
	public GUIStyle LocStyle;
	int wait;
	float radarRadius;
	string radarType, APIkey, radarSensor;
	string googleRespStr;
	public string ResponseString // Parsed Places Response JSON usable
	{
		get;
		private set;
	}
//	Rect GUIBoxGPSRect1 = new Rect(25, 25, 700, 100);
//	Rect GUIBoxGPSRect2  = new Rect(55,  25,  700,  100);
//	Rect GUIBoxRespRect1 = new Rect(25, 130, 700, 800);
//	Rect GUIBoxRespRect2  = new Rect(35,  135,  700,  800);

	// Use this for initialization
	void Start () {
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		radarRadius = 100f;
		radarType = "restaurant";
		APIkey = "AIzaSyC0hSk_GN1skCDphwYspPdKs9e5GQ4-fbs";
		radarSensor = "false";
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	void RetrieveGPSData()
	{
		currentGPSPosition = Input.location.lastData;
//		gpsString = "Lat: " + currentGPSPosition.latitude + "  Lon: " + currentGPSPosition.longitude + "  Alt: " + currentGPSPosition.altitude + 
//			"  HorAcc: " + Input.location.lastData.horizontalAccuracy + "  VerAcc: " + Input.location.lastData.verticalAccuracy + "  TS: " + Input.location.lastData.timestamp;
	}
	
	public void RefreshPlaces() {
		RetrieveGPSData();
		StartCoroutine(Radar());
	}
	
	void OnGUI ()
	{
//		GUI.Box (GUIBoxGPSRect1, "");
//		GUI.Label (GUIBoxGPSRect2, gpsString, LocStyle);
//		
//		GUI.Box (GUIBoxRespRect1, "");
//		GUI.Label (GUIBoxRespRect2, "" +googleRespStr, LocStyle);
//		
//		#if PC
//		Debug.Log("On PC / Don't have GPS");
//		#elif !PC
//		Input.location.Start(10f,1f);
//		int wait = 1000;
//		
//		if(Input.location.isEnabledByUser)
//		{
//			while(Input.location.status == LocationServiceStatus.Initializing && wait>0)
//			{
//				wait--;
//			}
//			if (Input.location.status == LocationServiceStatus.Failed)
//			{}
//			
//			else
//			{
//				RetrieveGPSData();
//				StartCoroutine(Radar());
//			}
//		}
//		else
//		{
//			GameObject.Find("gps_debug_text").guiText.text = "GPS not available";
//		}
//		#endif
	}
	
	IEnumerator Radar ()
	{
//		string radarURL = "https://maps.googleapis.com/maps/api/place/radarsearch/json?location=" + currentGPSPosition.latitude + "," + currentGPSPosition.longitude + "&radius=" + radarRadius + "&types=" + radarType + "&sensor=false" + radarSensor + "&key=" + APIkey;
		string radarURL = "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=37.68774,-121.8961&radius=" + radarRadius + "&types=" + radarType + "&sensor=false" + radarSensor + "&key=" + APIkey;
		WWW googleResp = new WWW(radarURL);
		yield return googleResp;
		googleRespStr = googleResp.text;
		ResponseString = googleRespStr;
		print (googleRespStr);       
	}
}