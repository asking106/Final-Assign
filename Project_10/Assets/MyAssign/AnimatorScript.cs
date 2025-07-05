using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorScript : MonoBehaviour
{
    public Weapon_automatic Weapon;
    public AudioSource audios;
    public Animator anim;


    // Start is called before the first frame update
    void Start()
    {
         
    }
    public void Reload_Sgun()
    {
        anim.Play("Reload");
    }
    public void Reload_Sgun_empty()
    {
        anim.Play("Reload Empty");
    }
    public void SGun_fire()
    {
        anim.Play("Fire");
    }
    public void ShotGun_Reload()
    {
        Weapon.ShotGunReload();
    }
    public void Reload_Sound(AudioClip audioclip)
    {
        audios.PlayOneShot(audioclip);
    }
    public void Aim_in()
    {
        Weapon.AimIn();
    }
    public void Aim_out()
    {
        Weapon.AimOut();
    }
    public void Reload()
    {
        Weapon.Reloads();
    }
    public void Reload_after()
    {
        Weapon.Reload();
    }
   
    // Update is called once per frame
    void Update()
    {
        
    }
}
