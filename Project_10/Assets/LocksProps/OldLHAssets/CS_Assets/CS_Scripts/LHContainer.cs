using System;
using LHToolkit.Misc;
using UnityEngine;

namespace LHToolkit
{
	/// <summary>
	/// This script handles containers, which are the objects that can be activated (door, safe, chest, etc). Each container can have a lock assigned to it, which 
	/// can be either locked or unlocked. If you click on a container with a locked lock, the lockpicking gameplay mechanism appears.
	/// </summary>
	public class LHContainer : MonoBehaviour
	{
		public static int picksLeft = 20;
		public Transform lockObj; // The lock object that will be activated when clicking on this container
		
		public bool locked = true; // Is the container locked?
		
		public GUISkin guiSkin; // A GUI for buttons and labels
		
		public Texture2D lockedIcon; // Display icon when the lock is locked.
		public Texture2D unlockedIcon; // Display icon when the lock is unlocked.
		
		public string lockedCaption = "Locked Door"; // Display text when the lock is locked.
		public string unlockedCaption = "Open Door"; // Display text when the lock is unlocked.
		
		public float captionWidth = 300; // GUI caption width.
		public float captionHeight = 100; // GUI caption height.
		
		public AudioClip soundLocked; // Locked sound.
		public AudioClip soundActivate; // Activate sound.
		
		public string activatorTag = "Player"; // The tag of the activator. This means that this container can be activated by the object with this tag only
		public string activateButton = "Fire1"; // The button that activates this container
		
		public ActivateFunctions[] activateFunctions; // A list of functions to be activated. Consists of a reciever and the name of the function to be run in it.
		public FailFunctions[] failFunctions; // A list of functions to be activated on failure. Consists of a reciever and the name of the function to be run in it.
		
		public AnimationClip[] activateAnimations; //A list of animations that are played in sequence activating this container. If no animation is set, the default animation is played
		public int animationIndex = 0;
		
		public float detectDistance = 3.0f; // How far off will the activator detect the container from
		public float detectAngle = 180 ; //
		
		internal GameObject activator; // The activator object that activates the container.
		
		internal bool activatorDetected = false; // Is the activator detected?
		internal int  requiredToolIndex = 0;     // The index of the tool in the inventory
		
		internal int    index = 0; // A general use index
		internal string requiredTool = string.Empty; // The name of the tool in the player's inventory which is required to unlock this lock ( lockpicks, bobbypins, safe cracker tools, etc )
		
		private string controlsType = string.Empty; // Holds the type of controls we use, mobile or otherwise
		private float  timeSinceTap = 0; // Time since we started the touch
		
		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled. 
		/// This allows you to delay any initialization code, until it is really needed. 
		/// Awake is always called before any Start functions. 
		/// This allows you to order initialization of scripts
		/// </summary>
		public void Start ()
		{
			// Detect if we are running on Android or iPhone
			#if UNITY_IPHONE
			controlsType = "iphone";
			print("iphone");
			#endif
			
			#if UNITY_ANDROID
			controlsType = "android";
			print("android");
			#endif
			
			// Assign the activator object by its tag.
			activator = GameObject.FindGameObjectWithTag(activatorTag);
			
			// If there is a lock object assigned
			if( lockObj )
			{
				// Get the name of the required tool from the locks and assign them to requiredTool
				if( lockObj.GetComponent<LHLock>() )
				{
					requiredTool = lockObj.GetComponent<LHLock>().requiredTool;
				}
				else if( lockObj.GetComponent<LHSafeLock>() )
				{
					requiredTool = lockObj.GetComponent<LHSafeLock>().requiredTool;
				}
				else if( lockObj.GetComponent<LHBombLock>() )
				{
					requiredTool = lockObj.GetComponent<LHBombLock>().requiredTool;
				}
				else if( lockObj.GetComponent<LHPanelLock>() )
				{
					requiredTool = lockObj.GetComponent<LHPanelLock>().requiredTool;
				}
			}
		}
		
		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		public void Update ()
		{
			// If an activator has been detected, wait for a button/touch
			if( activatorDetected )
			{
				// Check touch
				if( controlsType == "android" || controlsType == "iphone" )
				{
					// If there is more than 1 touch
					if( Input.touchCount > 0 )
					{
						//If the touch began, record its time
						if( Input.GetTouch(0).phase == TouchPhase.Began )
						{
							timeSinceTap = Time.time;
						}
						
						// If the touch ended, calculate its end time
						if( Input.GetTouch(0).phase == TouchPhase.Ended )
						{
							// If the touch time is less than 0.3 seconds, Activate!
							if( Time.time - timeSinceTap < 0.3 )
							{
								Activate();
							}
						}
					}
				}
				else if( !string.IsNullOrEmpty(activateButton) )
				{
					// If we press the activate button, Activate!
					if( Input.GetButtonDown(activateButton) )
					{
						Activate();
					}
				}
				else
				{
					// If no activate button is assigned, activate automatically
					Activate();
				}
			}
		}
		
		/// <summary>
		/// OnTriggerEnter is called when the Collider other enters the trigger.
		/// This message is sent to the trigger collider and the rigidbody (or the collider if there is no rigidbody) 
		/// that touches the trigger. Note that trigger events are only sent if one of the colliders also has a 
		/// rigid body attached.
		/// </summary>
		/// <param name="other">
		/// Other collider.
		/// </param>
		public void OnTriggerEnter (Collider other)
		{
			if( other.tag == activator.tag )
				activatorDetected = true;
		}
		
		/// <summary>
		/// OnTriggerExit is called when the Collider other has stopped touching the trigger.
		/// This message is sent to the trigger and the collider that touches the trigger. 
		/// Note that trigger events are only sent if one of the colliders also has a rigid body attached.
		/// </summary>
		/// <param name="other">
		/// Other collider.
		/// </param>
		public void OnTriggerExit (Collider other)
		{
			if( other.tag == activator.tag )
				activatorDetected = false;
		}
		
		/// <summary>
		/// A simple function to override the triggers and run the activator on demand.
		/// </summary>
		public void ManualActivate ()
		{
			activatorDetected = true;
		}
		
		/// <summary>
		/// A function that animates this container and plays a sound.
		/// </summary>
		public void AnimateContainer ()
		{			
			if( GetComponent<Animation>() )
			{
				if( !GetComponent<Animation>().isPlaying )
				{
					// If an activateAnimations is set, play it. Otherwise play the default animation
					if ( activateAnimations.Length > 0 )
					{
						if ( activateAnimations[animationIndex] )
						{
							//Play the current activation animation
							GetComponent<Animation>().Play(activateAnimations[animationIndex].name);
							
							//Cycle through the list of animations
							if ( animationIndex < activateAnimations.Length - 1 )    animationIndex++;
							else    animationIndex = 0;
						}
					}
					else
					{
						GetComponent<Animation>().Play();
					}
					
					// Play a sound if avaialble
					if( soundActivate )
						GetComponent<AudioSource>().PlayOneShot(soundActivate);
				}
			}
		}
		
		/// <summary>
		/// A function that stops the current animation
		/// </summary>
		public void StopAnimation()
		{			
			if( GetComponent<Animation>() )
			{
				GetComponent<Animation>().Stop();
			}
		}
		
		/// <summary>
		/// Activates when you fail at unlocking a lock. It's used for lock such as the bombLock.
		/// </summary>
		public void FailActivate ()
		{
			// Activate all the fail functions
			foreach( FailFunctions index in failFunctions )
			{
				if( index.reciever && !string.IsNullOrEmpty(index.functionName) )
					index.reciever.SendMessage(index.functionName);
			}
		}
		
		/// <summary>
		/// OnGUI is called for rendering and handling GUI events.
		/// This means that your OnGUI implementation might be called several times per frame (one call per event).
		/// For more information on GUI events see the Event reference. If the MonoBehaviour's enabled property is 
		/// set to false, OnGUI() will not be called.
		/// </summary>
		public void OnGUI ()
		{
			GUI.skin = guiSkin;
			
			if( activatorDetected == true )
			{
				if( locked == true )
				{
					//An icon that displays when the container is locked
					if ( lockedIcon )    GUI.DrawTexture(new Rect(Screen.width * 0.5f - lockedIcon.width * 0.5f, Screen.height * 0.5f - lockedIcon.height * 0.5f, lockedIcon.width, lockedIcon.height), lockedIcon);
					
					// A caption that displays when the container is locked
					if ( lockedCaption != string.Empty )    GUI.Label(new Rect(Screen.width * 0.5f - captionWidth * 0.5f, Screen.height * 0.5f - captionHeight * 0.5f, captionWidth, captionHeight), lockedCaption);
					
					//if ( requiredTool != string.Empty )
					//{
					if ( CheckRequiredTool() || requiredTool == "" )
					{
						// A caption that is displayed on a lock that we can open, but only if we have enough lockpicks
						if ( !string.IsNullOrEmpty(activateButton) && lockObj )
							GUI.Label( new Rect ( Screen.width * 0.5f - captionWidth * 0.5f, Screen.height * 0.5f + captionHeight * 0.5f, captionWidth, captionHeight), "Press " + activateButton + " to unlock");
					}
					else
					{
						// A caption that is displayed on a lock that we can open, but we don't have any lockpicks left
						if ( !string.IsNullOrEmpty(activateButton) && lockObj )
							GUI.Label( new Rect ( Screen.width * 0.5f - captionWidth * 0.5f, Screen.height * 0.5f + captionHeight * 0.5f, captionWidth, captionHeight), "You don't have " + requiredTool);
					}
					//}
				}
				else
				{
					//An icon that displays when the container is unlocked
					if ( unlockedIcon )    GUI.DrawTexture(new Rect(Screen.width * 0.5f - lockedIcon.width * 0.5f, Screen.height * 0.5f - lockedIcon.height * 0.5f, lockedIcon.width, lockedIcon.height), unlockedIcon);
					
					// A caption that displays when the container is unlocked
					if ( unlockedCaption != string.Empty )    GUI.Label(new Rect(Screen.width * 0.5f - captionWidth * 0.5f, Screen.height * 0.5f - captionHeight * 0.5f, captionWidth, captionHeight), unlockedCaption);
					
					// A caption that is displayed on a lock that is unlocked
					if( activateButton != string.Empty && lockObj )
						GUI.Label(new Rect(Screen.width * 0.5f - captionWidth * 0.5f, Screen.height * 0.5f + captionHeight * 0.5f, captionWidth, captionHeight), "Press " + activateButton + " to open");
				}
			}
		}
		
		/// <summary>
		/// This function activates a container: Checks if we have a lock, if it's locked, and if we have lockpicks.
		/// </summary>
		public void Activate ()
		{
			// If the container is locked, check if we have a lock assigned to it, in which case the lockpicking gameplay mechanism appears
			if( locked == true )
			{
				// Create a lockpick object and place it in front of the camera, activate lock
				if( lockObj )
				{
					activatorDetected = false;
					
					// If the activator is currently active...
					if( activator.activeSelf )
					{
						// Find the activator component and check if he has any picks left
						if( CheckRequiredTool() || requiredTool == "" )
						{
							// Create a new lock object and place at the center of the screen
							Transform newLock = (Instantiate(lockObj, activator.transform.position, Quaternion.identity) as Transform);
							
							// Check the type of lock component and assign it to the activator accordingly
							if( newLock.GetComponent<LHLock>() )
							{
								newLock.GetComponent<LHLock>().lockParent = transform;
								newLock.GetComponent<LHLock>().activator = activator;
							}
							else if( newLock.GetComponent<LHSafeLock>() )
							{
								newLock.GetComponent<LHSafeLock>().lockParent = transform;
								newLock.GetComponent<LHSafeLock>().activator = activator;
							}
							else if ( newLock.GetComponent<LHBombLock>() )
							{
								newLock.GetComponent<LHBombLock>().lockParent = transform;
								newLock.GetComponent<LHBombLock>().activator = activator;
							}
							else if ( newLock.GetComponent<LHPanelLock>() )
							{
								newLock.GetComponent<LHPanelLock>().lockParent = transform;
								newLock.GetComponent<LHPanelLock>().activator = activator;
							}
							// Deactivate the activator script so it doesn't interfere with the lockpicking gameplay
							activator.SetActive(false);
							
							// Disable this script while we interact with the lock
							//enabled = false;
							
							//Deactivate this gameObject while we interact with the lock
							gameObject.SetActive(false);
						}
						else
						{
							print("You don't have any " + requiredTool);
							
							activatorDetected = true;
							
							GetComponent<AudioSource>().PlayOneShot(soundLocked);
						}
					}
				}
				else
				{
					// The door can't be opened. Use this to make doors that can't be opened with lockpicking
					print("This container can't be unlocked");
					
					GetComponent<AudioSource>().PlayOneShot(soundLocked);
				}
			}
			else
			{
				// If the container is not locked, activate it
				foreach( var index in activateFunctions )
				{
					if( index.Reciever && index.FunctionName != null )   
						index.Reciever.SendMessage(index.FunctionName);
				}
			}
		}
		
		/// <summary>
		/// This funtion goes through all the items in the activator's inventory and checks 
		/// if we have at least 1 of the tool required to unlock this container
		/// </summary>
		/// <returns>
		/// Returns whether you have the required tool.
		/// </returns>
		public bool CheckRequiredTool ()
		{
			bool returnValue = false;
			
			if ( requiredTool != string.Empty )
			{
				if ( lockObj )
				{
					LHInventory tools = activator.GetComponent<LHInventory>(); // Get a reference to the tools inventory.
					
					// Go through all tools in the inventory
					for ( var index = 0 ; index < tools.inventory.Length ; index++ )
					{
						// If we have the correct tool name, and we have at least 1 of it, return true
						if ( tools.inventory[index].tool == requiredTool && tools.inventory[index].count > 0 )
						{
							returnValue = true;
							
							// Assign the index of the tool from the inventory to the lock object
							if ( lockObj.GetComponent<LHLock>() )
							{
								LHLock.requiredToolIndex = index;
							}
							else if ( lockObj.GetComponent<LHSafeLock>() )
							{
								lockObj.GetComponent<LHSafeLock>().requiredToolIndex = index;
							}
							else if ( lockObj.GetComponent<LHBombLock>() )
							{
								lockObj.GetComponent<LHBombLock>().requiredToolIndex = index;
							}
							else if ( lockObj.GetComponent<LHPanelLock>() )
							{
								LHPanelLock.requiredToolIndex = index;
							}
						}
					}
				}
			}
			else
			{
				returnValue = true;
			}
			
			return returnValue;
		}		
		
		/// <summary>
		/// Represents a single function to be invoked when you fail.
		/// </summary>
		[Serializable]
		public class FailFunctions
		{
			public Transform reciever;
			public string functionName;
		}
		
		/// <summary>
		/// Represents a single function to be invoked when the container is activated.
		/// </summary>
		[Serializable]
		public class ActivateFunctions
		{
			public Transform Reciever;
			public string FunctionName;
		}
	}
}