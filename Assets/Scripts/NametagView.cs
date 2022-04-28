using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NametagView : MonoBehaviour
{
    private Transform mainCameratransform;

    // Start is called before the first frame update
    void Start()
    {
        mainCameratransform = Camera.main.transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(transform.position + mainCameratransform.rotation * Vector3.back,
            mainCameratransform.rotation * Vector3.up);
    }
}
