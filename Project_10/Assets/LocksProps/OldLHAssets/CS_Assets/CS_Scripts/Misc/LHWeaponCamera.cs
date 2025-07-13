//This script simply hides the weapon camera when we are interacting with a lock (LHToolkit).
using UnityEngine;
using System.Collections;

public class LHWeaponCamera : MonoBehaviour 
{
	public string playerTag = "Player"; //The tag of the player
	private GameObject playerObject; // The player object that will be deactivated when activating on a container

	// Use this for initialization
	void Start() 
	{
		playerObject = GameObject.FindGameObjectWithTag(playerTag);
	}
	
	// Update is called once per frame
	void Update() 
	{
		if ( GetComponent<Camera>().enabled == false && playerObject.gameObject.activeSelf == true )
		{
			GetComponent<Camera>().enabled = true;
		}
		else if ( GetComponent<Camera>().enabled == true && playerObject.gameObject.activeSelf == false )
		{
			GetComponent<Camera>().enabled = false;
		}
	}
}
