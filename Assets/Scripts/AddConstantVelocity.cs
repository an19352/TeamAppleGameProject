using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddConstantVelocity : MonoBehaviour
{
    // This script is used by an animator component to move the moving platforms

    public Vector3 force;
    void FixedUpdate()
    {
        GetComponent<Rigidbody>().velocity += force;
    }
}
