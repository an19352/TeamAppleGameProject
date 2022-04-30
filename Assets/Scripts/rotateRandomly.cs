using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateRandomly : MonoBehaviour
{
    public float time;
    Quaternion random;
    float angle;

    private void Start()
    {
        random = Quaternion.Euler(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f));
    }

    void Update()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, random, time * Time.deltaTime);
        angle = Quaternion.Angle(transform.rotation, random);
        if (Quaternion.Angle(transform.rotation, random) < 50f)
            random = Quaternion.Euler(Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f), Random.Range(0.0f, 360.0f));
    }
}
