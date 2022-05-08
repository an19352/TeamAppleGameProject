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
            if (offline == true)
            {
                Debug.Log("1");
                player1.Ground(false);
            }
            else
            {
                //Debug.Log("2");
                player.Ground(false);
            }
            //if (other.transform.parent == null) return;
            //if (other.transform.parent.gameObject.TryGetComponent(out Animator animator))
              //  animator.enabled = false;
        }

        else if (other.CompareTag("FractureBoard"))
        {
            if (offline == true)
            {
                Debug.Log("3");
                player1.Fracture(false);
            }
            else
            {
                //Debug.Log("4");
                player.Fracture(false);
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("FractureBoard"))
        {
            if (offline == true)
            {
                Debug.Log("5");
                player1.Fracture(true);
            }
            else
            {
                //Debug.Log("6");
                player.Fracture(true);
            }
            //if (other.transform.parent == null) return;
            //if (other.transform.parent.gameObject.TryGetComponent(out Animator animator))
              //  animator.enabled = true;
        }
        else if (other.CompareTag("Ground"))
        {
            if (offline == true)
            {
                Debug.Log("7");
                player1.Ground(true);
            }
            else
            {
                //Debug.Log("8");
                player.Ground(true);
            }
        }
    }
}
