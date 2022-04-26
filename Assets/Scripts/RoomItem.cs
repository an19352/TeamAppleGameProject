using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class RoomItem : MonoBehaviour
{
    public static PhotonLobby photonLobby;

    public TextMeshProUGUI roomName;

    public void Start()
    {
        photonLobby = FindObjectOfType<PhotonLobby>();
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
