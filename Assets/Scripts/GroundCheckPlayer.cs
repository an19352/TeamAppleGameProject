using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheckPlayer : MonoBehaviour
{
    private bool offline = false;
    Movement player;
    private OfflineMovement player1;
    void Start()
    {
        if (transform.parent.TryGetComponent(out OfflineMovement off))
        {
            offline = true;
            player1 = GetComponentInParent<OfflineMovement>();
        }
        else
        {
            player = GetComponentInParent<Movement>();   
        }

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
        if (other.CompareTag("Ground"))
        {
            if(offline==true) player1.Ground(false);
            else player.Ground(false);
            //if (other.transform.parent == null) return;
            //if (other.transform.parent.gameObject.TryGetComponent(out Animator animator))
              //  animator.enabled = false;
        } 
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            if(offline==true) player1.Ground(true);
            else player.Ground(true);
            //if (other.transform.parent == null) return;
            //if (other.transform.parent.gameObject.TryGetComponent(out Animator animator))
              //  animator.enabled = true;
        }
    }
}
