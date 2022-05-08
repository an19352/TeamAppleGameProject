using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Float : MonoBehaviour
{
    // Start is called before the first frame update
    public float floatStrength;
    public float randomRotationStrength;

    void Update()
    {
        transform.GetComponent<Rigidbody>().AddForce(Vector3.up * floatStrength);
        transform.Rotate(randomRotationStrength, randomRotationStrength, randomRotationStrength);
    }

}
