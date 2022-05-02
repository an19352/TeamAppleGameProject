using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class WinManager : MonoBehaviour
{
    public GameObject RedWins;  
    public GameObject BlueWins;  
    public GameObject scoreDraw;

    private void Start()
    {
        PhotonRoom room = PhotonRoom.room;
        gameObject.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(2).GetComponent<TMP_Text>().text =
            room.redScore.ToString();
        gameObject.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(4).GetComponent<TMP_Text>().text =
            room.blueScore.ToString();
        gameObject.transform.GetChild(0).GetChild(2).GetChild(1).GetChild(2).GetComponent<TMP_Text>().text =
            room.redScore.ToString();
        gameObject.transform.GetChild(0).GetChild(2).GetChild(1).GetChild(4).GetComponent<TMP_Text>().text =
            room.blueScore.ToString();
        gameObject.transform.GetChild(0).GetChild(3).GetChild(1).GetChild(2).GetComponent<TMP_Text>().text =
            room.redScore.ToString();
        gameObject.transform.GetChild(0).GetChild(3).GetChild(1).GetChild(4).GetComponent<TMP_Text>().text =
            room.blueScore.ToString();
        
        gameObject.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(2).GetComponent<TMP_Text>().text =
            room.redFlag.ToString();
        gameObject.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(4).GetComponent<TMP_Text>().text =
            room.blueFlag.ToString();
        gameObject.transform.GetChild(0).GetChild(2).GetChild(0).GetChild(2).GetComponent<TMP_Text>().text =
            room.redFlag.ToString();
        gameObject.transform.GetChild(0).GetChild(2).GetChild(0).GetChild(4).GetComponent<TMP_Text>().text =
            room.blueFlag.ToString();
        gameObject.transform.GetChild(0).GetChild(3).GetChild(0).GetChild(2).GetComponent<TMP_Text>().text =
            room.redFlag.ToString();
        gameObject.transform.GetChild(0).GetChild(3).GetChild(0).GetChild(4).GetComponent<TMP_Text>().text =
            room.blueFlag.ToString();
        
        if (room.blueFlag == room.redFlag) 
        {    
            scoreDraw.SetActive(true);
            return;
        }

        if (room.redFlag > room.blueFlag) RedWins.SetActive(true);
        else BlueWins.SetActive(true);
    }
}
