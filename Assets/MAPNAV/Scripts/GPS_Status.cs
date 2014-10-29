//MAPNAV Navigation ToolKit v.1.3.2
//This script is for illustrative purposes only. Feel free to modify, extend or customize it to fit your own needs.

using UnityEngine;
using System.Collections;
[AddComponentMenu("MAPNAV/GPS_Status")]

public class GPS_Status : MonoBehaviour
{
    public float refreshRate = 0.2f;
    public GUIStyle style;
	public GUIStyle style2;
    private MapNav gps;
    private string ddLat;
    private string ddLon;
    private string dmsLat;
    private string dmsLon;
    private float heading;
    private float error;
    private string status;
    private int screenX;
    private int screenY;
    private int zoom;
    private float altitude;
    private bool info;

    void Awake()
    {
        //Reference to MapNav.cs script. Make sure that the map object containing the MapNav.cs script is tagged as "GameController"
        gps = GameObject.FindGameObjectWithTag("GameController").GetComponent<MapNav>();
        screenX = Screen.width;
        screenY = Screen.height;
    }

    void Start()
    {
        //Get gps Status Data every "refreshRate" seconds
        InvokeRepeating("GetData", 1.0f, refreshRate);
    }

    void GetData()
    {
        //Current latitude (decimal)
        ddLat = gps.userLat.ToString();
        //Current longitude (decimal)
        ddLon = gps.userLon.ToString();
        //Current latitude (degrees, minutes, seconds)
        dmsLat = gps.dmsLat;
        //Current longitude (degrees, minutes, seconds)
        dmsLon = gps.dmsLon;
        //Current heading/orientation
        heading = gps.heading;
        //Current GPS sensor accuracy
        error = gps.accuracy;
        //Current Zoom Level
        zoom = gps.zoom;
        //Current altitude(meters)
        altitude = gps.altitude;
    }

    void Update()
    {
        //Reference to MapNav.cs "status" variable  
        status = gps.status;
        //Reference to MapNav.cs "info" variable. Used to activate/de-activate the GUI elements.
        info = gps.info;
    }

    void OnGUI()
    {
        if (info)
        {
            //These GUI Styles can be modified using the inspector
            style.fontSize = (int) Mathf.Round((screenX + screenY) * 0.015f);
            style2.fontSize = (int) Mathf.Round((screenX + screenY) * 0.015f);

            //Display current gps Status data
            GUI.BeginGroup(new Rect(0, 0, screenX, screenY/4));
			GUI.Box(new Rect(0, 0, screenX, screenY/4), "");
			GUI.Label(new Rect(screenX/40, screenY/50, screenX-screenX/20, screenY/50), "Latitude: " + dmsLat, style);
			GUI.Label(new Rect(screenX/40, 3*screenY/50, screenX-screenX/20, screenY/50), "Longitude: " + dmsLon, style);
			GUI.Label(new Rect(screenX/40, 5*screenY/50, screenX-screenX/20, screenY/50), "Altitude(m): " + altitude, style);
			GUI.Label(new Rect(screenX/40, screenY/50, screenX-screenX/20, screenY/50), "Heading: " + Mathf.Round(heading), style2);
			GUI.Label(new Rect(screenX/40, 3*screenY/50, screenX-screenX/20, screenY/50), "Zoom Level: " + zoom, style2);
			GUI.Label(new Rect(screenX/40, 5*screenY/50, screenX-screenX/20, screenY/50), "Error(m): " + error, style2);
			GUI.Label(new Rect(screenX/40, 7*screenY/50, screenX-screenX/20, screenY/25), "Status: " + status, style);
            GUI.EndGroup();
        }
    }
}