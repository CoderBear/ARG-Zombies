using UnityEngine;
using System.Collections;

public class VerifyUserName : MonoBehaviour {
	private string verifyURL = "http://192.185.41.34/~codebear/nameverify.php";
	private const string verifyDB = "&dbuser=codebear_coder&dbpass=J29kMMX&dbtable=codebear_argz";
	public UILabel username, verifyStatus, problem, debug;
	
	public UIImageButton RegisterButton;
	public UIToggle verifyToggle;
	
	private string userName = "";
	
	void OnClick ()
	{
		userName = username.text;
		StartCoroutine (handleVerify (userName));
	}
	
	IEnumerator handleVerify (string userName)
	{
		problem.text = "Checking if username is available";
		string verify_URL = verifyURL + "?username=" + userName + verifyDB;

		WWW verifyReader = new WWW (verify_URL);
		yield return verifyReader;
		
		if (verifyReader.error != null) {
			problem.text = "Could not locate page";
		} else {
			problem.text = "Displaying Results";
#if UNITY_EDITOR
			debug.text = verifyReader.text;
#endif
			if (verifyReader.text == "available") {
				verifyToggle.value = true;
				verifyStatus.text = "Available";
				RegisterButton.gameObject.SetActive(true);
			} else {
				verifyStatus.text = "Unavailable";
			}
		}
	}
}