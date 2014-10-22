//MAPNAV Navigation ToolKit v.1.3.2

using UnityEngine;
using System.Collections;

public class InitScreen : MonoBehaviour
{
    private MapNav mapnav;
    private Transform initText;
    private Transform initBackg;

    void Awake()
    {
        //Reference to the MapNav.cs script and GUI elements
        mapnav = GameObject.FindGameObjectWithTag("GameController").GetComponent<MapNav>();
        initText = transform.Find("GUIText");
        initBackg = transform.Find("GUITexture");

        //Set GUIText font size according to our device screen size
        initText.guiText.fontSize = (int)Mathf.Round(15 * Screen.width / 320);
    }

    void Start()
    {
        //Initialization message
        initText.guiText.text = "Searching for satellites ...";

        //Enable initial screen
        initText.gameObject.SetActive(true);
        initBackg.gameObject.SetActive(true);
		initBackg.guiTexture.pixelInset = new Rect (initBackg.guiTexture.pixelInset.x, initBackg.guiTexture.pixelInset.y, Screen.width, Screen.height);  
    }

    void Update()
    {
        if (!mapnav.ready)
        {
            //Display GPS fix and maps download progress
            initText.guiText.text = mapnav.status;
        }
        else
        {
            //Clear messages once the map is ready
            initText.guiText.text = "";

            //Disable initial screen
            initBackg.gameObject.SetActive(false);

            //Disable this script (no longer needed)
            this.enabled = false; 
        }
    }
}
