//MAPNAV Navigation ToolKit v.1.3.2
//Attention: This script uses a custom editor inspector: MAPNAV/Editor/SetGeoInspector.cs

using UnityEngine;
using System.Collections;

[AddComponentMenu("MAPNAV/GetGeolocation")]

public class GetGeolocation : MonoBehaviour
{
    public float lat;
    public float lon;
    public float height;
    public float orientation;
    public float scaleX;
    public float scaleY;
    public float scaleZ;
    private float posX;
    private float posY;
    private float posZ;
    private float initX;
    private float initZ;
    private MapNav gps;
    private bool gpsFix;

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

    }

    void Update()
    {
        if (gpsFix)
        {
            orientation = transform.eulerAngles.y;
            posX = transform.position.x;
            posZ = transform.position.z;
            height = transform.position.y * 100; //1:100 scale (1 Unity world unit = 100 real world meters)
            scaleX = transform.localScale.x;
            scaleY = transform.localScale.y;
            scaleZ = transform.localScale.z;
            lat = ((360 / Mathf.PI) * Mathf.Atan(Mathf.Exp(0.00001567855943f * (posZ + initZ)))) - 90;
            lon = (18000 * (posX + initX)) / 20037508.34f;
        }
        else
        {
            lat = 0;
            lon = 0;
        }
    }
}