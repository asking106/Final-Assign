using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventorySystem : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> weapons = new List<GameObject>();
    public int currentWeaponID;
    public GameObject weapon;
    private List<GameObject> weaponsbody = new List<GameObject>();
    private MyPlayerControls inputActions;
 private PhotonView photonView;
    public RuntimeAnimatorController controllerAll;
    public RuntimeAnimatorController controllerHandgun;
    public Animator animbody;
    
    private Vector2 scrollValue;
    private void Awake()
    { 
        inputActions = new MyPlayerControls();
    }
    void Start()
    {
        photonView=GetComponent<PhotonView>();
        currentWeaponID = 0;
        foreach (Transform child in weapon.transform)
        {
            if (child.childCount > 0)
            {
                weaponsbody.Add(child.gameObject);
            }
        }


    }
    private void OnEnable()
    {
      inputActions.Enable();
        inputActions.Player.ScrollWheel.performed += changeCurrentWeaponID;
        
    }
    private void OnDisable()
    {
        inputActions.Disable();
    }
    // Update is called once per frame
    void Update()
    {  
        if(photonView != null)
        {
            if(!photonView.IsMine)
            {
                return;
            }
        }
        for (int i = 0;i<10;i++)
        {
            if(Input.GetKeyDown(KeyCode.Alpha0+i))
            {
                int num = 0;
                if(i!=0)
                {
                    num = i - 1;
                }
                else
                {
                    num = 10;
                }
                if (num < weapons.Count)
                {
                    photonView.RPC("ChangeWeapon", RpcTarget.AllBuffered, num);
                    
                    //ChangeWeapon(num);

                }
            }
        }
    }
    public void changeCurrentWeaponID(InputAction.CallbackContext context)
    {
        if (photonView != null)
        {
            if (!photonView.IsMine)
            {
                return;
            }
        }
        scrollValue = context.ReadValue<Vector2>();
        if (scrollValue.y > 0) // Scroll Up
        {
            currentWeaponID--;
            if(currentWeaponID<0)
            {
                currentWeaponID = weapons.Count-1;
            }
           
        }
        else if (scrollValue.y < 0) // Scroll Down
        {
            currentWeaponID++;
            if (currentWeaponID >=weapons.Count)
            {
                currentWeaponID = 0;
            }
             
        }
        photonView.RPC("ChangeWeapon", RpcTarget.AllBuffered, currentWeaponID);
        //ChangeWeapon(currentWeaponID );
    }
    [PunRPC]
    public void ChangeWeapon(int weaponID)
    {

        currentWeaponID = weaponID;
        for (int i = 0; i < weapons.Count; i++)
        {
            if(weaponID==i)
            {
                weapons[i].gameObject. SetActive(true);
            }
            else
            {
                weapons[i].gameObject.SetActive(false);
            }
           
        }

        if (weapons[weaponID].gameObject.name.StartsWith("HANDGUN"))
        {
            animbody.runtimeAnimatorController = controllerHandgun;
        }
        else
        {
            animbody.runtimeAnimatorController = controllerAll;
        }
       

        foreach (GameObject body in weaponsbody)
        {
            
                // 如果子物体的名字与当前武器的名字一致，则激活，否则禁用
                if (body.name == weapons[weaponID].name)
                {
                    body.gameObject.SetActive(true);
                    
                }
                else
                {
                    body.gameObject.SetActive(false);
                }
            
        }
    }
}
