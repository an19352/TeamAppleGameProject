using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zawarudo : MonoBehaviour
{
    public float slowScale;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Time.timeScale = slowScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }

    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Time.timeScale = 1;
            Time.fixedDeltaTime = Time.unscaledDeltaTime;
        }
    }
}
