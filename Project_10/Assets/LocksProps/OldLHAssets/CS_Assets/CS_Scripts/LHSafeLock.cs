using System.Collections;
using UnityEngine;

namespace LHToolkit
{
	/// <summary>
	/// This script handles a safe lock, which requires the activator to rotate a dial 
	/// while moving a stethoscope to try and find the sound source. 
	/// After that the activator should move the dialin a direction until a loud click is heard,
	/// then he must rotate the other way until another click is heard, and so until the safe lock is open.
	/// </summary>
	public class LHSafeLock : MonoBehaviour
	{
		// The camera of this lock
		public Camera cameraObject;

		// The stethoscope object
		public Transform stethoscope;
	
		// The hotspot object, which is the point at which you hear the loudest sound with the stethoscope.
		public Transform hotspotObject; 
		
		// How quickly the stethoscope focuses on an area after we stop moving it
		public float focusSpeed = 1;
	
		// The dial object, its rotation speed and target rotation
		public Transform dial;
		public float dialSpeed = 100;
		
		// The range of the hotspot, within this you hear the sound of the rotating dial clearly
		public float hotspotRange = 0.05f;
	
		// The falloff the hotspot, where the sound of the dial turning starts to fade until you don't hear it anymore
		public float hotspotFalloff = 0.1f;
	
		// The sequence we must follow in order to unlock this safe
		public int[] sequence;
	
		// The direction we should rotate in ( 1 - clockwise/right, 2 - counterclockwise - left )
		public int direction = 1;
	
		// How far in the wrong direction must a dial rotate before the whole sequence is reset 
		public float dialReset = 20;
	
		// State of the lock: 0 intro, 1 unlocking, 2 unlocked
		internal int lockState = 0; 
	
		// Should the hotspot be placed randomly?
		public bool randomHotspot = true;
		
		// The name of the tool in the player's inventory which is required to unlock this lock ( lockpicks, bobbypins, safe cracker tools, etc )
		public string requiredTool = "safecracking tools";

		// The index of the tool in the player's inventory
		internal int requiredToolIndex;
	
		// Various sounds
		public AudioClip soundTurn;
		public AudioClip soundClick;
		public AudioClip soundReset;
		public AudioClip soundUnlock;
		public AudioClip soundFail;

		// GUI for button graphics
		public GUISkin guiSkin;
	
		// The position and size of the "abort" button
		public Rect abortRect = new Rect(0, 0, 100, 50);
		public string abortText = "Abort";

		//The description for how to play this lock
		public string description = "Move the stethoscope (Mouse) while turning the dial (A/D) until you find the spot with the highest click sound. Then turn the dial either left or right until you hear a louder click, then turn the other way until you hear another click, and so on until the sequence is complete and the safe is unlocked.";
		public string descriptionMobile = "Move the stethoscope (Swipe right side of the screen) while turning the dial (Swipe left side of the screen) until you find the spot with the highest click sound. Then turn the dial either left or right until you hear a louder click, then turn the other way until you hear another click, and so on until the sequence is complete and the safe is unlocked.";

		// Icons for the mobile controls
		public Transform mobileIcons;
		internal float focus = 0;
		internal float dialTarget = 0;
		internal int   sequenceIndex = 0;
		internal float dialResetCount = 0;
		
		// The container of this lock ( A door, a safe, etc )
		internal Transform lockParent;
		
		// The activator object, holds lockpicks and whatnot
		internal GameObject activator;

		// Touch state and index for the stethoscope
		internal bool touchStethoscope = false;
		internal int touchStethoscopeIndex = -1;
		
		// Touch state and index for the dial
		internal bool touchDial = false;
		internal int touchDialIndex = -1;

		// The location of the hotspot
		private Vector3 hotspot = Vector3.zero;

		//Holds the type of controls we use, mobile or otherwise
		private string controlsType = string.Empty;

		public bool lockCursorOnExit = false; //Should we lock the mouse pointer on Exit?

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled. 
		/// This allows you to delay any initialization code, until it is really needed. 
		/// Awake is always called before any Start functions. 
		/// This allows you to order initialization of scripts
		/// </summary>		
		public void Start ()
		{
			//Detect if we are running on Android or iPhone
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

			// If we are not on a mobile platform, disable the mobile icons
			if( controlsType == "android" || controlsType == "iphone" )
			{
		
			}
			else
			{
				if( mobileIcons )
				{
					mobileIcons.gameObject.SetActive(false);
				}
			}

			//Set a random hotspot
			BoxCollider collider = GetComponent<BoxCollider>();
			
			if( randomHotspot == true )
			{	
				hotspotObject.localPosition = new Vector3(collider.center.x + collider.size.x * Random.Range(-0.5f, 0.5f),
					                                        collider.center.y + collider.size.y * Random.Range(-0.5f, 0.5f),
					                                        hotspotObject.localPosition.z);
			}

			// Assign the hotspot based on the object position
			hotspot.x = hotspotObject.position.x;
			hotspot.y = hotspotObject.position.y;
			hotspot.z = hotspotObject.position.z;
			
			// If we set the direction to anything other than 1 or -1, set it to 1
			if( direction != 1 && direction != -1 )
				direction = 1;
		
			// Skipping the intro animation
			lockState = 1;

			// Silencing the stethoscope at the start of the game
			GetComponent<AudioSource>().volume = 0;
		}
	
		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>		
		public void Update ()
		{
			//Show the cursor and allow it to move while we interact with the lock 
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			// Check touches and assign fingers to the correct controls (stethoscope or dial)
			if( controlsType == "android" || controlsType == "iphone" )
			{
				CheckTouch();
			}
	
			// We are in the "unlocking" state
			if( lockState == 1 )
			{
				// Change the focus based on the touch movement. The faster it moves the less focus we have
				if( (controlsType == "android" || controlsType == "iphone") && touchStethoscopeIndex > -1 )
				{
					// If there is 1 touch on the screen, assume it belongs to this stethoscope control. If there are 2 touches, move the control based on the correct index
					if( Input.touches.Length == 1 )
					{
						// If the touch moved, lose focus. Otherwise, return focus to normal
						if( Input.touches[0].phase == TouchPhase.Moved )
						{
							focus = Mathf.Lerp(focus, 0, Time.deltaTime * 5);
						}
						else
						{
							focus = Mathf.Lerp(focus, 1, Time.deltaTime * 5);
						}
					}
					else if( Input.touches.Length == 2 )
					{
						// If the touch moved, lose focus. Otherwise, return focus to normal
						if( Input.touches[touchStethoscopeIndex].phase == TouchPhase.Moved )
						{
							focus = Mathf.Lerp(focus, 0, Time.deltaTime * 5);
						}
						else
						{
							focus = Mathf.Lerp(focus, 1, Time.deltaTime * 5);
						}
					}
				}
				else
				{
					// Change the focus based on the mouse movement. The faster it moves the less focus we have
					// If the touch moved, lose focus. Otherwise, return focus to normal
					if( Input.GetAxis("Mouse X") != 0 )
					{
						focus = Mathf.Lerp(focus, 0, Time.deltaTime * 5);
					}
					else
					{
						focus = Mathf.Lerp(focus, 1, Time.deltaTime * 5);
					}
				}

				// Check hits with the safe surface
				RaycastHit hit;
				Ray ray = cameraObject.ScreenPointToRay(Input.mousePosition);
		
				// Check if we are touching the screen, moving the stethoscope
				if( (controlsType == "android" || controlsType == "iphone") && touchStethoscopeIndex > -1 )
				{
					// If there is 1 touch on the screen, assume it belongs to this stethoscope control. If there are 2 touches, move the control based on the correct index
					if( Input.touches.Length == 1 )
					{
						// Raycast from the camera to the touch position
						ray = cameraObject.ScreenPointToRay(Input.touches[0].position);
					}
					else if( Input.touches.Length == 2 )
					{
						// Raycast from the camera to the touch position
						ray = cameraObject.ScreenPointToRay(Input.touches[touchStethoscopeIndex].position);
					}
				}
		
				// If the raycast hits
				if( Physics.Raycast(ray, out hit, 50) )
				{
					// Allow hits only with this safe lock
					if( hit.collider.transform == transform )
					{
						// Move the stethoscope along the surface of the safe
						stethoscope.position = hit.point + Vector3.forward * 0.02f;
						stethoscope.Translate(Vector3.forward * focus * -0.02f, Space.Self);
				    
						// Check the distance between the stethoscope and the hotspot
						float distance = Vector3.Distance(stethoscope.position, hotspot);
				    
						// If we are well out of the hotspot falloff, then there is no sound
						if( distance > hotspotRange + hotspotFalloff )
						{
							GetComponent<AudioSource>().volume = 0;
						}
						else if( distance > hotspotRange )
						{
							// When we start getting closer to the hotspot range, the sound starts rising
							GetComponent<AudioSource>().volume = (distance - hotspotRange - hotspotFalloff) / (hotspotFalloff - hotspotRange - hotspotFalloff);
						}
						else
						{
							// We are within the hotspot range, full sound volume
							GetComponent<AudioSource>().volume = 1;
						}
				    
						// The sound is affected by the focus of the stethoscope. Lower focus means lower sound
						GetComponent<AudioSource>().volume *= focus;
					}
				}
			
				// Check if we are touching the screen, rotating the dial
				if( (controlsType == "android" || controlsType == "iphone") && touchDialIndex > -1 )
				{
					// If there is 1 touch on the screen, assume it belongs to this dial control. If there are 2 touches, move the control based on the correct index
					if( Input.touches.Length == 1 )
					{
						// If the touch is moving right, else if it's moving left
						if( Input.touches[0].deltaPosition.x > 0 )
						{
							// Dial right at a normal speed
							DialRight(1);
						}
						else if( Input.touches[0].deltaPosition.x < 0 )
						{
							// Dial left at a normal speed
							DialLeft(1);
						}
					}
					else if( Input.touches.Length == 2 )
					{
						// If the touch is moving right, else if it's moving left
						if( Input.touches[touchDialIndex].deltaPosition.x > 0 )
						{
							DialLeft(1);
						}
						else if( Input.touches[touchDialIndex].deltaPosition.x < 0 )
						{
							DialRight(1);
						}
					}
				}
				else
				{
					// If we are pressing D dial left, otherwise if we press A dial right
					if( Input.GetAxis("Horizontal") > 0 )
					{
						DialLeft(1);
					}
					else if( Input.GetAxis("Horizontal") < 0 )
					{
						DialRight(1);
					}
				}
		
				// Allowing the reset counter to loop
				if( dialResetCount < 0 )
					dialResetCount = 0;

				if( dialResetCount > dialReset * 3.6f )
					dialResetCount = dialReset * 3.6f;

				// If we swipe left/right, rotate the dial
				if( (controlsType == "android" || controlsType == "iphone") && touchDialIndex > -1 )
				{
					// If there is 1 touch on the screen, assume it belongs to this dial control. If there are 2 touches, move the control based on the correct index
					if( Input.touches.Length == 1 )
					{
						// If the touch is not stationary, run the DialRotate function
						if( Input.touches[0].deltaPosition.x != 0 )
						{
							DialRotate();
						}
					}
					else if( Input.touches.Length == 2 )
					{
						// If the touch is not stationary, run the DialRotate function
						if( Input.touches[touchDialIndex].deltaPosition.x != 0 )
						{
							DialRotate();
						}
					}
				}
				else if( Input.GetAxis("Horizontal") != 0 )
				{
					// If the mouse is moving, run the DialRotate function
					DialRotate();
				}
				else if( lockState != 2 )
				{
					// Stop all sounds if we unlocked this safe
					if( GetComponent<AudioSource>().isPlaying == true )
						GetComponent<AudioSource>().Stop();
				}
			
				// If we reach the end of the sequence, unlock the safe, we WIN
				if( sequenceIndex >= sequence.Length )
				{
					lockState = 2; 
				
					StartCoroutine(Unlock());
				}
			}
		}

		/// <summary>
		/// Turns the dial counterclockwise (increasing the numbers on the dial).
		/// </summary>
		/// <param name='multiplier'>
		/// Multiplier.
		/// </param>
		public void DialLeft (float multiplier)
		{
			// Set the dial target based on our turning speed
			dialTarget -= dialSpeed * Time.deltaTime * multiplier;

			// Reset it when it gets negative
			if( dialTarget < 0 )
			{
				dialTarget += 360;
			}

			// If we are turning the dial opposite the correct direction, it will reset the dial sequence
			if( direction == -1 )
			{
				DialReset();
			}
			else if( 100 - Mathf.Round(dialTarget / 3.6f) == sequence[sequenceIndex] )
			{
				// If we are rotating in the correct direction, go on to the next item in the sequence
				dialResetCount = 0;
		
				sequenceIndex++;
		
				direction *= -1;
		
				GetComponent<AudioSource>().PlayOneShot(soundClick);
		
				//print( "next in sequence -> direction " + direction.ToString() + " dial " + sequence[sequenceIndex].ToString() );
			}
		}

		/// <summary>
		/// Turns the dial clockwise (decreasing the numbers on the dial).
		/// </summary>
		/// <param name='multiplier'>
		/// Multiplier.
		/// </param>
		public void DialRight (float multiplier)
		{
			// Set the dial target based on our turning speed
			dialTarget += dialSpeed * Time.deltaTime * multiplier;

			// Reset it when it gets negative
			if( dialTarget > 360 )
			{
				dialTarget -= 360;
			}

			// If we are turning the dial opposite the correct direction, it will reset the dial sequence
			if( direction == 1 )
			{
				DialReset();
			}
			else if( 100 - Mathf.Round(dialTarget / 3.6f) == sequence[sequenceIndex] )
			{
				// If we are rotating in the correct direction, go on to the next item in the sequence
				dialResetCount = 0;
		
				sequenceIndex++;
		
				direction *= -1;
		
				GetComponent<AudioSource>().PlayOneShot(soundClick);
			}
		}

		/// <summary>
		/// Plays a sound and sets the exact dial position when we rotate left or right.
		/// </summary>
		public void DialRotate ()
		{
			GetComponent<AudioSource>().clip = soundTurn;
				
			if( GetComponent<AudioSource>().isPlaying == false )
				GetComponent<AudioSource>().Play();

			Vector3 newEulerAngles = dial.localEulerAngles;
			newEulerAngles.z = Mathf.Round(dialTarget / 3.6f) * 3.6f;
			dial.localEulerAngles = newEulerAngles;
		}

		/// <summary>
		/// Resets the dial sequence.
		/// </summary>
		public void DialReset()
		{
			// Count the dial reset value
			dialResetCount += dialSpeed * Time.deltaTime;
			
			// Reset the entire sequence if we move too much in the opposite direction
			if ( dialResetCount >= dialReset * 3.6f )
			{
				while ( sequenceIndex > 0 )
				{
					sequenceIndex--;
					
					direction *= -1;
				}
				
				dialResetCount = 0;
				
				GetComponent<AudioSource>().PlayOneShot(soundReset);
			}
		}

		/// <summary>
		/// Unlocks and opens a container, after that the container will have an unlocked lock.
		/// </summary>
		public IEnumerator Unlock ()
		{
			// Set and play relevant sounds
			GetComponent<AudioSource>().Stop();
			GetComponent<AudioSource>().PlayOneShot(soundUnlock);
		
			// Wait for 2 second
			yield return new WaitForSeconds(2);
		
			
			//Set the container to unlocked and activate it
			if ( lockParent )
			{
				LHContainer container = lockParent.GetComponent<LHContainer>();

				lockParent.gameObject.SetActive(true);
				container.locked = false;
				container.Activate();
			}
			
			//Exit the bomb difuse game
			Exit();
		}
	
		/// <summary>
		/// Runs when we fail to unlock the lock.
		/// </summary>
		public IEnumerator Fail()
		{
			GetComponent<AudioSource>().PlayOneShot(soundFail);
			
			// Wait for a second
			yield return new WaitForSeconds(1);
			
			// Activate the fail functions on the container
			if ( lockParent )
			{
				lockParent.gameObject.SetActive(true);
				lockParent.GetComponent<LHContainer>().FailActivate();
			}
			
			// Exit the bomb difuse game
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

		/// <summary>
		/// Check touches on mobile platforms. It looks for 2 touches, one for the stethoscope and one for the dial.
		/// </summary>
		public void CheckTouch ()
		{
			// Look for 2 touches
			if( Input.touchCount <= 2 )
			{
				// Go through all available touches
				for( int index = 0; index < Input.touchCount; index++ )
				{
					// If the touch just began
					if( Input.touches[index].phase == TouchPhase.Began )
					{
						// If the touch is within the right side of the screen, assign it to the stethoscope
						if( Input.touches[index].position.x > Screen.width * 0.6f )
						{
							touchStethoscope = true;
							touchStethoscopeIndex = index;
						}
				
						// If the touch is within the left side of the screen, assign it to the dial
						if( Input.touches[index].position.x < Screen.width * 0.3f )
						{
							touchDial = true;
							touchDialIndex = index;
						}
					}
			
					// If a touch has ended, reset its index
					if( Input.touches[index].phase == TouchPhase.Ended )
					{
						// Reset index for stethoscope
						if( index == touchStethoscopeIndex )
						{
							touchStethoscope = false;
							touchStethoscopeIndex = -1;
						}
				
						// Reset index for dial
						if( index == touchDialIndex )
						{
							touchDial = false;
							touchDialIndex = -1;
						}
					}
				}
			}
		}

		/// <summary>
		/// Draws Gizmos in editor objects.
		/// </summary>
		public void OnDrawGizmos ()
		{
			// The hotspot area
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(new Vector3(hotspot.x, hotspot.y, hotspot.z), hotspotRange);
		
			// The hotspot falloff area
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(new Vector3(hotspot.x, hotspot.y, hotspot.z), hotspotRange + hotspotFalloff);
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
		
			// Abort button
			if( GUI.Button(abortRect, abortText) )
			{
				Exit();
			}
		
			// Some explanation of how to play
			if( controlsType == "android" || controlsType == "iphone" )
			{
				GUI.Label(new Rect(0, Screen.height - 100, Screen.width, 100), descriptionMobile);
			}
			else
			{
				GUI.Label(new Rect(0, Screen.height - 100, Screen.width, 100), description);
			}
		}
	}
}
