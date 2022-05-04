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
    public GameObject Timeout;
    
    private void Start()
    { StartCoroutine(Wait()); }
    
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(2);
        Timeout.SetActive(false);
        PhotonRoom room = PhotonRoom.room;
        if (room.blueFlag == room.redFlag) 
        {    
            gameObject.transform.GetChild(0).GetChild(3).GetChild(0).GetChild(2).GetComponent<TMP_Text>().text =
                room.redFlag.ToString();
            gameObject.transform.GetChild(0).GetChild(3).GetChild(0).GetChild(4).GetComponent<TMP_Text>().text =
                room.blueFlag.ToString();
            gameObject.transform.GetChild(0).GetChild(3).GetChild(1).GetChild(2).GetComponent<TMP_Text>().text =
                room.redScore.ToString();
            gameObject.transform.GetChild(0).GetChild(3).GetChild(1).GetChild(4).GetComponent<TMP_Text>().text =
                room.blueScore.ToString();
            scoreDraw.SetActive(true);
        }
        if (room.redFlag > room.blueFlag)
        {
            gameObject.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(2).GetComponent<TMP_Text>().text =
                room.redFlag.ToString();
            gameObject.transform.GetChild(0).GetChild(1).GetChild(0).GetChild(4).GetComponent<TMP_Text>().text =
                room.blueFlag.ToString();
            gameObject.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(2).GetComponent<TMP_Text>().text =
                room.redScore.ToString();
            gameObject.transform.GetChild(0).GetChild(1).GetChild(1).GetChild(4).GetComponent<TMP_Text>().text =
                room.blueScore.ToString();
            RedWins.SetActive(true);
        }
        if (room.blueFlag > room.redFlag)
        {
            gameObject.transform.GetChild(0).GetChild(2).GetChild(0).GetChild(2).GetComponent<TMP_Text>().text =
                room.redFlag.ToString();
            gameObject.transform.GetChild(0).GetChild(2).GetChild(0).GetChild(4).GetComponent<TMP_Text>().text =
                room.blueFlag.ToString();
            gameObject.transform.GetChild(0).GetChild(2).GetChild(1).GetChild(2).GetComponent<TMP_Text>().text =
                room.redScore.ToString();
            gameObject.transform.GetChild(0).GetChild(2).GetChild(1).GetChild(4).GetComponent<TMP_Text>().text =
                room.blueScore.ToString();
            BlueWins.SetActive(true);
        }
    }
}
