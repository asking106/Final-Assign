using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ending : MonoBehaviour
{
    public List<GameObject> Enterplayers = new List<GameObject>();
    public MobileFPSGameManager MobileFPSGamemanager;
    public GameObject ending;
    public PhotonView photonview;
    
    public GameObject PlayerlistPrefab;
    public GameObject panel;
    bool isdone = false;
    public bool isgone = false;
    // Start is called before the first frame update
    void Start()
    {
     photonview = GetComponent<PhotonView>();   
        ending.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {   
        if (isgone && MobileFPSGamemanager.alivePlayers!=null&& isdone==false)
        {
            isdone = true;
            foreach (GameObject alive in MobileFPSGamemanager.alivePlayers)
            {
                GameObject playerlistGameobject = Instantiate(PlayerlistPrefab);
                playerlistGameobject.transform.SetParent(panel.transform);
                playerlistGameobject.transform.Find("Name").GetComponent<Text>().text = alive.GetComponent<Myplayer>().photonView.Owner.NickName;
                playerlistGameobject.transform.Find("Score").GetComponent<Text>().text =""+ alive.GetComponent<Myplayer>().scores;

            }

            Debug.Log("SSSSSSS");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (PhotonNetwork.IsMasterClient)
            {
                photonview.RPC("BroadcastPause", RpcTarget.AllBuffered);
            }
        }

    }
    [PunRPC]
    public void BroadcastPause()
    {
        Time.timeScale = 0f;
        ending.SetActive(true);
    }
     public void AreAllEnterPlayersAlive()
    {
        if (MobileFPSGamemanager.alivePlayers != null)
        {


            foreach (GameObject player in MobileFPSGamemanager.alivePlayers)
            {
                if (!Enterplayers.Contains(player) || Enterplayers == null)
                {
                    isgone= false;
                    return;
                }
                
            }
            isgone= true;
        }
        else
        {
            isgone = false;
        }    
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag=="Player")
        {
            Enterplayers.Add(other.gameObject);
            AreAllEnterPlayersAlive();
        }


    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Enterplayers.Remove(other.gameObject);
        }
    }
}
