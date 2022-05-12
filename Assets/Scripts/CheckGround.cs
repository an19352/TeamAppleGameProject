using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckGround : MonoBehaviour
{
    // This script stops objects from falling when they hit another object tagged as ground
    void OnTriggerEnter(Collider other)
    {
        //Debug.Log(other.gameObject.tag);
        if (other.gameObject.CompareTag("Ground"))
        {
            transform.parent.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}
