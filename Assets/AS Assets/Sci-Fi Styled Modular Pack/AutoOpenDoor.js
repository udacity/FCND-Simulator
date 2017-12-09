#pragma strict

private var anim : Animator;
var character : GameObject;
var distanceToOpen:float = 5;
private var characterNearbyHash : int = Animator.StringToHash("character_nearby");

function Start () 
{
    anim = GetComponent("Animator");
    if ( !character )
    	enabled = false;
}


function Update () 
{
	var distance = Vector3.Distance(transform.position,character.transform.position);
	
	if (distanceToOpen>=distance){
    	anim.SetBool(characterNearbyHash, true);
    }else{
    	anim.SetBool(characterNearbyHash, false);
    }
}