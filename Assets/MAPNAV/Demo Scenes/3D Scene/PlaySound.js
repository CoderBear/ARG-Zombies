#pragma strict

function OnTriggerEnter (other:Collider) {
	if(other.tag=="Player"){
		audio.Play();
	}
}
function OnTriggerExit (other:Collider) {
	if(other.tag=="Player"){
		audio.Stop();
	}
}
