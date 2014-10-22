//MAPNAV Navigation ToolKit v.1.3.2
//Attention: This script uses a custom editor inspector: MAPNAV/Editor/SetGeoInspector.cs

using UnityEngine;
using System.Collections;

[AddComponentMenu("MAPNAV/SetGeolocation")]

public class SetGeolocation : MonoBehaviour
{
    public float lat;
    public float lon;
    public float height;
    public float orientation;
    public float scaleX;
    public float scaleY;
    public float scaleZ;
    private float initX;
    private float initZ;
    private MapNav gps;
    private bool gpsFix;
    private float fixLat;
    private float fixLon;

    void Awake()
    {
        //Reference to the MapNav.js script and gpsFix variable. gpsFix will be true when a valid location data has been set.
        gps = GameObject.FindGameObjectWithTag("GameController").GetComponent<MapNav>();
        gpsFix = gps.gpsFix;
    }

    IEnumerator Start()
    {
        //Wait until the gps sensor provides a valid location.
        while (!gpsFix)
        {
            gpsFix = gps.gpsFix;
            yield return null;
        }
        //Read initial position (used as a reference system)
        initX = gps.iniRef.x;
        initZ = gps.iniRef.z;
        //Set object geo-location
        GeoLocation();
    }

    [ContextMenu("GeoLocation")]

    void GeoLocation()
    {
        //Translate the geographical coordinate system used by gps mobile devices(WGS84), into Unity's Vector2 Cartesian coordinates(x,z) and set height(1:100 scale).
        transform.position = new Vector3(((lon * 20037508.34f) / 18000) - initX, height / 100, ((Mathf.Log(Mathf.Tan((90 + lat) * Mathf.PI / 360)) / (Mathf.PI / 180)) * 1113.19490777778f) - initZ);

        //Set object orientation
        Vector3 tmp = transform.eulerAngles;
        tmp.y = orientation;
        transform.eulerAngles = tmp;

        //Set local object scale
        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }

    //This function is similar to GeoLocation() but is to be used by SetGeoInspector.cs
    public void EditorGeoLocation()
    {
        gps = GameObject.FindGameObjectWithTag("GameController").GetComponent<MapNav>();
        fixLat = gps.fixLat;
        fixLon = gps.fixLon;

        initX = fixLon * 20037508.34f / 18000;
        initZ = (float) (System.Math.Log(System.Math.Tan((90 + fixLat) * System.Math.PI / 360)) / (System.Math.PI / 180));
        initZ = initZ * 20037508.34f / 18000;

        //Translate the geographical coordinate system used by gps mobile devices(WGS84), into Unity's Vector2 Cartesian coordinates(x,z) and set height(1:100 scale).
        transform.position = new Vector3(((lon * 20037508.34f) / 18000) - initX, height / 100, ((Mathf.Log(Mathf.Tan((90 + lat) * Mathf.PI / 360)) / (Mathf.PI / 180)) * 1113.19490777778f) - initZ);
       
        //Set object orientation
        Vector3 tmp = transform.eulerAngles;
        tmp.y = orientation;
        transform.eulerAngles = tmp;
       
        //Set local object scale
        transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
    }
}