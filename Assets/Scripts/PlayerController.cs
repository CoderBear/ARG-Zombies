using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	[SerializeField] private float speed = 5f;

	[SerializeField] tk2dSprite sprite;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey (KeyCode.DownArrow)) {
			float y = Input.GetAxis ("Vertical") * Time.deltaTime * speed;
			sprite.transform.Translate (0f, y, 0f);
		}
		else if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown (KeyCode.RightArrow)) {
			float x = Input.GetAxis ("Horizontal") * Time.deltaTime * speed;
			sprite.transform.Translate (x, 0, 0);
		}
	}
}