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
    public float nothingChance = 0.3f;

    public void SpawnObject()
    {
        if (Random.Range(0.0f, 1.0f) <= nothingChance) return;

        float choice = Random.Range(0.0f, 1.0f);
        foreach (spawnable chosen in spawns)
            if (choice > chosen.chance) choice -= chosen.chance;
            else
            {
                PhotonNetwork.Instantiate(chosen.obj.name, transform.position + Vector3.up, Quaternion.identity);
                return;
            }
    }
}
