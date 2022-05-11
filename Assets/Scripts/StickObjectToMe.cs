 using System.Collections;
 using System.Collections.Generic;
 using UnityEngine;
using Photon.Pun;

 public class StickObjectToMe : MonoBehaviour
 {
    // The moving platforms parents objects it touches to itself so they move with it. When they leave the platform
    // it reassigns them their previous parents
    // This essentially simulates initial frinction

     Dictionary<Transform, Transform> oldParentPhoneBook = new Dictionary<Transform, Transform>();
    PhotonView PV;

    private void Start()
    {
        PV = GetComponent<PhotonView>();
        if (PhotonNetwork.IsMasterClient) StartCoroutine(ActivateAnimator()); // Only master client applies the animation for better sync
    }


    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Undetectable") || collision.CompareTag("Detector") || collision.CompareTag("Detected"))
            return;

        Transform target = collision.transform;
        if (oldParentPhoneBook.ContainsKey(target)) return;

        if (GameMechanics.gameMechanics.GetLocalPlayer() == null) return;

        // When the local player arrives on it, the camera gets parented too, to eliminate gitteriness
        if (target.Equals(GameMechanics.gameMechanics.GetLocalPlayer().transform)) Camera.main.transform.SetParent(transform);

        oldParentPhoneBook.Add(target, target.parent);
        target.SetParent(transform);
    }

    private void OnTriggerExit(Collider collision)
     {
        if (collision.CompareTag("Undetectable") || collision.CompareTag("Detector") || collision.CompareTag("Detected"))
            return;

        Transform other = collision.transform;
         if (oldParentPhoneBook.ContainsKey(other))
         {
             other.SetParent(oldParentPhoneBook[other]);
             if (other.Equals(GameMechanics.gameMechanics.GetLocalPlayer().transform)) Camera.main.transform.SetParent(oldParentPhoneBook[other]);
             oldParentPhoneBook.Remove(other);
         }
     }

    IEnumerator ActivateAnimator()
    {
        yield return new WaitForSeconds(Random.Range(0.0f, 120.0f));

        GetComponent<Animator>().enabled = true;
    }
}
