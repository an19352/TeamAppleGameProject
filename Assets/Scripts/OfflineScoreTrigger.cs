using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineScoreTrigger : MonoBehaviour
{
    // Score trigger for the tutorial
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OfflineMovement mov = other.GetComponent<OfflineMovement>();
            mov.Spawn();
        }
    }
}
