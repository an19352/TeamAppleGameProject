using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheckPlayer : MonoBehaviour
{
    Movement player;

    void Start()
    {
        player = GetComponentInParent<Movement>();
    }
/*
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ground")
            player.Ground(true);
    }

*/
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Ground")
        {
            player.Ground(false);
            //if (other.transform.parent == null) return;
            //if (other.transform.parent.gameObject.TryGetComponent(out Animator animator))
              //  animator.enabled = false;
        } 
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Ground")
        {
            player.Ground(true);
            //if (other.transform.parent == null) return;
            //if (other.transform.parent.gameObject.TryGetComponent(out Animator animator))
              //  animator.enabled = true;
        }
    }
}
