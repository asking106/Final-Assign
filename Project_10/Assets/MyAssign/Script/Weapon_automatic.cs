using Cinemachine;
using Photon.Pun;
using Photon.Pun.Demo.Asteroids;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

using UnityEngine.UI;
 



[System.Serializable]
public class SoundClip
{
     
    public AudioClip ShootSound;
    public AudioClip SilencerShootSound;
    public AudioClip ReloadSoundAmmoLeft;
    public AudioClip ReloadSoundOutOfAmmo;
    public AudioClip aimsound;
}

public class Weapon_automatic : Weapon
{
     public PhotonView photonview;
    private Camera mainCamera;
    public Camera Guncamera;
    private float Fieldofview;
    public Myplayer playerController;
    public Animator animator;
    private int ShotgunFragment;
    public float damageforShot;
    public float recoilamount = 2f;
    public float Aimrecoilamount = 1f;


    [Header("Weapon Component")]
    public Transform ShootPoint;
    public Transform BulletShootPoint;
    public Transform CasingBulletSpawnPoint;
    public Transform BulletPrefab;
    public Transform CasingPrefab;
    [Header ("weapon attrubute")]
    public float range;
    public float FireRate;
    private float originRate;
    public float SpreadFactor;
    private float fireTimer;
    private float bulletForce;
    public int BulletMag;
    public int currentBullets;
    public int BulletLeft;
    public bool IsSilencer;

    [Header("After Effects")]
    public Light MuzzleFlashLight;
    private float lightDuration; 
    public ParticleSystem MuzzlePartic;
    public ParticleSystem MuzzleParticSpark;
    public int MinEmit;
    public int MaxEmit;
    private Animator animbody;
    [Header("UI")]
 
    public float CurrentExpandedDegree;
    public float crossExpandedDegree;
    private float MaxCrossDegree;
    public TextMeshProUGUI AmmoTextUi;
    public TextMeshProUGUI ShootModelTextUi;
    public Image[] CrossQuaterImags;
    /*ToDO*/
    private string ShootModeName;
    public bool isAiming;
    private float speed ;
    [Header("Audio")]
    private AudioSource MainAudioSource;
    public SoundClip soundClip;

    public enum ShootMode
    {
        SemiGun,
        AutoRifle


    }
   
    public ShootMode shootmode;
    private bool gunShootInput;
    public bool IsReloading;
    private int modeNum;
    





    private void Awake()
    {
        ShotgunFragment = 8;
        speed = 100f;
      mainCamera = Camera.main;
        IsReloading = false;
       playerController =GetComponentInParent<Myplayer>();
        MainAudioSource=GetComponent<AudioSource>();
         
        animbody = playerController.Animbody;
    }

    private void Start()
    {
        
        if(Guncamera!=null)
        {
            Fieldofview = Guncamera.fieldOfView;
        }
        originRate =FireRate;
        bulletForce = 100f;
        MaxCrossDegree = 300f;
        
        MuzzleFlashLight.gameObject.SetActive(true);
        MuzzleFlashLight.enabled = false;
        lightDuration = 0.02f;
        range = 300f;
        BulletLeft = BulletMag * 2;
        currentBullets = BulletMag;
        UpdateAmmoUI();

        if(shootmode==ShootMode.AutoRifle)
        {
            modeNum = 1;
            ShootModeName = "Full Auto";
            UpdateAmmoUI();
        }
        else if(shootmode == ShootMode.SemiGun)
        {
            modeNum = 0;
            ShootModeName = "Semi Auto";
            UpdateAmmoUI();
        }
    }
    private void Update()
    {   
        if(playerController.photonview!=null)
        {
        if(!playerController.photonview.IsMine)
            {

            return;
             }
        }
        if(playerController.isDead)
        {
            return ;
        }
       
         
        if(isAiming)
        {
             
            mainCamera.fieldOfView = Mathf.MoveTowards(mainCamera.fieldOfView, 30f, speed*Time.deltaTime);
            if (gameObject.name == "SNIPER_01")
            {
                Guncamera.fieldOfView = Mathf.MoveTowards(Guncamera.fieldOfView, 30f, speed * Time.deltaTime);
            }

        }
        else
        {
            if (gameObject.name == "SNIPER_01")
            {
                Guncamera.fieldOfView = Mathf.MoveTowards(Guncamera.fieldOfView, Fieldofview, speed * Time.deltaTime);
            }

            mainCamera.fieldOfView = Mathf.MoveTowards(mainCamera.fieldOfView, 60f, speed * Time.deltaTime);
        }
        SpreadFactor =isAiming ? 0.01f : 0.1f;
        if(Input.GetMouseButton(1)&&!IsReloading)
        {
            photonView.RPC("isAim", RpcTarget.AllBuffered);
            animator.SetBool("Aim", isAiming);
            animbody.SetBool("Aim", isAiming);
            

        }
        else
        {
            photonView.RPC("isNotAim", RpcTarget.AllBuffered);
           // isAiming = false;
            animator.SetBool("Aim", isAiming);
            animbody.SetBool("Aim", isAiming);
        }
        if (shootmode == ShootMode.AutoRifle)
        {
            if (Input.GetKeyDown(KeyCode.X) && modeNum != 1)
            {
                modeNum = 1;
                ShootModeName = "Full Auto";
                UpdateAmmoUI();
            }
            else if (Input.GetKeyDown(KeyCode.X) && modeNum != 0)
            {
                modeNum = 0;
                ShootModeName = "Semi Auto";
                UpdateAmmoUI();
            }
            switch (modeNum)
            {
                case 0:
                    gunShootInput = Input.GetMouseButtonDown(0);
                    FireRate = originRate;
                    break;
                case 1:
                    gunShootInput = Input.GetMouseButton(0);
                    FireRate = 0.2f;
                    break;
            }
        }
        else
        {
            gunShootInput = Input.GetMouseButtonDown(0);
        }

        if (playerController.isrun == false && Vector3.SqrMagnitude(playerController.moveDirection) > 0 && playerController.isCrouching == false)
        {
            ExpandingCrossUpdate(crossExpandedDegree);
        }
        else if (playerController.isrun == true && Vector3.SqrMagnitude(playerController.moveDirection) > 0)
        {
            ExpandingCrossUpdate(crossExpandedDegree * 2);
        }
        else
        {
            ExpandingCrossUpdate(0);
        }
        if (fireTimer < FireRate)
        {
            fireTimer += Time.deltaTime;
        }
        if (gunShootInput)
        {
            if(photonview != null&&photonview.IsMine)
            {
                if (fireTimer < FireRate || currentBullets <= 0 || animator.GetCurrentAnimatorStateInfo(0).IsName("Take_out") || IsReloading)
                {
                    return;
                }
                photonview.RPC("FireRPC", RpcTarget.AllBuffered);
            }
            else
            {
                if (fireTimer < FireRate || currentBullets <= 0 || animator.GetCurrentAnimatorStateInfo(0).IsName("Take_out") || IsReloading)
                {
                    return;
                }
                GunFire();
            }
            
        }



        if(Input.GetKeyDown(KeyCode.I))
        {
            photonView.RPC("inspects", RpcTarget.AllBuffered);
        }

        animator.SetBool("Run",playerController.isrun);
        animbody.SetBool("Run", playerController.isrun);
        animator.SetBool("Walk",playerController.isWalk);
        animbody.SetFloat("Horizontal",playerController.moveInput.x);
        animbody.SetFloat("Vertical", playerController.moveInput.y);

       
        animbody.SetBool("Crouch",  playerController.isCrouching);

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if(info.IsName("Reload_ammo_left")||info.IsName("Reload_out_of_ammo")
           || info.IsName("reload_close")
           || info.IsName("reload_open")
           || info.IsName("reload_insert 1")
           || info.IsName("reload_insert 2")
           || info.IsName("reload_insert 3")
           || info.IsName("reload_insert 4")
           || info.IsName("reload_insert 5")|| info.IsName("reload_insert 6"))
        {
            IsReloading = true;
        }
        else
        {
            IsReloading=false;
        }
        if(Input.GetKeyDown(KeyCode.R)&&currentBullets<BulletMag&&BulletLeft>0&&!IsReloading)
        {
            IsReloading = true;
            //DoReloadAnimation();
            photonView.RPC("DoReloadAnimation", RpcTarget.AllBuffered);
             
        }
    }

    public override void AimIn()
    {
        for (int i = 0; i < CrossQuaterImags.Length; i++)
        {
            CrossQuaterImags[i].gameObject.SetActive(false);
        }

        MainAudioSource.PlayOneShot(soundClip.aimsound);
         

    }
    [PunRPC]
    public void isAim()
    {
        isAiming = true;    
    }
    [PunRPC]
    public void isNotAim()
    {
        isAiming = false;
    }
    [PunRPC]
    public void inspects()
    {
        animator.SetTrigger("Inspect");
        animbody.SetTrigger("Inspect");

    }
    public void Reloads()
    {
        for (int i = 0; i < CrossQuaterImags.Length; i++)
        {
            CrossQuaterImags[i].gameObject.SetActive(false);
        }
    }

    public override void AimOut()
    {

        for (int i = 0; i < CrossQuaterImags.Length; i++)
        {
            CrossQuaterImags[i].gameObject.SetActive(true);

        }


    }

 

    public override void ExpandingCrossUpdate(float expanDegree)
    {
       if(CurrentExpandedDegree <expanDegree-1)
        {
            ExpendCross(150*Time.deltaTime);
        }
       else if(CurrentExpandedDegree >expanDegree+1)
        {
            ExpendCross(-300*Time.deltaTime);
        }
    }
    public void ExpendCross(float add)
    {   
        CrossQuaterImags[0].transform.localPosition += new Vector3(-add,0,0);
        CrossQuaterImags[1].transform.localPosition += new Vector3(add, 0, 0);
        CrossQuaterImags[2].transform.localPosition += new Vector3(0, add, 0);
        CrossQuaterImags[3].transform.localPosition += new Vector3(0,-add, 0);
        CurrentExpandedDegree += add;
         
        
    }
    public IEnumerator Shoot_crss()
    {
        yield return null;
        for(int i = 0; i<5;i++)
        {
            ExpendCross(Time.deltaTime * 500);
        }

    }
    public void UpdateAmmoUI()
    {
        AmmoTextUi.text = currentBullets + "/" + BulletLeft;
        ShootModelTextUi.text = ShootModeName;
    }

    public override void GunFire()
    {

        //  if (fireTimer < FireRate || currentBullets <= 0 || animator.GetCurrentAnimatorStateInfo(0).IsName("Take_out") || IsReloading)
        //  {
        //     return;
        //  }
        if(!isAiming&&photonview.IsMine)
        {
            playerController.camerashake.CameraShake(recoilamount);
        }
        else if(photonview.IsMine) 
        {
            playerController.camerashake.CameraShake(Aimrecoilamount);
        }
        

        StartCoroutine(MuzzleFlashLights());
        MuzzlePartic.Emit(1);
        MuzzleParticSpark.Emit(Random.Range(MinEmit, MaxEmit));
        StartCoroutine(Shoot_crss());
        if (!isAiming)
        {
            animator.CrossFadeInFixedTime("Fire", 0.1f);
            if (!playerController.isCrouching)
                if(photonview!=null)
                {
                     
                        animbody.CrossFadeInFixedTime("Fire", 0.1f);
                    
                }
                else
                {
                     animbody.CrossFadeInFixedTime("Fire", 0.1f);
                }
               
        }
        else
        {
            animator.CrossFadeInFixedTime("Aim_fire", 0.1f);

        }

        if (gameObject.name == "SHOTGUN_01")
        { for(int i=0;i<ShotgunFragment;i++)
            {

            
            
            Vector3 ShootDirection = ShootPoint.forward;
            ShootDirection = ShootDirection + ShootPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor, SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));
             if(photonview.IsMine)
                {
                    photonview.RPC("instantShot", RpcTarget.All,ShootDirection);
                }
             
            }
            Instantiate(CasingPrefab, CasingBulletSpawnPoint.transform.position, CasingBulletSpawnPoint.transform.rotation);
            MainAudioSource.clip = IsSilencer ? soundClip.SilencerShootSound : soundClip.ShootSound;
            MainAudioSource.Play();
            fireTimer = 0f;
            currentBullets--;
            UpdateAmmoUI();
        }
        else
        {   
       
            Vector3 ShootDirection = ShootPoint.forward;
            ShootDirection = ShootDirection + ShootPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor, SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));
            if(photonview.IsMine)
            {
                photonview.RPC("instantNormal", RpcTarget.All, ShootDirection);
            }
            
            Instantiate(CasingPrefab, CasingBulletSpawnPoint.transform.position, CasingBulletSpawnPoint.transform.rotation);
            MainAudioSource.clip = IsSilencer ? soundClip.SilencerShootSound : soundClip.ShootSound;
            MainAudioSource.Play();
            fireTimer = 0f;
            currentBullets--;
            UpdateAmmoUI();
        }

        

    }
    [PunRPC]
    public void instantNormal(Vector3 vector)
    {
        RaycastHit hit;
        if (Physics.Raycast(ShootPoint.position, vector, out hit, range))
        {

            Transform bullet;
            bullet = (Transform)Instantiate(BulletPrefab, BulletShootPoint.transform.position, BulletShootPoint.transform.rotation);
            bullet.gameObject.GetComponent<Projectile>().myplayers = playerController.gameObject;
            bullet.GetComponent<Rigidbody>().velocity = (bullet.transform.forward + vector) * bulletForce;
            bullet.GetComponent<Projectile>().damage = damageforShot;
            Debug.Log(hit.transform.gameObject.name);


        }
    }
    [PunRPC]
    public void instantShot(Vector3 vector)
    {
        RaycastHit hit;
        if (Physics.Raycast(ShootPoint.position, vector, out hit, range))
        {


            Transform bullet;
            bullet = Instantiate(BulletPrefab, hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
            bullet.gameObject.GetComponent<Projectile>().myplayers = playerController.gameObject;
            bullet.GetComponent<Projectile>().damage = damageforShot;
            bullet.GetComponent<Rigidbody>().velocity = (bullet.transform.forward + vector) * bulletForce;
            Debug.Log(hit.transform.gameObject.name);






        }
    }
    public IEnumerator MuzzleFlashLights()
    {
        MuzzleFlashLight.enabled = true;
        yield return new WaitForSeconds(lightDuration);
        MuzzleFlashLight.enabled = false;
    }
    [PunRPC]
    public override void DoReloadAnimation()
    {   

        if(gameObject.name== "SHOTGUN_01")
        {
            if (currentBullets == BulletMag|| BulletLeft <= 0)
                return;
            if (!playerController.isCrouching)
            {
                animbody.CrossFadeInFixedTime("Reload_out_of_ammo", 0.1f);
            }
            animator.SetTrigger("Shotgun_Reload");
        }
        else
        {
            if (currentBullets > 0 && BulletLeft > 0)
            {

                animator.Play("Reload_ammo_left", 0);
                if (!playerController.isCrouching)
                {
                    animbody.CrossFadeInFixedTime("Reload_ammo_left", 0.1f);
                }


                MainAudioSource.PlayOneShot(soundClip.ReloadSoundAmmoLeft);
            }
            if (currentBullets == 0 && BulletLeft > 0)
            {
                animator.Play("Reload_out_of_ammo", 0);

                if (!playerController.isCrouching)
                {
                    animbody.CrossFadeInFixedTime("Reload_out_of_ammo", 0.1f);
                }

                MainAudioSource.PlayOneShot(soundClip.ReloadSoundOutOfAmmo);
            }
        }
      
    }
    [PunRPC]
    public void FireRPC() // 注意不能是 static
    {
        GunFire();
    }

    public void ShotGunReload()
    {
        
        if (currentBullets<BulletMag)
        {
            currentBullets++;
            BulletLeft--;
            UpdateAmmoUI();
        }
        if(currentBullets >= BulletMag)
        {
            animator.Play("reload_close");
            return;
        }
        if (BulletLeft <= 0)
        {
            animator.Play("reload_close");
            return;
        }

    }
    
  
    public override void Reload()
    {
        if (BulletLeft <= 0) return;
        int bulletToLoad= BulletMag - currentBullets;
       int bulletToReduce= BulletLeft >= bulletToLoad ? bulletToLoad : BulletLeft;
        BulletLeft-=bulletToReduce;
        currentBullets += bulletToReduce;
        UpdateAmmoUI();
    }
}
