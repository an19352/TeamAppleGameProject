using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    public static PhotonLobby lobby;

    public byte maxPlayers;
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI loading;
    public Button joinButton;

    public GameObject loadingCanvas;
    public GameObject lobbyCanvas;
    public GameObject roomCanvas;

    public RoomItem roomItemPrefab;
    List<RoomItem> roomItemsList = new List<RoomItem>();
    public Transform contentObj;
    public GameObject Nickname;
    public teamSelectionScreenManager TSSM;

    public float timeBetweenUpdates = 1.5f;
    float nextUpdateTime;
    int selectedRoom = -1;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();


        loadingCanvas.SetActive(false);
        lobbyCanvas.SetActive(true);
        roomCanvas.SetActive(false);
        playerName.transform.parent.parent.gameObject.GetComponent<TMP_InputField>().Select();
    }

    public void OnClickCreate()
    {
        if (playerName.text.Length > 1)
        {
            PhotonNetwork.NickName = playerName.text;
            PhotonNetwork.CreateRoom("Room of " + playerName.text, new Photon.Realtime.RoomOptions() { MaxPlayers = maxPlayers });
        }
    }

    public void ClickThatButton(Button _button)
    {
        _button.Select();
    }

    public override void OnJoinedRoom()
    {
        lobbyCanvas.SetActive(false);
        roomCanvas.SetActive(true);
        base.OnJoinedRoom();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateRoomList(roomList);
            nextUpdateTime = Time.time + timeBetweenUpdates;
        }
        base.OnRoomListUpdate(roomList);
    }

    void UpdateRoomList(List<RoomInfo> roomList)
    {
        foreach(RoomItem item in roomItemsList)
        {
            Destroy(item.gameObject);
        }
        roomItemsList.Clear();

        foreach(RoomInfo roomInfo in roomList)
        {
            RoomItem newRoom = Instantiate(roomItemPrefab, contentObj);
            newRoom.SetRoomName(roomInfo.Name);
            roomItemsList.Add(newRoom);
        }
        SelectRoom();
    }

    void SelectRoom()
    {
        if (selectedRoom > -1)
            roomItemsList[selectedRoom].gameObject.GetComponent<Button>().Select();
    }

    public void JoinRoom(string roomName)
    {
        if (playerName.text.Length > 1)
        {
            PhotonNetwork.NickName = playerName.text;
            PhotonNetwork.JoinRoom(roomName);
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null) return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if(EventSystem.current.currentSelectedGameObject.TryGetComponent(out Button _button))
                ClickThatButton(_button);
            else
                ClickThatButton(joinButton);
        }
        
        if (roomItemsList.Count < 1) return;

        if (Input.GetKeyDown(KeyCode.RightArrow) && joinButton.gameObject == EventSystem.current.currentSelectedGameObject)
        {
            selectedRoom = 0;
            SelectRoom();
            joinButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Join";
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) && joinButton.gameObject != EventSystem.current.currentSelectedGameObject)
        {
            selectedRoom = -1;
            ClickThatButton(joinButton);
            joinButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Create";
        }

        if(Input.GetKeyDown(KeyCode.UpArrow) && selectedRoom > 0)
        {
            selectedRoom--;
            SelectRoom();
        }

        if(Input.GetKeyDown(KeyCode.DownArrow) && selectedRoom < roomItemsList.Count - 1)
        {
            selectedRoom++;
            SelectRoom();
        }
    
    }
}
