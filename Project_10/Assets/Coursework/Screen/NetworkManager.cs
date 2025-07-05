using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
public class NetworkManager : MonoBehaviourPunCallbacks
{

    [Header("connection Status")]
    public Text connectionStatusText;

    [Header("Login UI Panel")]
    public InputField playerNameInput;
    public GameObject Login_UI_Panel;
    [Header("Game Options UI Panel")]
    public GameObject gameOptions_UI_Panel;
    [Header("create Room UI panel")]
    public GameObject createRoom_UI_panel;
    public InputField roomNameInputField;

    public InputField MaxPlayerInputField;
    [Header("inside room UI panel")]
    public GameObject insideRoom_UI_panel;
    public Text roomInfoText;
    public GameObject PlayerlistPrefab;
    public GameObject PlayerListContent;
    public GameObject StartGameButton;
    [Header("Room List UI panel")]
    public GameObject RoomList_UI_Panel;
    public GameObject roomlistEntryPrefab;
    public GameObject roomListParentGameobject;
    [Header("Join Random Room UI Panel")]
    public GameObject JoinRandomRoom_UI_panel;
    public Animator anim1;
    public Animator anim2;
    public Animator anim3;
    public Animator anim4;
    public AudioSource audios;
    private PhotonView photon;

    private Dictionary<string, RoomInfo> cacheRoomList;
    private Dictionary<string, GameObject> roomlistgameObject;
    private Dictionary<int, GameObject> playerListgameobject;
    public GameObject loginPanel;
    public GameObject lobbyPanel;
    public GameObject StartPanel;
    public GameObject TopPanel;
    public Slider Slider;
    public TextMeshProUGUI text;
    public float Textspeed;
  

    

    
      
    


    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        photon=GetComponent<PhotonView>();
        StartPanel.SetActive(false);
        if (GameManager.Instance != null && GameManager.Instance.hasLoggedIn)
        {
            ActivatePanel(gameOptions_UI_Panel.name);
        }
        else
        {
            ActivatePanel(Login_UI_Panel.name);
             
        }
      
        cacheRoomList = new Dictionary<string, RoomInfo>();
        roomlistgameObject = new Dictionary<string, GameObject>();
        PhotonNetwork.AutomaticallySyncScene = true;

    }
                                                                                                 
    // Update is called once per frame
    void Update()
    {
        connectionStatusText.text = "Connection Status: " + PhotonNetwork.NetworkClientState;
    }
    #endregion
    #region UI Callbacks
    public void OnloginVuttonClicked()
    {
        string playerName = playerNameInput.text;
          
    
        GameManager.Instance.hasLoggedIn = true;
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("PlayerName is invalid");

        }
    }
    #endregion

    #region Photon Callbacks
    public override void OnConnected()
    {
         
        Debug.Log("coonected to the internet");
    }
    public override void OnConnectedToMaster()
    {
        
        Debug.Log(PhotonNetwork.LocalPlayer. NickName + " is connected to Photon");
        ActivatePanel(gameOptions_UI_Panel.name);

    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel(insideRoom_UI_panel.name);
        if(PhotonNetwork.LocalPlayer.IsMasterClient) {
            StartGameButton.SetActive(true);
            }
        else
        {
            StartGameButton.SetActive(false);
        }
        roomInfoText.text = "room name: " + PhotonNetwork.CurrentRoom.Name + "        " + "players/Max.players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
        if (playerListgameobject == null)
        {
            playerListgameobject = new Dictionary<int, GameObject>();
        }
        foreach (Player player in PhotonNetwork .PlayerList)
        {
           GameObject playerListgameObject = Instantiate(PlayerlistPrefab);
            playerListgameObject.transform.SetParent(PlayerListContent.transform);
            playerListgameObject.transform.localScale = Vector3.one;
            playerListgameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;
            if(player.ActorNumber==PhotonNetwork.LocalPlayer.ActorNumber)
            {
                playerListgameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
            }
            else
            {
                playerListgameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
            }
            playerListgameobject.Add(player.ActorNumber, playerListgameObject);

        }
       
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created");
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + "        " + " players/Max.Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;
        GameObject playerlistGameobject = Instantiate(PlayerlistPrefab);
        playerlistGameobject.transform.SetParent(PlayerListContent.transform);
        playerlistGameobject.transform.localScale = Vector3.one;
        playerlistGameobject.transform.Find("PlayerNameText").GetComponent<Text>().text = newPlayer.NickName;
        if(newPlayer.ActorNumber==PhotonNetwork.LocalPlayer.ActorNumber)
        {
            playerlistGameobject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
        }
        else
        {
            playerlistGameobject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
        }
        playerListgameobject.Add(newPlayer.ActorNumber, playerlistGameobject);

    }

    public void oncreateRoombuttonClicked()
    {
        string roomName = roomNameInputField.text;
        if(string.IsNullOrEmpty(roomName))
        {
            roomName = "room" + Random.Range(1000, 10000);

        }
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)int.Parse(MaxPlayerInputField.text);
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    public void OnjoinRandomRoomButtonClicked()
    {
        ActivatePanel(JoinRandomRoom_UI_panel.name);
        PhotonNetwork.JoinRandomRoom();
    }
    public void OnStartGameButtonClicked()
    {
        photon.RPC("Starts",RpcTarget.All);
        

    }
    [PunRPC]
    public void Starts()
    {   
        StartCoroutine(StartGame());

        anim1.SetBool("IsStart", true);
        anim2.SetBool("IsStart", true);
        anim3.SetBool("IsStart", true);
        anim4.SetBool("IsStart", true);
        audios.Play();
        ActivatePanel(StartPanel.name);
        TopPanel.gameObject.SetActive(false);
    }
    
    public IEnumerator StartGame()
    {
        float waitTime = 40f;
        float timer = 0f;
        float escHoldTime = 2f; 
        float escTimer = 0f;

        while (timer < waitTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.PingPong(Time.time * Textspeed, 1f);
            Color currentColor = Color.Lerp(new Color(65f / 255f, 65f / 255f, 65f / 255f, 1), new Color(188f / 255f, 188f / 255f, 188f / 255f, 1), t);
            text.color = currentColor;


            if (PhotonNetwork.IsMasterClient)
            {
                photon.RPC("processData", RpcTarget.All, Slider.value);
                if (Input.GetKey(KeyCode.Escape))
                {   
                    escTimer += Time.deltaTime;
                    Slider.value = escTimer / escHoldTime;
                    
                    
                    if (escTimer >= escHoldTime)
                    {
                        Slider.value = 1;
                    }

                    if (escTimer >= escHoldTime)
                    {
                        Debug.Log("ESC held down, skipping wait...");
                        break;
                    }
                }
                else
                {
                    Slider.value = 0f;
                    escTimer = 0f;
                }

                yield return null;
            }
            else
            {
                text.text = "Press ESC to skip  (For master client)";
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {   

            PhotonNetwork.LoadLevel("GameScene");
        }

    }
    [PunRPC]
    public void processData(float value)
    {
        Slider.value = value;

    }    
    public void onCancelButtonClicked()
    {
        ActivatePanel(gameOptions_UI_Panel.name);
    }
    public void onShowRoomListButtonClicked()
    {
        if(!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();

        }
        ActivatePanel(RoomList_UI_Panel.name);
    }

    public void onBackButtonClicked()
    {
        if(PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        ActivatePanel(gameOptions_UI_Panel.name);
    }
    public void OnleeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomInfoText.text = "Room name: " + PhotonNetwork.CurrentRoom.Name + "        " + " players/Max.Players: " + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers;

        Destroy(playerListgameobject[otherPlayer.ActorNumber].gameObject);
        playerListgameobject.Remove(otherPlayer.ActorNumber);
    }
    public override void OnLeftRoom()
    {
        
        ActivatePanel(gameOptions_UI_Panel.name);
        if (playerListgameobject != null)
        {
            foreach (GameObject playerlistGameobject in playerListgameobject.Values)
            {
                Destroy(playerlistGameobject);
            }
            playerListgameobject.Clear();
            playerListgameobject = null;
        }
        
       
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        clearRoomListView();
        foreach(var roomlistGameObject in roomlistgameObject.Values)
        {
            Destroy(roomlistGameObject);

        }
        roomlistgameObject.Clear();
        foreach(RoomInfo room in roomList)
        {
            Debug.Log(room.Name);
            if(!room.IsOpen || !room.IsVisible||room.RemovedFromList)
            {
                if(cacheRoomList.ContainsKey(room.Name))
                {
                    cacheRoomList.Remove(room.Name);
                }
            }
            else
            {
                if(cacheRoomList.ContainsKey(room.Name))
                {
                    cacheRoomList[room.Name] = room;

                }
                else
                {
                    cacheRoomList.Add(room.Name, room);
                }
            }
        }
        foreach(RoomInfo room in cacheRoomList.Values)
        {
            GameObject roomlistEntryGameobject = Instantiate(roomlistEntryPrefab);
            roomlistEntryGameobject.transform.SetParent(roomListParentGameobject.transform);
            roomlistEntryGameobject.transform.localScale = Vector3.one;
            roomlistEntryGameobject.transform.Find("RoomNameText").GetComponent<Text>().text = room.Name;
            roomlistEntryGameobject.transform.Find("RoomPlayersText").GetComponent<Text>().text = room.PlayerCount + "/" + room.MaxPlayers;
            roomlistEntryGameobject.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() => OnJoinRoomButtonClicked(room.Name));
            roomlistgameObject.Add(room.Name, roomlistEntryGameobject);
        }
    }
    public override void OnLeftLobby()
    {
        clearRoomListView();
        cacheRoomList.Clear();
        base.OnLeftLobby();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        string roomname = "Room" + Random.Range(1000, 10000);
        RoomOptions roomOptions=new RoomOptions();
        roomOptions.MaxPlayers = 20;
        PhotonNetwork.CreateRoom(roomname, roomOptions);
    }
    #endregion

    #region Private Methods
    void OnJoinRoomButtonClicked(string _roomName)
    {
        if(PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinRoom(_roomName);
    }
    void clearRoomListView()
    {
        foreach(var roomListGameobject in roomlistgameObject.Values)
        {
            Destroy(roomListGameobject);

        }
        roomlistgameObject.Clear();
    }

    #endregion

    #region Public Methods
    public void ActivatePanel(string panelTobeactiavated)
    {
        Login_UI_Panel.SetActive(panelTobeactiavated.Equals(Login_UI_Panel.name));
        gameOptions_UI_Panel.SetActive(panelTobeactiavated.Equals(gameOptions_UI_Panel.name));
        createRoom_UI_panel.SetActive(panelTobeactiavated.Equals(createRoom_UI_panel.name));
        insideRoom_UI_panel.SetActive(panelTobeactiavated.Equals(insideRoom_UI_panel.name));
        RoomList_UI_Panel.SetActive(panelTobeactiavated.Equals(RoomList_UI_Panel.name));
        JoinRandomRoom_UI_panel.SetActive(panelTobeactiavated.Equals(JoinRandomRoom_UI_panel.name));
        StartPanel.SetActive(panelTobeactiavated.Equals(StartPanel.name));

    }
    #endregion
}
