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
            foreach(GameObject gameObject in Soldeier_childGameObjects)
            {
                SkinnedMeshRenderer[] renderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);
                foreach(MeshRenderer mesh in meshRenderers)
                {
                    mesh.enabled = false;
                }
                foreach (SkinnedMeshRenderer renderer in renderers)
                {
                    renderer.enabled = false;
                }

            }
        }
        else
        {
            foreach (GameObject game in FPS_Hands_childGameobjects)
            {
                SkinnedMeshRenderer[] renderers = game.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (SkinnedMeshRenderer renderer in renderers)
                {
                    renderer.enabled = false;
                }
                MeshRenderer[] meshRenderers = game.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer renderer in meshRenderers)
                {
                    renderer.enabled = false;
                }

                Camera[] cameras = game.GetComponentsInChildren<Camera>(true);
                    foreach (Camera cam in cameras)
                        cam.enabled = false;

                    AudioListener[] listeners = game.GetComponentsInChildren<AudioListener>(true);
                    foreach (AudioListener listener in listeners)
                        listener.enabled = false;
                Canvas[] canvases = game.GetComponentsInChildren<Canvas>(true);
                 foreach (Canvas canvas in canvases)
                {
                    canvas.enabled = false;
                }
                

            }
            foreach (GameObject gameObject in Soldeier_childGameObjects)
            {
                SkinnedMeshRenderer[] renderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (SkinnedMeshRenderer renderer in renderers)
                {
                    renderer.enabled = true;
                }

                MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);
                foreach (MeshRenderer mesh in meshRenderers)
                {
                    mesh.enabled = true;
                }

            }
             
        }
    }

    // Update is called once per frame
    

}
