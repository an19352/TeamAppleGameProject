using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddConstantVelocity : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 force;
    // Update is called once per frame
    void FixedUpdate()
    {
        GetComponent<Rigidbody>().velocity += force;
    }
}
