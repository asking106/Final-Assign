using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class UIManager : MonoBehaviour
{       
    public static UIManager Instance;
    public RectTransform panel;
    private bool isopen;
    public float PanelMoveSpeed;
    private float targetX;
    private float targetStartX;
    public TextMeshProUGUI text;
    public GameObject respawnpanel;




    // Start is called before the first frame update
    void Start()
    {
        targetX = -8f;
        targetStartX = 2000f;
        Instance = this;
        isopen = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(isopen)
        {
            Cursor.lockState = CursorLockMode.None;  // 锁定鼠标在屏幕中央
            Cursor.visible = true;                    // 隐藏鼠标
            Vector2 anchoredPosition = panel.anchoredPosition;
            anchoredPosition.x = Mathf.MoveTowards(anchoredPosition.x, targetX, PanelMoveSpeed * Time.deltaTime);
            panel.anchoredPosition = anchoredPosition;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;  // 锁定鼠标在屏幕中央
            Cursor.visible = false;
            Vector2 anchoredPosition = panel.anchoredPosition;
            anchoredPosition.x = Mathf.MoveTowards(anchoredPosition.x, targetStartX, PanelMoveSpeed * Time.deltaTime);
            panel.anchoredPosition = anchoredPosition;
        }
    }

    public void openClose()
    {
        isopen = !isopen;
    }
    public void Quit()
    {
        Application.Quit();

    }
   
    public void loadscenes()
    {

        PhotonNetwork.LoadLevel("LobbyScene");
        PhotonNetwork.LeaveRoom();
       // SceneManager.LoadScene("LobbyScene");
       
       
    }
}
