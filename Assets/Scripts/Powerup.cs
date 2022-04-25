using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Powerup : MonoBehaviour//, IPunObservable
{
    PhotonView PV;
    public static PhotonRoom room;
    public static GameMechanics gameMechanics;
    [Tooltip("Check the Resources/Powerup Settings folder for a Inventory Element")]
    public InventoryElement powerup;
    public GameObject pickupEffect;

    public float maxDiscrepencyDistance = 3f;
    Rigidbody powerupRB;
    Vector3 networkPosition;
    Quaternion networkRotation;
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
        powerupRB = GetComponent<Rigidbody>();

        if (PV == null) Debug.LogWarning(this.name + " does not have a PV");
        room = PhotonRoom.room;

        //thisPowerup = transform.gameObject;
    }

/*    private void FixedUpdate()
    {
        if (Vector3.Distance(networkPosition, powerupRB.position) > maxDiscrepencyDistance)
            powerupRB.position = networkPosition;
        else powerupRB.position = Vector3.MoveTowards(powerupRB.position, networkPosition, Time.fixedDeltaTime * 100f);

        powerupRB.rotation = Quaternion.RotateTowards(powerupRB.rotation, networkRotation, Time.fixedDeltaTime * 100f);
    }*/

    //Makes powerup disappear when touched and writes to powerup text in UI
    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.CompareTag("Player"))
        {
            PV.RPC("ActivateItem", RpcTarget.All, other.collider.GetComponent<Movement>().GetId(), powerup.powerupName);
            PhotonNetwork.Instantiate(pickupEffect.name, transform.position, transform.rotation);
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

/*    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(gameObject.activeSelf);
            stream.SendNext(powerupRB.position);
            stream.SendNext(powerupRB.rotation);
            stream.SendNext(powerupRB.velocity);
        }
        if (stream.IsReading)
        {
            gameObject.SetActive((bool)stream.ReceiveNext());
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            powerupRB.velocity = (Vector3)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.timestamp));
            networkPosition += powerupRB.velocity * lag;
        }
    }*/
}
