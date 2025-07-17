using System.Collections;
using UnityEngine;

#pragma warning disable 0649
#pragma warning disable 0414

namespace LHToolkit
{
	/// <summary>
	/// This script handles a standard lock, which requires the activator to rotate a lockpick while turning the cylinder in order to unlock
	/// it. The lockpick may break, and there is a limited number of lockpicks with the activator.
	/// </summary>
	public class LHBombLock : MonoBehaviour
	{
		// The camera attached to this bomb
		public Transform cameraObject;

		// The cutter object
		public Transform cutterObject;

		// The timer object
		public Transform timerObject;

		// The beeper object that will go off when cutting a lose wire
		public Transform beeperObject;

		// A list of all the wires in this bomb
		public Transform[] wires;

		// The currently selected wire
		private Transform currentWire;

		// A list of the indexes of the list of all wires
		private GameObject[] wiresIndexArray;

		// The number of win/fail/timer wires available, must not be more than the total number of wires
		public int winWiresCount = 1;
		public int failWiresCount = 1;
		public int timerWiresCount = 1;

		// A list of the correct wires, fail wires, and timer wires
		internal Transform[] winWires;
		internal Transform[] failWires;
		internal Transform[] timerWires;

		// How many win wires need to be cut in order to win
		public int cutsToWin = 1;

		// How many fail wires need to be cut in order to fail
		public int cutsToFail = 1;

		// The multiplier for the speed when cutting a timer wire
		public float timerSpeedChange = 1.1f;

		// How much time is left before we fail
		public float timeLeft = 99;

		// How quickly the timer counts
		private float timerSpeed = 1;

		// A list of available wire colors, from which 3 are randomly chosen to represent win/fail/timer wires
		public Color[] wireColors;

		// The highlight color of the wire when we select it
		public Color wireHighlight;

		// Holds the previous color of the selected wire
		internal Color tempColor;

		// The the material of the beeper when it beeps
		public Material beeperMaterial;

		// A counter for the beeper
		private float beeperCount = 1;

		// The highlighted shader
		private Shader highlightShader;
		private Shader tempShader;

		// The speed at which you orbit around the bomb
		public float rotateSpeed = 300;

		// The speed at which the bomb will go off
		public float rotateFailSpeed = 3;

		// How much time before the bomb goes off if we are rotating it too fast
		public float rotateFailTime = 1;

		// How long it takes to cut a wire
		public float wireCutTime = 1;
		private float wireCutTimeCount = 0;

		// Are we cutting the wire now?
		private bool isCuttingWire = false;

		// Minutes and seconds for the timer
		private int minutes = 0;
		private int seconds = 0;

		// Did we succeed or did we fail?
		private bool success = false;
		private bool failure = false;

		// The name of the tool in the player's inventory which is required to unlock this lock ( lockpicks, bobbypins, safe cracker tools, etc )
		public string requiredTool = "bomb defusal set";

		// The index of the tool in the player's inventory
		internal int requiredToolIndex;

		// The container of this lock ( A door, a safe, etc )
		internal Transform lockParent;
		
		// The activator object
		internal GameObject activator;
		
		// Various sounds
		public AudioClip soundCut;
		public AudioClip soundBeep;
		public AudioClip soundFail;
		public AudioClip soundUnlock;
		
		// GUI for button graphics
		public GUISkin GUISkin;

		// The description for how to play this lock
		public string description = "Click on a wire to cut it. Choose the correct color to disarm the bomb and be careful not to cut \nthe wrong wire. Hold the Middle Mouse Button to rotate the bomb and get a better view of the wires.";

		// Holds the type of controls we use, mobile or otherwise
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
			// Detect if we are running on Android or iPhone
			#if UNITY_IPHONE
    	controlsType = "iphone";
    	print("iphone");
  		#endif

  		#if UNITY_ANDROID
    	controlsType = "android";
    	print("android");
  		#endif

			// Deactivate the activator and container objects so they don't interfere with the lockpicking gameplay
			activator.SetActive(false);
			//lockParent.gameObject.SetActive(false);

			// If the total number of win/fail/timer wires exceeds the total number of wires, display a warning
			if( winWiresCount + failWiresCount + timerWiresCount > wires.Length )
			{
				Debug.LogWarning("The number of win/fail/timer wires is larger than the total number of wires available");
			}

			// Randomize the wire types (win, fail, timer)
			RandomizeBuiltinArray(wires);

			// Randomize the colors of the wires
			RandomizeBuiltinArrayColor(wireColors);

			// Make sure we have at least 3 colors for the wire types
			if( wireColors.Length < 3 )
			{
				Debug.LogWarning("Must have at least 3 wire colors");
			}
			else
			{
				// Go through all the wires
				foreach( Transform wire in wires )
				{
					// Find the already-cut model of the wire and deactivate it
					wire.Find("WireCut").gameObject.SetActive(false);

					// Assign the win wires
					if( winWiresCount > 0 )
					{
						winWiresCount--;
				
						// Set the name
						wire.name = "winWire";
				
						// Set the color
						wire.GetComponent<Renderer>().material.color = wireColors[0];
				
						// Set the color to the already-cut model of the wire too
						wire.Find("WireCut").GetComponent<Renderer>().material.color = wireColors[0];
					}
					else if( failWiresCount > 0 )
					{
						failWiresCount--;

						// Set the name
						wire.name = "failWire";

						// Set the color
						wire.GetComponent<Renderer>().material.color = wireColors[1];

						// Set the color to the already-cut model of the wire too
						wire.Find("WireCut").GetComponent<Renderer>().material.color = wireColors[1];
					}
					else if( timerWiresCount > 0 )
					{
						timerWiresCount--;

						// Set the name
						wire.name = "timerWire";

						// Set the color
						wire.GetComponent<Renderer>().material.color = wireColors[2];

						// Set the color to the already-cut model of the wire too
						wire.Find("WireCut").GetComponent<Renderer>().material.color = wireColors[2];
					}
				}
			}

			// Set the shader of the highlight
			highlightShader = Shader.Find("Self-Illumin/Diffuse");

			// Put the original shader in a remporary variable
			tempShader = GetComponent<Renderer>().material.shader;

			// Hide the cutter object
			StartCoroutine(HideCutter(0));
		}

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		public void Update ()
		{
			//Show the cursor and allow it to move while we interact with the lock 
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;

			// If we have not yet failed or succeeded, keep the game running
			if( failure == false && success == false )
			{
				// Count down the time left until the bomb explodes
				timeLeft -= Time.deltaTime * timerSpeed;
				
				// Calculate the minutes left
				minutes = (int)Mathf.Floor(timeLeft / 60);

				// Calculate the seconds left
				seconds = (int)Mathf.Floor(timeLeft - minutes * 60);

				// Assign the time in the text mesh
				timerObject.GetComponent<TextMesh>().text = minutes.ToString("00") + ":" + seconds.ToString("00") + ":" + (Mathf.Floor(10 * (timeLeft % 1))).ToString("0");//(Mathf.Ceil(timeLeft * 100)/100).ToString("00:00:0");
				
				// If we have less than 10 seconds left, start beeping
				if( timeLeft <= 10 )
				{
					// Count up the beeper timer
					beeperCount += Time.deltaTime * timerSpeedChange;
					
					// As we get closer to explosion start beeping faster and faster
					if( timeLeft <= 10 && beeperCount >= 1 )
					{
						StartCoroutine(HighlightObject(beeperObject, 0.2f));
						
						beeperCount = 0;
					}
					else if( timeLeft <= 5 && beeperCount >= 0.5f )
					{
						StartCoroutine(HighlightObject(beeperObject, 0.2f));
						
						beeperCount = 0;
					}
					else if( timeLeft <= 2 && beeperCount >= 0 )
					{
						StartCoroutine(HighlightObject(beeperObject, 0.5f));
						
						beeperCount = 0;
					}
				}
				
				// If there is no more time left, fail and set off the bomb
				if( timeLeft <= 0 )
				{
					StartCoroutine(Fail());
				}
				
				// If you hold down the middle mouse button, you can rotate the bomb around
				if( Input.GetButton("Fire3") )
				{
					// Rotate around based on mouse movement
					cameraObject.RotateAround(transform.position, cameraObject.right, Input.GetAxis("Mouse Y") * Time.deltaTime * -rotateSpeed);
					cameraObject.RotateAround(transform.position, cameraObject.up, Input.GetAxis("Mouse X") * Time.deltaTime * rotateSpeed);
					
					// If we rotate the bomb too quickly, start beeping.
					if( Mathf.Abs(Input.GetAxis("Mouse Y") * Time.deltaTime * -rotateSpeed) > rotateFailSpeed || Mathf.Abs(Input.GetAxis("Mouse X") * Time.deltaTime * rotateSpeed) > rotateFailSpeed )
					{
						rotateFailTime -= Time.deltaTime;
						
						// Highlight the beeper object
						StartCoroutine(HighlightObject(beeperObject, 0.1f));
						
						// If we rotate the bomb for too long, explode
						if( rotateFailTime <= 0 )
						{
							StartCoroutine(Fail());
						}
					}
				}
                else if (Input.touchCount > 0 )
                {
                    // Rotate around based on mouse movement
                    cameraObject.RotateAround(transform.position, cameraObject.right, Input.GetTouch(0).deltaPosition.y * Time.deltaTime * -rotateSpeed * 0.1f);
                    cameraObject.RotateAround(transform.position, cameraObject.up, Input.GetTouch(0).deltaPosition.x * Time.deltaTime * rotateSpeed * 0.1f);

                    // If we rotate the bomb too quickly, start beeping.
                    if (Mathf.Abs(Input.GetTouch(0).deltaPosition.y * Time.deltaTime * -rotateSpeed * 0.1f) > rotateFailSpeed || Mathf.Abs(Input.GetTouch(0).deltaPosition.x * Time.deltaTime * rotateSpeed * 0.1f) > rotateFailSpeed)
                    {
                        rotateFailTime -= Time.deltaTime;

                        // Highlight the beeper object
                        StartCoroutine(HighlightObject(beeperObject, 0.1f));

                        // If we rotate the bomb for too long, explode
                        if (rotateFailTime <= 0)
                        {
                            StartCoroutine(Fail());
                        }
                    }
                }


                // Check hits with the wires
                if ( isCuttingWire == false )
				{
					RaycastHit hit;
					Ray ray = cameraObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
		
					if( Physics.Raycast(ray, out hit, 100) )
					{
						// Allow hits only with a wire
						if( hit.collider.tag == "LHwire" && currentWire == null )
						{
							// Set the current wire and highlight it
							currentWire = hit.collider.transform;
							currentWire.GetComponent<Renderer>().material.shader = highlightShader;
							
							// Set the position and rotation of the cutter based on the current wire
							//cutterObject.position = (hit.collider as SphereCollider).center + transform.position;
							//cutterObject.rotation = Quaternion.LookRotation(cutterObject.position - transform.position);
							cutterObject.position = currentWire.Find("IconCut").transform.position;
							cutterObject.rotation = Quaternion.LookRotation(cutterObject.position - currentWire.position);
							
						}
					}
					else
					{
						// Clear the highlight off other wires
						foreach( Transform wire in wires )
						{
							if( wire )
								wire.GetComponent<Renderer>().material.shader = tempShader;
						}
						
						StartCoroutine(HideCutter(0.2f));
					}
				}
			}
			
			// If we have a wire selected...
			if( currentWire != null )
			{
				// If we click LMB start cutting the wire
				if( Input.GetButtonDown("Fire1") )
				{
					isCuttingWire = true;
					
					ShowCutter();
				}
				
				// If we are cutting the wire...
				if( isCuttingWire == true )
				{
					// Count up to the cut
					wireCutTimeCount += Time.deltaTime;
					
					// If we completed the cutting process, check which wire we got
					if( wireCutTimeCount >= wireCutTime )
					{
						isCuttingWire = false;
						
						// Check if we got a win/lose/timer wire
						if( currentWire.name == "winWire" )
						{
							cutsToWin--;
							
							// If we cut enough win wires, unlock the bomb
							if( cutsToWin <= 0 )
							{
								StartCoroutine(Unlock());
							}
						}
						else if( currentWire.name == "failWire" )
						{
							cutsToFail--;
							
							// If we cut enough fail wires, explode the bomb
							if( cutsToFail <= 0 && failure == false )
							{
								StartCoroutine(Fail());
							}
						}
						else if( currentWire.name == "timerWire" )
						{
							// If we cut a timer wire, change timer speed
							timerSpeed *= timerSpeedChange;
						}
						
						//Replace the wire with an already-cut wire object
						currentWire.Find("WireCut").gameObject.SetActive(true);
						currentWire.Find("WireCut").parent = currentWire.parent;
						
						// Play a cut sound
						GetComponent<AudioSource>().PlayOneShot(soundCut);
						
						// Remove the current wire
						Destroy(currentWire.gameObject);
						
						// Hide the cutter object
						StartCoroutine(HideCutter(0.2f));
					}
				}
			}
			
			// If we unclick LMB, cancel the cutting process
			if( Input.GetButtonUp("Fire1") )
			{
				StartCoroutine(HideCutter(0));
			}
		}

		/// <summary>
		/// OnGUI is called for rendering and handling GUI events.
		/// This means that your OnGUI implementation might be called several times per frame (one call per event).
		/// For more information on GUI events see the Event reference. If the MonoBehaviour's enabled property is 
		/// set to false, OnGUI() will not be called.
		/// </summary>
		public void OnGUI()
		{
		    GUI.skin = GUISkin;

		    // Some explanation of how to play
		    GUI.Label( new Rect( 0, Screen.height - 60, Screen.width, 60), description );
		}

		/// <summary>
		/// Shows the cutter object.
		/// </summary>
		internal void ShowCutter()
		{
			// Hide the wire icon
			if ( currentWire )
				currentWire.Find("IconCut").gameObject.SetActive(false);
		
			// Show the cutter object
			cutterObject.gameObject.SetActive(true);
		
			// Play the cutter animation
			cutterObject.GetComponent<Animation>().Play();
		}

		/// <summary>
		/// Hides the cutter object.
		/// </summary>
		/// <param name='delay'>
		/// <see cref="Float"/> delay length.
		/// </param>
		internal IEnumerator HideCutter( float delay )
		{
			// Show the wire icon
			if ( currentWire )
				currentWire.Find("IconCut").gameObject.SetActive(true);
		
			// Reset the cutting timer
			wireCutTimeCount = 0;

			// No wire is selected
			currentWire = null;
		
			// We are not cutting a wire
			isCuttingWire = false;
		
			// Wait a little
			yield return new WaitForSeconds(delay);
		
			//And hide the cutter object
			if ( cutterObject.gameObject.activeSelf == true )
			{
                // Play the cutter animation
                cutterObject.GetComponent<Animation>().Rewind();
                cutterObject.GetComponent<Animation>().Stop();

                //cutterObject.gameObject.SetActive(false);
            }
		}

		/// <summary>
		/// Highlights the object by replacing it's shader.
		/// </summary>
		/// <param name='target'>
		/// <see cref="Transform"/>
		/// </param>
		/// <param name='beepTime'>
		/// <see cref="Float"/>Beep time.
		/// </param>
		internal IEnumerator HighlightObject( Transform target, float beepTime )
		{
			// Set the highlighted shader
			target.GetComponent<Renderer>().material.shader = highlightShader;

			// Play a beeping sound
			GetComponent<AudioSource>().PlayOneShot(soundBeep);

			// Wait for a while
			yield return new WaitForSeconds(beepTime);

			// Reset the back to the normal shader
			if ( target )
				target.GetComponent<Renderer>().material.shader = tempShader;
		}

		/// <summary>
		/// Unlocks and opens a container. After that the container will have an unlocked lock.
		/// </summary>
		internal IEnumerator Unlock()
		{
			success = true;
		
			timerSpeed = 0;
		
			// Set and play relevant sounds
			GetComponent<AudioSource>().Stop();
			GetComponent<AudioSource>().PlayOneShot(soundUnlock);
		
			// Wait for a second
			yield return new WaitForSeconds(2);
		
			// Set the container to unlocked and activate it
			if ( lockParent )
			{
				LHContainer container = lockParent.GetComponent<LHContainer>();

				lockParent.gameObject.SetActive(true);
				container.locked = false;
				container.Activate();
			}
			//	Exit the bomb difuse game
			Exit();
		}

		/// <summary>
		/// Runs when we fail to disarm the bomb.
		/// </summary>
		internal IEnumerator Fail()
		{
			failure = true;
		
			timerSpeed = 0;
		
			timerObject.GetComponent<TextMesh>().text = ("BYE BYE").ToString();
		
			GetComponent<AudioSource>().PlayOneShot(soundFail);
		
			// Wait for a second
			yield return new WaitForSeconds(1);

			// Activate the fail functions on the container.
			if (lockParent)
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
		internal void Exit()
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
		/// Randomizes an array of colors.
		/// </summary>
		/// <param name='array'>
		/// <see cref="Color"/> Array
		/// </param>
		internal void RandomizeBuiltinArrayColor (Color[] array)
		{
			// Go through all the objects and randmoize them
			for( int index = array.Length - 1; index > 0; index-- )
			{
				// Choose a random index from the array
				int randomIndex = Random.Range(0, index);
		
				// Put the object in a temporary variable
				Color temp = new Color(array[index].r, array[index].g, array[index].b, array[index].a);
		
				// Put the random index object into the current index object
				array[index] = array[randomIndex];
		
				// Place the temporary object into the random index object
				array[randomIndex] = temp;
			}
		}

		/// <summary>
		/// Randomizes the builtin array.
		/// </summary>
		/// <param name='array'>
		/// <see cref="Object"/> Array.
		/// </param>
		internal void RandomizeBuiltinArray (Object[] array)
		{
			// Go through all the objects and randmoize them
			for( int index = array.Length - 1; index > 0; index-- )
			{
				// Choose a random index from the array
				int randomIndex = Random.Range(0, index);
		
				// Put the object in a temporary variable
				Object temp = array[index];
		
				// Put the random index object into the current index object
				array[index] = array[randomIndex];
		
				// Place the temporary object into the random index object
				array[randomIndex] = temp;
			}
		}
	}
}
