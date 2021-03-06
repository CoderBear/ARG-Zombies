﻿using UnityEngine;
using System.Collections;

public class Register : MonoBehaviour {
	
	string registerURL = "http://192.185.41.34/~codebear/tp_argz/register.php";
	public UILabel username, password, firstname, lastname, email, problem;
	private string userName = "", passWord = "", firstName="", lastName="", eMail="";
	private const string verifyDB = "&dbuser=codebear_coder&dbpass=J29kMMX&dbtable=codebear_argz";
	
	void OnClick ()
	{
		userName = username.text;
		passWord = password.text;
		firstName = firstname.text;
		lastName = lastname.text;
		eMail = email.text;

		StartCoroutine (handleRegister (userName, passWord, firstName, lastName, eMail));
		
	}
	
	IEnumerator handleRegister (string username, string password, string firstname, string lastname, string email)
	{
		string register_URL = registerURL + "?username=" + username + "&password=" + password + "&firstname=" + firstname + "&lastname=" + lastname + "&email=" + email + verifyDB;
		WWW registerReader = new WWW (register_URL);
		yield return registerReader;
		
		if (registerReader.error != null) {
			problem.text = "Could not locate page";
		} else {
			if (registerReader.text == "registered") {
				problem.text = "Registered";
				Application.LoadLevel ("mainMenu");
			} else {
				problem.text = "Did not register";
			}
		}
	}
}