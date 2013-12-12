using UnityEngine;
using System.Collections;

public class NumberExample : MonoBehaviour {

	public GUIText number_output_text;
	AndroidJavaClass argzTestActivityJavaClass;
	// Use this for initialization
	void Start () {
		AndroidJNI.AttachCurrentThread ();
		argzTestActivityJavaClass = new AndroidJavaClass ("com.tandosbs.ARGZtest");
	}
	
	// Update is called once per frame
	void Update () {
		int number = argzTestActivityJavaClass.CallStatic<int>("getNumber");
		number_output_text.text = "nr: " + number;
	}
}