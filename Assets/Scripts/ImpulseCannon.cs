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

    List<int> toBePushed;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        toBePushed = new List<int>();
    }

    public void Fire()
    {
        int[] pushNow = new int[toBePushed.Count];
        //pushed = GameObject.FindGameObjectsWithTag("Detected");
        for (int i = 0; i < toBePushed.Count; i++)
        {
            pushNow[i] = toBePushed[i];
            //                PV.RPC("RPC_Cannon", RpcTarget.All, i);
        }
        PV.RPC("RPC_Cannon", RpcTarget.All, pushNow, transform.forward * pushForce);
    }
    
    [PunRPC]
    void RPC_Cannon(int[] pushNow, Vector3 pushFactor)
    {/*
        rigid = pushed[i].transform.parent.GetComponent<Rigidbody>();
        parentPlayer = transform.parent;
        distance = Vector3.Distance(parentPlayer.position, pushed[i].transform.position);
        rigid.AddForce(transform.forward * pushForce * (1 / distance), ForceMode.Impulse);
    */
    
        for (int i = 0; i < pushNow.Length; i++)
        {
            GameObject _obj = PhotonView.Find(pushNow[i]).gameObject;
            distance = Vector3.Distance(transform.parent.position, _obj.transform.position);
            _obj.GetComponent<Rigidbody>().AddForce(pushFactor * (1 / distance), ForceMode.Impulse);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Detector") && other.transform.parent != this.transform)
        {
            other.tag = "Detected";
            toBePushed.Add(other.gameObject.transform.parent.gameObject.GetComponent<PhotonView>().ViewID);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Detected") && other.transform.parent != this.transform)
        {
            other.tag = "Detector";
            toBePushed.Remove(other.gameObject.transform.parent.gameObject.GetComponent<PhotonView>().ViewID);
        }
    }
}
