using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zawarudo : MonoBehaviour
{
    // Gives everyone bullet time... OUT OF SERVICE
    public float slowScale;

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
