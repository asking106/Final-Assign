using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649

namespace LHToolkit
{
	/// <summary>
	/// This script handles a standard lock, which requires the activator to rotate a lockpick while turning the cylinder in order to unlock
	/// it. The lockpick may break, and there is a limited number of lockpicks with the activator.
	/// </summary>
	[ExecuteInEditMode]
	public class LHPanelLock : MonoBehaviour
	{
		// The camera of this lock
		public Camera cameraObject;
		
		// List of the panel plugs, and which plugs they connect to with wires
		public List<Plug> plugs = new List<Plug>();
		
		// The currently selected slot.
		internal Transform currentSlot;
		
		// The currently selected plug.
		internal Transform currentPlug;
		
		// Colors of the correct plu icon
		public Color colorSlotCorrect;
		public Color colorSlotWrong;
		
		// Did we succeed or fail in unlocking this lock?
		private bool success = false;
		private bool failure = false;
		
		// Are we moving a plug now?
		private bool isMoving = false;
		
		// How many times we can move the plugs around. If you lift a plug and then plug it back into the same slot, it doesn't count as a move
		public int movesLeft = 10;
		
		// The name of the tool in the player's inventory which is required to unlock this lock ( lockpicks, bobbyplugs, safe cracker tools, etc )
		public string requiredTool = string.Empty;
		
		// The index of the tool in the player's inventory
		public static int requiredToolIndex;
		
		// Container of this lock ( A door, a safe, etc )
		internal Transform lockParent;
		
		// The activator object
		internal GameObject activator;
		
		// Various sounds
		public AudioClip soundPlugOut;
		public AudioClip soundPlugIn;
		public AudioClip soundUnlock;
		public AudioClip soundFail;
		
		// GUI for button graphics
		public GUISkin GUISkin;
		
		// The position and size of the "abort" button
		public Rect abortRect = new Rect(0, 0, 100, 50);
		public string abortText = "Abort";
		
		// The position and size of the "moves left" display
		public Rect movesLeftRect = new Rect(0, 50, 100, 50);
		public string movesLeftText = "Moves left: ";
		
		// The description for how to play this lock
		public string description = "Click on a plug to pick it up, then click on an empty slot to plug it in.\nPut all the plugs in the correct slots and don't let the wires intersect.";
		public string descriptionMobile = "Tap on a plug to pick it up, then tap on an empty slot to plug it in.\nPut all the plugs in the correct slots and don't let the wires intersect.";
		
		// A check for control types ( default, mobile, etc )
		private string controlsType = string.Empty;

		public bool lockCursorOnExit = false; //Should we lock the mouse pointer on Exit?

		public void Start ()
		{
			//Check if we are running on a mobile and set the controls accordingly
			#if UNITY_IPHONE
			controlsType = "iphone";
			print("iphone");
			#endif
			
			#if UNITY_ANDROID
			controlsType = "android";
			print("android");
			#endif
			
			// Deactivate the activator and container objects so they don't interfere with the lockpicking gameplay
			if( activator )
				activator.SetActive(false);
			
			// Update the positions of the wires and their colliders
			UpdateWires();
			
			// Check if we met all the success goals; all plugs in correct slots, and no wires intersecting
			CheckSuccess();
		}
		
		public void Update ()
		{
			//Show the cursor and allow it to move while we interact with the lock 
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			if( failure == false && success == false )
			{
				// If we are not moving a plug, look for a plug to select
				if( isMoving == false )
				{
					RaycastHit hit;
					Ray ray = cameraObject.ScreenPointToRay(Input.mousePosition);
					
					// If on mobile, check from camera object to touch position
					if( (controlsType == "android" || controlsType == "iphone") && Input.touches.Length > 0 )
						ray = cameraObject.ScreenPointToRay(Input.touches[0].position);
					
					if( Physics.Raycast(ray, out hit, 100) )
					{
						// Allow checks only with a slot
						if( hit.collider.tag == "LHslot" )
						{
							// Set the current slot
							currentSlot = hit.collider.transform;
							
							// If we are on mobile, Check for a touch to move the plug, otherwise check for a click
							if( (controlsType == "android" || controlsType == "iphone") && Input.touches.Length > 0 )
							{
								if( Input.touches[0].phase == TouchPhase.Began )
								{
									if( currentPlug == null && currentSlot.Find("Plug") )
										PlugOut();
									else if( currentPlug && currentSlot.Find("Plug") == null )
										StartCoroutine(PlugIn(currentSlot));
								}
							}
							else if( Input.GetButtonDown("Fire1") )
							{
								if( currentPlug == null && currentSlot.Find("Plug") )
									PlugOut();
								else if( currentPlug && currentSlot.Find("Plug") == null )
									StartCoroutine(PlugIn(currentSlot));
							}
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Updates the positions of the wires and their colliders.
		/// </summary>
		public void UpdateWires ()
		{
			// Go through all the plugs, update the positions of the wires and their colliders to strech correctly between the plugs
			foreach( Plug plug in plugs )
			{
				Transform fromWire = plug.connectsFrom.Find("Wire"); // store a reference to the from wire.
				LineRenderer fromLine = fromWire.GetComponent<LineRenderer>(); // store a reference to the linerender in the from wire.
				BoxCollider fromBoxCollider = fromWire.GetComponent<BoxCollider>(); // store a reference of the box collider in the form wire.

				Transform toWire = plug.connectsTo.Find("Wire"); // store a reference to the to wire.

				// Strech the wire between two plugs
				fromLine.SetPosition(0, fromWire.position);
				fromLine.SetPosition(1, toWire.position);
				
				// Rotate the wire to look at the next plug
				fromWire.LookAt(toWire);
				
				// Strech the collider of the wire between two plugs
				Vector3 centerVector3 = fromBoxCollider.center; // instantiate centerVector var to the center of the box collider.
				centerVector3.z = Vector3.Distance(fromWire.position, toWire.position) * 0.5f; // Adjust the z
				fromBoxCollider.center = centerVector3; // Set center with updated Vector3

				Vector3 sizeVector3 = fromBoxCollider.size;
				sizeVector3.z = Vector3.Distance(fromWire.position, toWire.position) * 0.7f; // Adjust the z
				fromBoxCollider.size = sizeVector3; // Set size vector3 with updated vector3
			}
		}
		
		/// <summary>
		/// Checks if we met all the success goals: all plugs in correct slots, and no wires intersecting.
		/// </summary>
		public void CheckSuccess ()
		{
			// Reset the correct slots counter
			int correctSlots = 0;
			
			// Go through all the plugs
			foreach( Plug plug in plugs )
			{
				// If this plug is in a correct slot, proceed to the next check
				if( plug.connectsFrom.parent.Find("CorrectSlot") )
				{
					// If the wire of this plug is not intersecting with other wires, increase the correctSlots count
					if( plug.connectsFrom.Find("Wire").GetComponent<LHPanelWire>().isIntersecting == false )
						correctSlots++;
					
					// Change the color of the slot
					plug.connectsFrom.parent.Find("CorrectSlot").GetComponent<Renderer>().material.color = colorSlotCorrect;
				}
			}
			
			// If the number of correct slots is the same as the number of all the slots, then SUCCESS
			if( correctSlots == plugs.Count )
			{
				success = true;
				
				// Unlock this lock
				StartCoroutine(Unlock());
			}
			else if( movesLeft <= 0 )
			{
				// If no moves are left, we fail
				StartCoroutine(Fail());
			}
		}
		
		/// <summary>
		/// Moves the plug out of the slot.
		/// </summary>
		public void PlugOut ()
		{
			GetComponent<AudioSource>().PlayOneShot(soundPlugOut);
			
			// Set current plug
			currentPlug = currentSlot.Find("Plug");

			// Reset the color of the correct slot to it's wrong state ( not plugged in )
			if( currentPlug.parent.Find("CorrectSlot") )
				currentPlug.parent.Find("CorrectSlot").GetComponent<Renderer>().material.color = colorSlotWrong;
			
			// Move the plug out of the slot																
			StartCoroutine(MoveTo(currentPlug, new Vector3(currentPlug.position.x, currentPlug.position.y, currentPlug.position.z + 0.2f), 0.2f));
			
			// Remove the plug from the hierarchy so we can place it in another slot
			currentPlug.parent = null;
		}
		
		/// <summary>
		/// Moves the plug to the selected slot and then plugs it in.
		/// </summary>
		/// <param name="plugSlot">Plug slot.</param>
		public IEnumerator PlugIn (Transform plugSlot)
		{
			// If we selected a slot which is not the same as the slot of the selected plug, then we move the plug to the new slot
			if( currentPlug.position.x != plugSlot.position.x || currentPlug.position.y != plugSlot.position.y )
			{
				// Reduce from the moves left. If this reaches 0, we fail
				movesLeft--;
				
				// Move the plug to the new slot position
				StartCoroutine(MoveTo(currentPlug, new Vector3(plugSlot.position.x, plugSlot.position.y, currentPlug.position.z), 0.2f));
				
				yield return new WaitForSeconds(0.3f);
			}
			
			// Move the plug into the slot
			StartCoroutine(MoveTo(currentPlug, plugSlot.position, 0.2f));
			
			// Place the plug in the hierarchy of this slot
			currentPlug.parent = plugSlot;
			
			// Clear the current plug
			currentPlug = null;
			
			GetComponent<AudioSource>().PlayOneShot(soundPlugIn);
			
			yield return new WaitForSeconds(0.2f);
			
			// Check if we meet the goals
			CheckSuccess();
		}
		
		/// <summary>
		/// Moves an object to another point with a delay.
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="movedObject">Moved object.</param>
		/// <param name="targetPosition">Target position.</param>
		/// <param name="moveTime">Move time.</param>
		public IEnumerator MoveTo (Transform movedObject, Vector3 targetPosition, float moveTime)
		{
			// We are now moving
			isMoving = true;
			
			// Used to help us calculate the speed of movement
			float timeLeft = moveTime;
			
			while( timeLeft > 0 )
			{
				timeLeft -= Time.deltaTime;
				
				yield return new WaitForSeconds(Time.deltaTime);
				
				// Move the object in increments from 0 to moveTime (0 is where the object started, 0.5 is halfway through, and 1 is the end of the path)
				movedObject.position = Vector3.Lerp(movedObject.position, targetPosition, (moveTime - timeLeft) / moveTime);
				
				// Update the wires between plugs
				UpdateWires();
			}
			
			// Place the object at the target position
			movedObject.position = targetPosition;
			
			// Update the wires between plugs
			UpdateWires();
			
			// We stopped moving
			isMoving = false;
		}
		
		/// <summary>
		/// Unlocks and opens a container.
		/// After that the container will have an unlocked lock.
		/// </summary>
		public IEnumerator Unlock ()
		{
			// Set and play relevant sounds
			// audio.Stop();
			if( soundUnlock )
				GetComponent<AudioSource>().PlayOneShot(soundUnlock);
			
			// Wait for a second
			yield return new WaitForSeconds(1.5f);
			
			// Set the container to unlocked and activate it
			if( lockParent )
			{
				LHContainer container = lockParent.GetComponent<LHContainer>();
				lockParent.gameObject.SetActive(true);
				container.locked = false;
				container.Activate();
			}
			
			//Exit the lock
			Exit();
		}
		
		//This function runs when we run out of moves
		/// <summary>
		/// Runs when we run out of moves
		/// </summary>
		public IEnumerator Fail ()
		{
			failure = true;
			
			GetComponent<AudioSource>().PlayOneShot(soundFail);
			
			// Wait for a second
			yield return new WaitForSeconds(1);
			
			// Activate the fail functions on the container
			if( lockParent )
			{
				lockParent.gameObject.SetActive(true);
				lockParent.GetComponent<LHContainer>().FailActivate();
			}
			
			// Exit the lock
			Exit();
		}
		
		/// <summary>
		/// Aborts the lockpicking gameplay and reactivates the activator.
		/// </summary>
		public void Exit ()
		{
			// Activate the activator prefab, meaning that we are done with lockpicking
			activator.SetActive(true);
			
			// Enable the container script
			//lockParent.GetComponent<LHContainer>().enabled = true;

			//Activate the container object
			lockParent.gameObject.SetActive(true);

			//Lock the mouse pointer
			if ( lockCursorOnExit )    Cursor.lockState = CursorLockMode.Locked;
			
			// Destroy this lock
			Destroy(gameObject);
		}
		
		public void OnDrawGizmos ()
		{
			//Draw lines between the plugs so we see where the wires are when editing a lock
			foreach( Plug plug in plugs )
			{
				if ( plug.connectsFrom && plug.connectsTo )
					Gizmos.DrawLine(plug.connectsFrom.Find("Wire").position, plug.connectsTo.Find("Wire").position);
			}
		}
		
		public void OnGUI ()
		{
			GUI.skin = GUISkin;
			
			//Abort button
			if( GUI.Button(abortRect, abortText) )
			{
				Exit();
			}
			
			// Display lockpick health
			GUI.Label(movesLeftRect, movesLeftText + "\n" + (movesLeft).ToString());
			
			// Some explanation of how to play
			if( controlsType == "android" || controlsType == "iphone" )
			{
				GUI.Label(new Rect(0, Screen.height - 60, Screen.width, 60), descriptionMobile);
			}
			else
			{
				GUI.Label(new Rect(0, Screen.height - 60, Screen.width, 60), description);
			}
		}
	}

	/// <summary>
	/// Represents a plug wire and where it is connected from and to.
	/// </summary>
	[Serializable]
	public class Plug
	{
		public Transform connectsFrom;
		public Transform connectsTo;
	}
}