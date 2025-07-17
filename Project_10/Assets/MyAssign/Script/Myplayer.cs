using System.Collections;
 
using Unity.VisualScripting;
using UnityEngine;
 
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System.Linq;






public class Myplayer : MonoBehaviourPun
{
   
    private bool getWeapon;
    public Text textForprop;
    public float rotationSpeed = 10f;
    public GameObject playerModel;
    public GameObject CameraPlace;
    public bool shouldShow;
    public float PanelMoveSpeed;
    public RectTransform panel;
    public GameObject RescureSlider;
    private float reviveTimer = 0f;
    public float reviveTime = 5f;
    private Coroutine reviveCoroutine = null;
    public GameObject[] PanelWhenDying;
    public TextMeshProUGUI WaitingText;
    public LayerMask layer;
    public CharacterController characterController;
    public Vector3 moveDirection;
    public Animator Animbody;
    public PhotonView photonview;
    public MyPlayerControls controls;
    public Vector2 moveInput;
    public bool isrun;
    public Weapon_automatic Myweapon;
    public InventorySystem inventory;
    public GameObject imageidle;
    public CameraShakeCinemachine camerashake;
    public GameObject imageCrouch;
    private bool isreborn;
    [Header("statistics")]
    public float Textspeed = 10f;
    public float speed;
    public float walkspeed = 4f;
    public float runSpeed = 6f;
    public float crouchspeed = 2f;
    private float jumpforce;
    private float fallforce = 10f;
    private bool isGround;
    public bool isjump;
    public bool isWalk;
    private CollisionFlags collisionFlags;
    private float DeathTimer;
    private float DeathTime;
    private Myplayer targetPlayers = null;
    private string nickname;
    public int scores;
    private float targetSpeed;
    public float acceleration=5f;
     


    public float crouchHeight;
    public float standHeight;
    public bool isCanCrouch;
    public bool isCrouching;
    public LayerMask crouchLayerMask;
    public bool isDamage;
    public bool isDead;
    public float playerHealth;
    public TextMeshProUGUI PlayerHealthUI;
    public Slider slider;
    public float Maxhealth =100f;
    public Image hurtImage;
    public Image hurtImage2;
    public AudioClip[] HurtClips;
    public Photon.Realtime.Player currentReviver = null;
    public float reviveRange;
    public GameObject UICanvas;
    public GameObject cameras;
    public Transform head;
    public Transform originaltransform;
    

    [Header("Music part")]
    public AudioClip walkSound;
    public AudioClip RunningSound;
    private AudioSource audiosource;
    public AudioSource audioForHurt;
    


    private void Awake()
    {
        shouldShow = false;
        RescureSlider.SetActive(false);
    
        DeathTime = 60f;
       controls = new MyPlayerControls();

        isCrouching = false;
        isrun = false;
        isWalk = false;
        
        controls.Player.Crouch.performed += ctx => crouch();

     




       
        controls.Player.Jump.performed += ctx => isjump = true;
        controls.Player.Jump.canceled += ctx => isjump = false;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
    


    void Start()
    {   
        isreborn = false;
        playerHealth = Maxhealth;
        characterController = GetComponent<CharacterController>();
        speed = walkspeed;
        crouchHeight = 1f;

        audiosource=GetComponent<AudioSource>();
        standHeight = characterController.height;
        PlayerHealthUI.text = ""+playerHealth;

    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, reviveRange);
    }


    private void Update()

    {
        float horizontal = Input.GetAxis("Horizontal"); // 对应 A/D 或 左/右箭头
        float vertical = Input.GetAxis("Vertical");     // 对应 W/S 或 上/下箭头
        speed = Mathf.Lerp(speed, targetSpeed, Time.deltaTime * acceleration);

        moveInput = new Vector2(horizontal, vertical);
        if (isDamage)
        {
            hurtImage.color = new Color(150 / 255f, 0f, 0f, 1f);
            hurtImage2.color = new Color(1f, 0f, 0f, 100 / 255f);
            isDamage = false;
            if(!isDead)
            {
                Animbody.CrossFadeInFixedTime("Hurt", 0.2f);
            }
        }
        else
        {
            hurtImage.color = Color.Lerp(hurtImage.color, Color.clear, Time.deltaTime * 2);
            hurtImage2.color = Color.Lerp(hurtImage.color, Color.clear, Time.deltaTime * 5);
        }

        Myweapon = inventory.weapons[inventory.currentWeaponID].GetComponent<Weapon_automatic>();

        Myweapon.gameObject.SetActive(!isDead);
       // ***************************TO********************************//
       //    Animbody.SetBool("IsReborn", isreborn);
        


        if (photonview != null)
        {
            if (!photonview.IsMine)
            {
                if (isDead&&!isreborn)
                {



                    foreach (GameObject obj in PanelWhenDying)
                    {
                        obj.SetActive(true);
                        WaitingText.text = "Waiting for rescue (" + photonview.Owner.NickName + ")";
                        float t = Mathf.PingPong(Time.time * Textspeed, 1f);
                        Color currentColor = Color.Lerp(new Color(1, 0, 0, 1), new Color(0, 0, 0, 1), t);
                        WaitingText.color = currentColor;
                    }
                }
                else
                {
                    foreach (GameObject obj in PanelWhenDying)
                    {
                        obj.SetActive(false);
                    }
                }
                return;
            }
        }
        
        Animbody.SetBool("Death", isDead);
        Animbody.SetBool("IsHit", !isDead && isDamage);
        

        
             

            if (isDead&&isreborn==false)
        {
            

             
            foreach (GameObject obj in PanelWhenDying)
            {
                obj.SetActive(true);
            }
            if(targetPlayers!=null)
            {
                RescureSlider.SetActive(false);


                photonView.RPC("SetReviver", RpcTarget.All, targetPlayers.photonView.ViewID);
                targetPlayers = null;
            }
            WaitingText.text = "Waiting for rescue(" + Mathf.Ceil(DeathTime)+ "S)";
            float t = Mathf.PingPong(Time.time * Textspeed, 1f);
            slider.value = 0;
            PlayerHealthUI.text = "0";


            Color currentColor = Color.Lerp(new Color(1,0,0,1),new Color(0,0,0,1), t);
            WaitingText.color = currentColor;

          
            DeathTime-=Time.deltaTime;
             
            if(DeathTime<=0)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            return;

        }
        else
        {
            foreach (GameObject obj in PanelWhenDying)
            {
                obj.SetActive(false);
            }
            
             Collider[] colliders = Physics.OverlapSphere(transform.position, reviveRange,layer);
            Collider[] collider2 = Physics.OverlapSphere(transform.position, reviveRange+2f, layer);
            foreach (Collider collider in collider2)
            {

                if (!colliders.Contains(collider))
                {
                    Myplayer myplayer = collider.GetComponent<Myplayer>();


                    if (myplayer != null&&targetPlayers==myplayer  )
                    {
                        shouldShow = false;
                        RescureSlider.SetActive(false);

                        reviveTimer = 0f;
                        photonView.RPC("SetReviver", RpcTarget.All, targetPlayers.photonView.ViewID);
                        targetPlayers = null;
                    }
                }

            }

            Collider[] colliderForProps = Physics.OverlapSphere(transform.position, reviveRange);
            Collider[] colliderForProps2 = Physics.OverlapSphere(transform.position, reviveRange + 2f);
            foreach (Collider collider in colliderForProps2)
            {

                if (colliderForProps.Contains(collider))
                {


                    if (collider.gameObject.CompareTag("Bomb"))
                    {
                        shouldShow = true;
                        textForprop.text = "Press E to check...";
                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            shouldShow = false;
                            collider.gameObject.GetComponent<LPLockActivator>().ActivateObject();

                            break;
                        }
                        break;



                    }
                    
                }
                else
                {
                    if (collider.gameObject.CompareTag("Bomb"))
                    {
                        shouldShow = false;
                    }
                }
                
                
            }
            

         

            float targetX = shouldShow ? -185 : 210;
            Vector2 anchoredPosition = panel.anchoredPosition;
            anchoredPosition.x = Mathf.MoveTowards(anchoredPosition.x, targetX, PanelMoveSpeed * Time.deltaTime);
            panel.anchoredPosition = anchoredPosition;


            foreach (var col in colliders)
            {   
                 
                Myplayer myplayer=col.GetComponent<Myplayer>();
                

                if (myplayer!=null&&myplayer.isDead==true&&isDead==false&&myplayer.isreborn==false)
                {   if (myplayer.currentReviver == null || myplayer.currentReviver == photonView.Owner)
                    {   
                        if(targetPlayers==null||targetPlayers==myplayer)
                        {
                            targetPlayers = myplayer;
                        }
                        else
                        {
                            reviveTimer = 0f;

                            if (photonView.IsMine)
                                RescureSlider.SetActive(false);

                            
                                photonView.RPC("SetReviver", RpcTarget.All, targetPlayers.photonView.ViewID);
                                targetPlayers = null;
                            
                        }
                        shouldShow = true;
                        textForprop.text = "Long Press F to rebirth";


                        Debug.Log("The player is enter");
                        if (Input.GetKey(KeyCode.F))
                        {
                            photonView.RPC("StartReviving", RpcTarget.All, myplayer.photonView.ViewID);
                            if (photonview.IsMine)
                            {
                                RescureSlider.SetActive(true); // 显示进度条

                                RescureSlider.GetComponent<Slider>().value = reviveTimer / reviveTime;

                            }
                            reviveTimer += Time.deltaTime;
                            if (reviveTimer >= reviveTime)
                            {
                                shouldShow = false;
                                myplayer.photonView.RPC("SetHealth", RpcTarget.AllBuffered);
                                targetPlayers = null;

                                reviveTimer = 0;
                                if (photonview.IsMine)
                                {
                                    RescureSlider.SetActive(false);
                                }
                                photonView.RPC("SetReviver", RpcTarget.All, myplayer.photonView.ViewID);

                            }
                        }
                        else
                        {
                            reviveTimer = 0f;
                            photonView.RPC("SetReviver", RpcTarget.All, myplayer.photonView.ViewID);
                            if (photonview.IsMine)
                            {
                                RescureSlider.SetActive(false); // 显示进度条

                                 

                            }
                        }
                        break;
                    }
                    
                }
            }
        }

            if(isDead)
        {
            return;
        }

        
        //animbody.SetBool("Run", isrun);
       
     Animbody.SetFloat("Speed", moveInput.magnitude * speed);
        // moveInput.magnitude* speed

        //  animbody.SetFloat("Horizontal", moveInput.x);
        // animbody.SetFloat("Vertical", moveInput.y);


        Animbody.SetBool("Crouch", isCrouching);

        if (isCrouching)
        {
            imageidle.SetActive(false);
            imageCrouch.SetActive(true);
        }
        else
        {
            imageidle.SetActive(true);
            imageCrouch.SetActive(false);
        }

        if(playerHealth>0)
        {
         slider.value = playerHealth / Maxhealth;
            PlayerHealthUI.text = "" + playerHealth;

        }
        else
        {
            slider.value = 0;
            PlayerHealthUI.text = "0";
        }
        if (controls.Player.Sprint.IsPressed() && moveDirection.sqrMagnitude > 0 && !isCrouching)
        {
            if (isGround) { targetSpeed = runSpeed; isrun = true; }
        }
        else
        {
            if (!isCrouching) { targetSpeed = walkspeed; isrun = false; }
            else { isrun = false; }


        }
       

        Moving();
        jump();
        Cancrouch();
        PlayerFootSoundSet();
        if (!isrun && moveDirection.sqrMagnitude > 0)
        {
            isWalk = true;
        }
        else
        {
            isWalk = false;
        }

    }

    [PunRPC]
    public void SetReviver(int targetViewID)
    {
        PhotonView targetPV = PhotonView.Find(targetViewID);
        Myplayer targetPlayer = targetPV.GetComponent<Myplayer>();
        targetPlayer.currentReviver = null;
    }
    [PunRPC]
    public void StartReviving(int targetViewID)
    {
        PhotonView targetPV = PhotonView.Find(targetViewID);
        if (targetPV == null) return;

        Myplayer targetPlayer = targetPV.GetComponent<Myplayer>();
        if (targetPlayer == null || !targetPlayer.isDead) return;

       
        targetPlayer.currentReviver =photonView.Owner;

        
        


        
        
    }
   
    public void jump()
    {   

        

        if (!isGround)
        {
            jumpforce = jumpforce - fallforce * Time.deltaTime;
            Vector3 jump = new Vector3(0, jumpforce * Time.deltaTime, 0);
            collisionFlags = characterController.Move(jump);
            if (collisionFlags == CollisionFlags.Below)
            {
                isGround = true;
                jumpforce = 0f;
            }
            if (collisionFlags == CollisionFlags.None)
            {
                isGround = false;
            }

        }
        else
        {
            isGround = false;
        }
        if (isCrouching)
        {
            return;
        }
        if (isjump && isGround)
        {
            isGround = false;
            Animbody.SetBool("Jump", true);
            jumpforce = 5f;
        }
        else if(isGround)
        {
            Animbody.SetBool("Jump", false);
        }

    }
 
    public void Cancrouch()
    { 
     Vector3 sphereLocation  =  transform.position + Vector3.up * standHeight;
       isCanCrouch=( Physics.OverlapSphere(sphereLocation, characterController.radius, crouchLayerMask).Length)==0 && Physics.Raycast(transform.position, Vector3.down, 0.1f) ;
        Vector3 sphereLocation2 = transform.position;
         


    }
    public void crouch( )
    {   
        if(!isCanCrouch)
        {
            return;
        }
        isCrouching =  !isCrouching;
        if(isCrouching==true)
        {
            targetSpeed = crouchspeed;
        }
        characterController.height = isCrouching ? crouchHeight : standHeight;
        characterController.center = characterController.height / 2.0f * Vector3.up;
    }
    public void PlayerFootSoundSet()
    {
        if(isGround&&moveDirection.sqrMagnitude>0)
        {   
            audiosource.clip = isrun ? RunningSound : walkSound;
             
            if(!audiosource.isPlaying)
            {
                audiosource.Play();

            }
            

           
        }
        else
        {
            if (audiosource.isPlaying)
            {
                audiosource.Pause();
            }
        }
        if (isCrouching)
        {
            if (audiosource.isPlaying)
            {
                audiosource.Pause();
            }
        }
    }
    public void pickUpWeapon(GameObject weapon)
    {
        if(inventory.weapons.Contains(weapon))
        {
            weapon.GetComponent<Weapon_automatic>().BulletLeft = weapon.GetComponent<Weapon_automatic>().BulletMag * 3;
            weapon.GetComponent<Weapon_automatic>().UpdateAmmoUI();
            return;
        }
        else
        {   
            if(inventory.weapons.Count<10)
            {
                inventory.weapons.Add(weapon);
            }
           
        }
    }
   
   


    public IEnumerator reborn()
    {
        yield return new WaitForSeconds(8f);
        isDead = false;
        isreborn = false;

    }
    [PunRPC]
    public void SetHealth()
    {
        isreborn = true;
        StartCoroutine(reborn());
   
        playerHealth = 20f;
    }


    public void PlayerHealth(float damage)
    {   
        if(isDead)
        {
            return;
        }
        if(photonview!=null )
        { 
            if( photonview.IsMine)
            {
             photonview.RPC("RPCPlayerHealth",RpcTarget.AllBuffered,damage);
            }
         
        }
        else
        {
            playerHealth -= damage;
        isDamage=true;
        
         
        audioForHurt.clip = HurtClips[Random.Range(0, HurtClips.Length)];
        audioForHurt.time = 0.2f;
        audioForHurt.Play();
       // audioForHurt.PlayOneShot(HurtClips[Random.Range(0, HurtClips.Length)]);

        if (playerHealth <=0)
        {
            isDead = true;
             
        }
        }
         
        
        
    }
    [PunRPC]
    public void RPCPlayerHealth(float damage)
    {
        playerHealth -= damage;
        isDamage = true;


        audioForHurt.clip = HurtClips[Random.Range(0, HurtClips.Length)];
        audioForHurt.time = 0.2f;
        audioForHurt.Play();
        // audioForHurt.PlayOneShot(HurtClips[Random.Range(0, HurtClips.Length)]);

        if (playerHealth <= 0)
        {
            isDead = true;
            
            DeathTime = 60f;
         
           

        }
         
    }

     IEnumerator DelayDeath()
    {// TTTTTTTTTTTTTTTTTTTTTTTTTTTDDDDDDDDDDDDDDDDDDDDDDDDoooooooooooooooo
        yield return new WaitForSeconds(60f);
        if(isDead)
        {
            Destroy(gameObject, 10f);
        }
       

    }
    public void Moving()
    {


        // 1. 摄像机前后左右方向（忽略上下角度）
        Vector3 camForward = CameraPlace.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = CameraPlace.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        // 2. 得到角色在摄像机方向的移动方向
          moveDirection = (camRight * moveInput.x + camForward * moveInput.y).normalized;

        characterController.Move(moveDirection * speed * Time.deltaTime);
        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            playerModel.transform.rotation = Quaternion.Slerp(playerModel.transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
