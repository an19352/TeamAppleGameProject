 using System.Collections;
 using System.Collections.Generic;
 using UnityEngine;
using Photon.Pun;

 public class StickObjectToMe : MonoBehaviour
 {
     Dictionary<Transform, Transform> oldParentPhoneBook = new Dictionary<Transform, Transform>();
    PhotonView PV;

    private void Start()
    {
        PV = GetComponent<PhotonView>();
        if (PhotonNetwork.IsMasterClient) GetComponent<Animator>().enabled = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Transform target = collision.collider.transform;
        if (oldParentPhoneBook.ContainsKey(target)) return;

        if (target.Equals(GameMechanics.gameMechanics.GetLocalPlayer().transform)) Camera.main.transform.SetParent(transform);

        oldParentPhoneBook.Add(target, target.parent);
        target.SetParent(transform);
    }

    private void OnCollisionExit(Collision collision)
     {
         Transform other = collision.collider.transform;
         if (oldParentPhoneBook.ContainsKey(other))
         {
             other.SetParent(oldParentPhoneBook[other]);
             if (other.Equals(GameMechanics.gameMechanics.GetLocalPlayer().transform)) Camera.main.transform.SetParent(oldParentPhoneBook[other]);
             oldParentPhoneBook.Remove(other);
         }
     }
 }
