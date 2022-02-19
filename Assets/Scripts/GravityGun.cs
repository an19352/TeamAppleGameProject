using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityGun : MonoBehaviour
{

    [SerializeField] Camera cameraMain;
    [SerializeField] float maxGrabDistance = 3f, throwForce = 20f, lerpSpeed = 10f;
    [SerializeField] Transform objectHolder;
    Rigidbody grabbedRB;


    // Start is called before the first frame update
    void Start()
    {
        cameraMain = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {
        if (grabbedRB)
        {
            grabbedRB.position = objectHolder.transform.position;
            if (Input.GetKeyDown(KeyCode.F))
            {
                grabbedRB.isKinematic = false;
                grabbedRB.AddForce(objectHolder.transform.forward * throwForce, ForceMode.VelocityChange);
                grabbedRB = null;

            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (grabbedRB)
            {
                grabbedRB.isKinematic = false;
                grabbedRB = null;
            }
            else
            {
                RaycastHit hit;
                Ray mouseRay = cameraMain.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(mouseRay, out hit))
                {
                    GameObject objectHit = hit.collider.gameObject;
                    float dist = Vector3.Distance(objectHolder.position, objectHit.transform.position);
                    // make sure to only grab objects that are tagged and close to player 
                    if (hit.transform.CompareTag("Grabbable") && dist <= maxGrabDistance)
                    {
                        grabbedRB = objectHit.GetComponent<Rigidbody>();
                        grabbedRB.isKinematic = true;
                    }
                }
            }
        }

    }
}
