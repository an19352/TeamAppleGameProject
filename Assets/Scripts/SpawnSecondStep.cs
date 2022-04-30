using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnSecondStep : MonoBehaviour
{
    [System.Serializable]
    public struct spawnable
    {
        public GameObject obj;
        public float chance;
    }

    public List<spawnable> spawns;


    public void SpawnObject()
    {
        PhotonNetwork.Instantiate(spawns[0].obj.name, transform.position + Vector3.zero, Quaternion.identity);
        return;
    }
}