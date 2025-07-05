using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using Unity.VisualScripting;
using TMPro;

public class MobileFPSGameManager : MonoBehaviour
{
    [SerializeField]
    GameObject playerPrefab;
 
    public List<GameObject> alivePlayers = new List<GameObject>();
    private int currentIndex = 0;
    public GameObject myplayer;
    private bool isdestroy;
   
    private GameObject cachemyplayer;
    private List<GameObject> CachealivePlayers =  new List<GameObject>();
    private float timer;
    // Start is called before the first frame update
    void Start()
    {
       
        isdestroy = false;
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (playerPrefab != null)
            {
                int randomPoint = Random.Range(1220, 1230);
                PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(randomPoint, 105f, 1300f),Quaternion.identity);
            }
            else
            {
                Debug.Log("Place PlayerPrefab");
            }
        }
        

    }
    

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.openClose();

        }

        CachealivePlayers = GameObject.FindGameObjectsWithTag("Player")
         .Where(p => p != null).ToList(); // 加其他检查如是否活着
        cachemyplayer = GameObject.FindGameObjectsWithTag("Player")
   .FirstOrDefault(p => {
       var view = p.GetComponent<PhotonView>();
       return view != null && view.IsMine;
   });
        if(alivePlayers!= CachealivePlayers)
        {
            alivePlayers = CachealivePlayers;
        }
        if(myplayer!= cachemyplayer)
        {
            myplayer=cachemyplayer;
        }

        //TODO for myplayerIsAliveOrNot
        if(myplayer==null&&isdestroy==false)
        {
            isdestroy = true;
            SwitchToNextPlayer();
            timer = 100f;



        }
        if(isdestroy)
        {
            timer -= Time.deltaTime;
            UIManager.Instance.text.text="Wait For Respawn "+Mathf.Ceil(timer);
            UIManager.Instance.respawnpanel.SetActive(true);
            if(timer <= 0f)
            {
                isdestroy = false;
                UIManager.Instance.respawnpanel.SetActive(false);
                if (playerPrefab != null)
                {
                   



                    Camera[] cameras = alivePlayers[currentIndex].GetComponentsInChildren<Camera>();
                    Canvas[] canvases = alivePlayers[currentIndex].GetComponentsInChildren<Canvas>();

                    SkinnedMeshRenderer[] renderers = alivePlayers[currentIndex].GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach (SkinnedMeshRenderer renderer in renderers)
                    {

                        if (renderer.gameObject.name != "TP_CH_3D")
                        {
                            renderer.enabled = false;
                        }
                        else
                        {
                            SkinnedMeshRenderer[] render2 = renderer.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                            foreach (SkinnedMeshRenderer renderer3 in render2)
                            {
                                renderer3.enabled = true;


                            }
                            MeshRenderer[] meshRenderers = renderer.GetComponentsInChildren<MeshRenderer>(true);
                            foreach (MeshRenderer mesh in meshRenderers)
                            {
                                mesh.enabled = true;
                            }

                        }

                    }

                    foreach (Canvas canvas in canvases)
                    {
                        canvas.enabled = false;
                    }
                    foreach (Camera cam in cameras)
                        cam.enabled = false;

                    AudioListener[] listeners = alivePlayers[currentIndex].GetComponentsInChildren<AudioListener>(true);
                    foreach (AudioListener listener in listeners)
                        listener.enabled = false;



                    int randomPoint = Random.Range(-10, 10);

                    PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(randomPoint, 105f, 1300f), Quaternion.identity);




                }
                else
                {
                    Debug.Log("Place PlayerPrefab");
                }

            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SwitchToPreviousPlayer();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                SwitchToNextPlayer();
            }
        }

         
    }
    void SwitchToNextPlayer()
    {
        if (alivePlayers.Count == 0) return;

        Camera[] cameras = alivePlayers[currentIndex].GetComponentsInChildren<Camera>();
        Canvas[] canvases = alivePlayers[currentIndex].GetComponentsInChildren<Canvas>();

        SkinnedMeshRenderer[] renderers = alivePlayers[currentIndex].GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer renderer in renderers)
        {

            if (renderer.gameObject.name != "TP_CH_3D")
            {
                renderer.enabled = false;
            }
            else
            {
                SkinnedMeshRenderer[] render2 = renderer.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (SkinnedMeshRenderer renderer3 in render2)
                {
                    renderer3.enabled = true;
 

                }
                MeshRenderer[] meshRenderers = renderer.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer mesh in meshRenderers)
                {
                    mesh.enabled = true;
                }

            }

        }

        foreach (Canvas canvas in canvases)
        {
            canvas.enabled = false;
        }
        foreach (Camera cam in cameras)
            cam.enabled = false;

        AudioListener[] listeners = alivePlayers[currentIndex].GetComponentsInChildren<AudioListener>(true);
        foreach (AudioListener listener in listeners)
            listener.enabled = false;
        currentIndex = (currentIndex + 1) % alivePlayers.Count;
        SetSpectatorView(alivePlayers[currentIndex]);
    }
    void SetSpectatorView(GameObject target)
    {
        SkinnedMeshRenderer[] renderers = target.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer renderer in renderers)
        {   
            
            if(renderer.gameObject.name!= "TP_CH_3D")
            {
                renderer.enabled = true;
                Debug.Log(renderer);
            }
            else
            {
                SkinnedMeshRenderer[] render2 = renderer.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (SkinnedMeshRenderer renderer3 in render2)
                { renderer3.enabled = false; }
                MeshRenderer[] meshRenderers = renderer.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer mesh in meshRenderers)
                {
                    mesh.enabled = false;
                }
            }
           
        }

        Camera[] cameras = target.GetComponentsInChildren<Camera>(true);
        foreach (Camera cam in cameras)
            cam.enabled = true;
        Canvas[] canvases = target.GetComponentsInChildren<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            canvas.enabled = false;
        }
        target.GetComponent<Myplayer>().hurtImage.transform. GetComponentInParent<Canvas>().enabled = true;
         

        AudioListener[] listeners = target.GetComponentsInChildren<AudioListener>(true);
        foreach (AudioListener listener in listeners)
            listener.enabled = true;
    }
    void SwitchToPreviousPlayer()
    {
        if (alivePlayers.Count == 0) return;
        Camera[] cameras = alivePlayers[currentIndex].GetComponentsInChildren<Camera>(true);
        foreach (Camera cam in cameras)
            cam.enabled = false;
        Canvas[] canvases = alivePlayers[currentIndex].GetComponentsInChildren<Canvas>(true);
        foreach (Canvas canvas in canvases)
        {
            canvas.enabled = false;
        }
        SkinnedMeshRenderer[] renderers = alivePlayers[currentIndex].GetComponentsInChildren<SkinnedMeshRenderer>(true);
        foreach (SkinnedMeshRenderer renderer in renderers)
        {

            if (renderer.gameObject.name != "TP_CH_3D")
            {
                renderer.enabled = false;
            }
            else
            {
                SkinnedMeshRenderer[] render2 = renderer.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (SkinnedMeshRenderer renderer3 in render2)
                    renderer3.enabled = true;
                MeshRenderer[] meshRenderers = renderer.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer mesh in meshRenderers)
                {
                    mesh.enabled = true;
                }
            }

        }

        AudioListener[] listeners = alivePlayers[currentIndex].GetComponentsInChildren<AudioListener>(true);
        foreach (AudioListener listener in listeners)
            listener.enabled = false;

        currentIndex = (currentIndex - 1 + alivePlayers.Count) % alivePlayers.Count;
        SetSpectatorView(alivePlayers[currentIndex]);
    }

}
