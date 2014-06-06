#pragma strict

function OnTriggerEnter(other:Collider){
	if(other.tag=="Player"){
		transform.Find("Points").guiText.text="IN";
	}
}
function OnTriggerExit(other:Collider){
	if(other.tag=="Player"){
		transform.Find("Points").guiText.text="OUT";
	}
}