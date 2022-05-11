using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fixrotation : MonoBehaviour
{
    public Transform child;

    // Rotation Constraint component was not working so we created our own
    // This is currently used for the indicator arrow
    void Update()
    {
        child.transform.rotation = Quaternion.Euler (0.0f, 0.0f, gameObject.transform.rotation.z * -1.0f);
    }
    
}
