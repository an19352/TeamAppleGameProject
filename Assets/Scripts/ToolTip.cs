using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTip : MonoBehaviour
{
    // OUT OF SERVICE
    public float timeToRead = 3f;
    float timeToDie;

    void Start()
    {
        timeToDie = Time.time + timeToRead;
    }

    void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
        transform.Rotate(new Vector3(0, 180, 0));
        if (Time.time > timeToDie)
            Destroy(gameObject);
    }
}
