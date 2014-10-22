using UnityEngine;
using System.Collections;

public class LocationManager : MonoBehaviour {

	private float lat = 0.0f, lon = 0.0f;
	private int accuracy = 10; //accuracy of the GPS module
	private int distance = 10; // minimum distance before next location variables allocation
	private int maxWait = 20;  //maximum time (s) before time out

//	private int locationStatus;
	private LocationServiceStatus locationStatus;

	string locationURL = "http://192.185.41.34/~codebear/tp_argz/gpsloc.php";
	private const string verifyDB = "&dbuser=codebear_coder&dbpass=J29kMMX&dbtable=codebear_argz";

//	public UILabel problem;

	// Use this for initialization
	void Start () {
		Input.location.Start (); //enable location settings
		Input.compass.enabled = true;
		locationStatus = Input.location.status;

		startLocationService(); //begin GPS transmittion
	}

	// Update is called once per frame
	void Update () {
		if (locationStatus == LocationServiceStatus.Running) {
			lat = Input.location.lastData.latitude;
			lon = Input.location.lastData.latitude;
		}
	}

	void onApplicationQuit() {
		Input.location.Stop ();
	}

	private void startLocationService() {
		Input.location.Start (accuracy, distance);

		while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
			maxWait--;
			locationStatus = Input.location.status;
		}
	}

	public void updateStoredLocation()
	{
		StartCoroutine (handleLocationUpdate (lat,lon));
	}

	public void getFourSquareVenues() {
		int year = System.DateTime.Today.Date.Year;
		int month = System.DateTime.Today.Month;
		int day = System.DateTime.Today.Day;

		string date = getDate (month, day, year);

		string fs_link = "https://api.foursquare.com/v2/venues/search?ll="+lat+","+lon+"4&client_id=WCLUYQFPTWX3B5BOFB1DOKD1P5Y4WDRE44N5FCQASZI3303J&client_secret=3GHHLGY1N42Q4INAC10YTRN2RVVSKRXNUIWL10V2WA2QARRC&v=" + date;
	}

	private string getDate (int m, int d, int y) {
		string mDate, dDate, yDate = "";

		if (m < 10)
			mDate = "0" + m.ToString ();
		else
			mDate = m.ToString ();

		if (d < 10) {
			dDate = "0" + d.ToString();
		} else {
			dDate = d.ToString();
		}

		return yDate.ToString () + mDate + dDate;
	}
	
	public float GetLatitude() { return lat; }
	public float GetLongitude() { return lon; }

	IEnumerator handleLocationUpdate(float latitude, float longitude) {
		string coord = "?latitude=" + latitude + "&longitude=" + longitude;

		string location_URL = locationURL + coord + verifyDB;
		WWW locReader = new WWW (location_URL);
		yield return locReader;

		if (locReader.error != null) {
//			problem.text = "Could not locate page";
		}
	}
}