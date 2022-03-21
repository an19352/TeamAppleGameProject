using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepelField : MonoBehaviour
{
    public float force;
    void OnTriggerEnter(Collider other)
    {
        // apply the force in the opposite direction that the player went in
        Vector3 forceDir = -(transform.position - other.transform.position);
        Rigidbody playerBody = other.attachedRigidbody;
        playerBody.AddForce(forceDir * force);
    }
}
