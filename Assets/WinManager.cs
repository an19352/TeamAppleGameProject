using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinManager : MonoBehaviour
{
    public GameObject scoreRed;  
    public GameObject scoreGreen;  
    public GameObject scoreDraw;

    private void Start()
    {
        PhotonRoom room = PhotonRoom.room;
        if (room.greenScore == room.redScore) 
        {    
            scoreDraw.SetActive(true);
            return;
        }

        if (room.redScore > room.greenScore) scoreRed.SetActive(true);
        else scoreGreen.SetActive(true);
    }
}
