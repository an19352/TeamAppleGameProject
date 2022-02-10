using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoomItem : MonoBehaviour
{
    public static PhotonLobby photonLobby;

    public TextMeshProUGUI roomName;

    public void Start()
    {
        photonLobby = FindObjectOfType<PhotonLobby>();  
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
