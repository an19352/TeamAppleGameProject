using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class RoomItem : MonoBehaviour
{
    // The prefab for the room buttons in the room selection screen
    public static PhotonLobby photonLobby;

    public TextMeshProUGUI roomName;

    public void Start()
    {
        photonLobby = FindObjectOfType<PhotonLobby>();
    }

    private void OnEnable()
    {
        if (PhotonNetwork.CurrentRoom != null)
            roomName.text = PhotonNetwork.CurrentRoom.Name;
    }

    public void SetRoomName(string _roomName)
    {
        roomName.text = _roomName;
    }

    public void OnClickItem()
    {
        photonLobby.JoinRoom(roomName.text);
    }
}
