using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    public static PhotonLobby lobby;

    public byte maxPlayers;
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI loading;

    public GameObject loadingCanvas;
    public GameObject lobbyCanvas;

    public RoomItem roomItemPrefab;
    List<RoomItem> roomItemsList = new List<RoomItem>();
    public Transform contentObj;

    public float timeBetweenUpdates = 1.5f;
    float nextUpdateTime;

    void Start()
    {
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
    }

    public void OnClickCreate()
    {
        if (playerName.text.Length > 1)
        {
            PhotonNetwork.NickName = playerName.text;
            PhotonNetwork.CreateRoom("Room of " + playerName.text, new Photon.Realtime.RoomOptions() { MaxPlayers = maxPlayers });
            
        }
    }

    public override void OnJoinedRoom()
    {
        SceneManager.LoadScene(1);
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
}
