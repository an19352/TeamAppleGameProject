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

    public enum Effects { Gravity_Gun, Grapple_Gun, Coin };
    public Effects _effect;
    string effect;
    /*
    Dictionary<string, int> itemsLookup = new Dictionary<string, int>()
    {
        { "Gravity Gun", 0},
        { "Grapple Gun", 1 },
        { "Coin", 3 }
    };*/

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        gameMechanics = GameMechanics.gameMechanics;

        if (PV == null) Debug.LogWarning(this.name + " does not have a PV");
        room = PhotonRoom.room;

        //thisPowerup = transform.gameObject;
        effect = _effect.ToString().Replace("_", " ");
        Debug.Log(effect);
    }

    //Makes powerup disappear when touched and writes to powerup text in UI
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PV.RPC("ActivateItem", RpcTarget.All, other.GetComponent<Movement>().GetId(), effect);
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
    public void ActivateItem(int playerID, string item)
    {
        gameMechanics.players[playerID].obj.GetComponent<Inventory>().activateItem(item);
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
    public void Coin(int playerID)
    {
        GameObject playerObj = gameMechanics.players[playerID].obj;
        Vector3 scaleChange = new Vector3(0.5f, 0.5f, 0.5f);
        playerObj.transform.localScale += scaleChange;
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
