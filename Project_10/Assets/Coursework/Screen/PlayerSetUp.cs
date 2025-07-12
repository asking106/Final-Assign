using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityStandardAssets.Characters.FirstPerson;
using System.Linq;

public class PlayerSetUp : MonoBehaviourPunCallbacks
{
    public GameObject[] FPS_Hands_childGameobjects;
    public GameObject[] Soldeier_childGameObjects;
   
    

    // Start is called before the first frame update
    void Start()
    {   
        
        if (photonView.IsMine)
        {
            foreach(GameObject game in FPS_Hands_childGameobjects)
            {
                game.SetActive(true);
               
            }
            

        }
        else
        {
            foreach (GameObject game in FPS_Hands_childGameobjects)
            {
                game.SetActive(false);

            }
            
              


        }
             
       
    }

    // Update is called once per frame
    

}
