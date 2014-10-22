using UnityEngine;
using System.Collections;

public class GoogleMap : MonoBehaviour
{
	public enum MapType
	{
		RoadMap,
		Satellite,
		Terrain,
		Hybrid
	}
	public bool loadOnStart = true;
	public bool autoLocateCenter = true;
	public GoogleMapLocation centerLocation;
	public int zoom = 13;
	public MapType mapType;
	public int size = 512;
	public bool doubleResolution = false;
	public GoogleMapMarker[] markers;
	public GoogleMapPath[] paths;
	[SerializeField] string API_KEY;
	public UITexture mapTex;
	
	void Start() {
		mapTex.gameObject.SetActive(true);
//		mapTex.mainTexture = new Texture(size,size);
		if(loadOnStart) Refresh();	
	}
	
	public void Refresh() {
		if(autoLocateCenter && (markers.Length == 0 && paths.Length == 0)) {
			Debug.LogError("Auto Center will only work if paths or markers are used.");	
		}
		StartCoroutine(_Refresh());
	}
	
	IEnumerator _Refresh ()
	{
		var url = "https://maps.googleapis.com/maps/api/staticmap";
		var qs = "";
		if (!autoLocateCenter) {
			if (centerLocation.address != "")
				qs += "center=" + centerLocation.address;
			else {
				qs += "center=" + string.Format ("{0},{1}", centerLocation.latitude, centerLocation.longitude);
			}
		
			qs += "&zoom=" + zoom.ToString ();
		}
		qs += "&size=" + string.Format ("{0}x{0}", size);
		qs += "&scale=" + (doubleResolution ? "2" : "1");
		qs += "&maptype=" + mapType.ToString ().ToLower ();
		var usingSensor = false;
#if UNITY_IPHONE
		usingSensor = Input.location.isEnabledByUser && Input.location.status == LocationServiceStatus.Running;
#endif
		qs += "&sensor=" + (usingSensor ? "true" : "false");
		
		foreach (var i in markers) {
			qs += "&markers=" + string.Format ("size:{0}|color:{1}|label:{2}", i.size.ToString ().ToLower (), i.color, i.label);
			foreach (var loc in i.locations) {
				if (loc.address != "")
					qs += "|" + loc.address;
				else
					qs += "|" + string.Format ("{0},{1}", loc.latitude, loc.longitude);
			}
		}
		
		foreach (var i in paths) {
			qs += "&path=" + string.Format ("weight:{0}|color:{1}", i.weight, i.color);
			if(i.fill) qs += "|fillcolor:" + i.fillColor;
			foreach (var loc in i.locations) {
				if (loc.address != "")
					qs += "|" + loc.address;
				else
					qs += "|" + string.Format ("{0},{1}", loc.latitude, loc.longitude);
			}
		}
		
		WWW mapReader = new WWW(url + "?" + qs + "&key=" + API_KEY);
		Debug.Log(url + "?" + qs + "&key=" + API_KEY);
//		while(!mapReader.isDone)
//			yield return null;
		yield return mapReader;
		if(mapReader.error == null) {
			Debug.Log("Building Map Image");
			var tex = new Texture2D (size, size);
//			tex.LoadImage(mapReader.bytes);
			mapReader.LoadImageIntoTexture(tex);
			Debug.Log("Rendering Map Image");
			mapTex.mainTexture = tex;
			mapTex.MarkAsChanged();
//			mapTex.renderer.material.mainTexture = tex;
		}
	}
}

public enum GoogleMapColor
{
	black,
	brown,
	green,
	purple,
	yellow,
	blue,
	gray,
	orange,
	red,
	white
}

[System.Serializable]
public class GoogleMapLocation
{
	public string address;
	public float latitude;
	public float longitude;
}

[System.Serializable]
public class GoogleMapMarker
{
	public enum GoogleMapMarkerSize
	{
		Tiny,
		Small,
		Mid
	}
	public GoogleMapMarkerSize size;
	public GoogleMapColor color;
	public string label;
	public GoogleMapLocation[] locations;
}

[System.Serializable]
public class GoogleMapPath
{
	public int weight = 5;
	public GoogleMapColor color;
	public bool fill = false;
	public GoogleMapColor fillColor;
	public GoogleMapLocation[] locations;	
}