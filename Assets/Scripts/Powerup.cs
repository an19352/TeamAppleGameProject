using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Powerup : MonoBehaviour
{
    PhotonView PV;
    public static PhotonRoom room;
    //GameObject thisPowerup;

    public enum Effects{Orange,Blue,Purple};
    public Effects _effect;
    string effect;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        room = PhotonRoom.room;

        //thisPowerup = transform.gameObject;
        effect = _effect.ToString();
    }

    //Makes powerup disappear when touched and writes to powerup text in UI
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PV.RPC(effect, RpcTarget.All, other.GetComponent<Movement>().GetId());
            PV.RPC("Disable", RpcTarget.All, null);
        }
    }

    [PunRPC]
    public void Orange(int playerID)
    {
        Debug.Log(playerID.ToString());
    }

    [PunRPC]
    public void Blue(int playerID) 
    {
        Debug.Log(playerID.ToString());
    }

    [PunRPC]
    public void Purple(int playerID) 
    {
        Debug.Log(playerID.ToString());
    }

    [PunRPC]
    public void Disable()
    {
        transform.gameObject.SetActive(false);
    }
}
