using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ImpulseCannon : MonoBehaviour
{
    PhotonView PV;

    GameObject[] pushed;
    Transform parentPlayer;
    Rigidbody rigid;
    public float pushForce;
    private float distance;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            pushed = GameObject.FindGameObjectsWithTag("Detected");
            for (int i = 0; i < pushed.Length; i++)
            {
                PV.RPC("RPC_Cannon", RpcTarget.All, i);
            }
        }
    }
    
    [PunRPC]
    void RPC_Cannon(int i)
    {
        rigid = pushed[i].transform.parent.GetComponent<Rigidbody>();
        parentPlayer = transform.parent;
        distance = Vector3.Distance(parentPlayer.position, pushed[i].transform.position);
        rigid.AddForce(transform.forward * pushForce * (1 / distance), ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Detector"))
        {
            other.tag = "Detected";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Detected"))
        {
            other.tag = "Detector";
        }
    }
}
