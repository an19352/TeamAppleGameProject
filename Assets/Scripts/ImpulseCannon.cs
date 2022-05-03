using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ImpulseCannon : MonoBehaviour
{
    PhotonView PV;

    public float pushForce;
    private float distance;

    List<int> toBePushed;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        toBePushed = new List<int>();
        if (PV.IsMine)
        {
            transform.GetChild(2).gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire();
        }
    }

    public void Fire()
    {
        int[] pushNow = new int[toBePushed.Count];
        for (int i = 0; i < toBePushed.Count; i++)
        {
            pushNow[i] = toBePushed[i];
        }
        PV.RPC("RPC_Cannon", RpcTarget.All, pushNow, transform.forward * pushForce);
    }

    [PunRPC]
    void RPC_Cannon(int[] pushNow, Vector3 pushFactor)
    {
        for (int i = 0; i < pushNow.Length; i++)
        {
            GameObject _obj = PhotonView.Find(pushNow[i]).gameObject;
            distance = Vector3.Distance(transform.position, _obj.transform.position);
            _obj.GetComponent<Rigidbody>().AddForce(pushFactor * (1 / distance), ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Detector") && other.transform.parent != this.transform)
        {
            toBePushed.Add(other.gameObject.transform.parent.gameObject.GetComponent<PhotonView>().ViewID);
        }
        Debug.Log(toBePushed);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Detector") && other.transform.parent != this.transform)
        {
            toBePushed.Remove(other.gameObject.transform.parent.gameObject.GetComponent<PhotonView>().ViewID);
        }
    }
}
