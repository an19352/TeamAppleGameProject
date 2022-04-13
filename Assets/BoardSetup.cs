using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSetup : MonoBehaviour
{
    public List<Transform> transforms;
    

    // Update is called once per frame
    public void Setup()
    {
        Vector3 helper;
        foreach(Transform trans in transform)
        {
            helper = trans.position;
            helper.x = transform.position.x - (trans.position.x - transform.position.x);
            trans.position = helper;
        }
    }
}
