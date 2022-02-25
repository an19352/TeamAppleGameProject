using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GravityGun : MonoBehaviour
{
    PhotonView PV;

    [SerializeField] Camera cameraMain;
    [SerializeField] float maxGrabDistance = 3f, throwForce = 20f, lerpSpeed = 10f;
    [SerializeField] Transform objectHolder;
    Rigidbody grabbedRB;
    int grabbedID;


    // Start is called before the first frame update
    void Start()
    {
        cameraMain = Camera.main;
        PV = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine) return;

        if (grabbedRB)
        {
            grabbedID = grabbedRB.gameObject.GetComponent<PhotonView>().ViewID;
            PV.RPC("ParentObject", RpcTarget.All, grabbedID);
            //grabbedRB.transform.position = objectHolder.transform.position;
            if (Input.GetKeyDown(KeyCode.F))
            {
                PV.RPC("ChangeKinematic", RpcTarget.All, grabbedID, false);
                PV.RPC("AddSomeForce", RpcTarget.All, grabbedID, objectHolder.transform.forward * throwForce, ForceMode.VelocityChange);
                //grabbedRB.AddForce(objectHolder.transform.forward * throwForce, ForceMode.VelocityChange);
                PV.RPC("ReleaseChildren", RpcTarget.All, grabbedID);
                grabbedRB = null;
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (grabbedRB)
            {
                PV.RPC("ChangeKinematic", RpcTarget.All, grabbedID, false);
                PV.RPC("ReleaseChildren", RpcTarget.All, grabbedID);
                grabbedRB = null;
            }
            else
            {
                RaycastHit hit;
                Ray mouseRay = cameraMain.ScreenPointToRay(Input.mousePosition);

                int lm = LayerMask.GetMask("Grabbable");

                if (Physics.Raycast(mouseRay, out hit, 1000f, lm))
                {
                    GameObject objectHit = hit.collider.gameObject;
                    float dist = Vector3.Distance(objectHolder.position, objectHit.transform.position);
                    // make sure to only grab objects that are within a certain distance to the player 
                    if (dist <= maxGrabDistance)
                    {
                        grabbedRB = objectHit.GetComponent<Rigidbody>();
                        grabbedID = grabbedRB.gameObject.GetComponent<PhotonView>().ViewID;
                        PV.RPC("ChangeKinematic", RpcTarget.All, grabbedID, true);
                    }
                }
            }
        }

    }

    [PunRPC]
    void ChangeKinematic(int PVID, bool value)
    {
        Rigidbody rb = PhotonView.Find(PVID).gameObject.GetComponent<Rigidbody>();
        rb.isKinematic = value;
    }

    [PunRPC]
    void ParentObject(int PVID)
    {
        GameObject _obj = PhotonView.Find(PVID).gameObject;
        _obj.transform.SetParent(transform);
    }

    [PunRPC]
    void ReleaseChildren(int PVID)
    {
        GameObject _obj = PhotonView.Find(PVID).gameObject;
        _obj.transform.parent = null;
    }

    [PunRPC]
    void AddSomeForce(int PVID, Vector3 force, ForceMode mode)
    {
        Rigidbody _rb = PhotonView.Find(PVID).gameObject.GetComponent<Rigidbody>();
        _rb.AddForce(force, mode);
    }
}
