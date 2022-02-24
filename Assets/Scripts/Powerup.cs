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
    public static GameMechanics gameMechanics;

    public enum Effects { Orange, Blue, Purple, GravityGun, Grapple };
    public Effects _effect;
    string effect;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        gameMechanics = GameMechanics.gameMechanics;

        if (PV == null) Debug.LogWarning(this.name + " does not have a PV");
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

    private void OnDisable()
    {
        if (PV) if (PV.IsMine)
                PV.RPC("Disable", RpcTarget.All);
    }

    private void OnEnable()
    {
        if (PV) if (PV.IsMine)
                PV.RPC("NotifyMe", RpcTarget.All);
    }

    [PunRPC]
    public void Orange(int playerID)
    {
        return;
    }

    [PunRPC]
    public void Blue(int playerID)
    {
        return;
    }

    [PunRPC]
    public void Purple(int playerID)
    {
        return;
    }

    [PunRPC]
    public void GravityGun(int playerID)
    {
        GameObject playerObj = gameMechanics.players[playerID].obj;

        playerObj.GetComponent<GravityGun>().enabled = true;
        // player.GetComponent<Powerup>().enabled = true;

    }

    [PunRPC]
    public void Grapple(int playerID)
    {
        gameMechanics.players[playerID].obj.GetComponent<Grapple>().enabled = true;
    }

    [PunRPC]
    public void NotifyMe()
    {
        gameMechanics.activePowerups.Add(PV.ViewID, transform.position);
    }

    [PunRPC]
    public void Disable()
    {
        if (gameMechanics.activePowerups.ContainsKey(PV.ViewID))
            gameMechanics.activePowerups.Remove(PV.ViewID);
        transform.gameObject.SetActive(false);
    }
}
