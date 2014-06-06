using UnityEngine;
using System.Collections;

public class Login : MonoBehaviour {
	private string loginURL = "http://192.185.41.34/~codebear/argz/login.php";
	private const string verifyDB = "&dbuser=codebear_coder&dbpass=J29kMMX&dbtable=codebear_argz";

	public UILabel username, problem;
	public UIInput password;
	private string userName = "", passWord ="";
	
	void OnClick ()
	{
		userName = username.text;
		passWord = password.value;
		StartCoroutine (handleLogin (userName, passWord));
	}
	
	IEnumerator handleLogin (string user, string pass)
	{
		problem.text = "Checking username and password..";
		string login_URL = loginURL + "?username=" + user + "&password=" + pass + verifyDB;
		Debug.Log (login_URL);
		WWW loginReader = new WWW (login_URL);
		yield return loginReader;
		
		if (loginReader.error != null) {
			problem.text = "Could not locate page";
		} else {
			if (loginReader.text == "right") {
				problem.text = "logged in";
				Application.LoadLevel ("mainMenu");
			} else {
				problem.text = "invalid user/pass";
			}
		}
	}
}