using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Float : MonoBehaviour
{
    // We're in space! This script drive that point home by making stuff float

    public float floatStrength;
    public float randomRotationStrength;

    void Update()
    {
        transform.GetComponent<Rigidbody>().AddForce(Vector3.up * floatStrength);
        transform.Rotate(randomRotationStrength, randomRotationStrength, randomRotationStrength);
    }

}
