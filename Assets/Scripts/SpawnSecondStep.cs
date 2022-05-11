using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnSecondStep : MonoBehaviour
{
    // Sits on boards that can spawn fun elements. Each board has a number of slots available and each element requiers a few slots,
    // in the future a new team may make these into scriptable objects and play around even more, adding diversity

    [System.Serializable]
    public struct spawnable
    {
        public GameObject obj;
        public int slotRequierment;  
        public float chance;
    }

    public List<spawnable> spawns;
    public float nothingChance = 0.3f;

    public int availableSlots = 3;
    public List<Vector3> slotPositions;

    // returns this list of what it spawned where so the map generation can mirror it
    public List<(string, Vector3)> SpawnObject()
    {
        if (Random.Range(0.0f, 1.0f) <= nothingChance) return new List<(string, Vector3)>();

        List<(string, Vector3)> result = new List<(string, Vector3)>();

        while (availableSlots > 0)
        {
            float choice = Random.Range(0.0f, 1.0f);
            foreach (spawnable chosen in spawns)
                if (choice > chosen.chance) choice -= chosen.chance;
                else
                if (chosen.slotRequierment <= availableSlots)
                {
                    availableSlots -= chosen.slotRequierment;
                    PhotonNetwork.Instantiate(chosen.obj.name, transform.position + slotPositions[availableSlots], Quaternion.identity);
                    result.Add((chosen.obj.name, slotPositions[availableSlots]));
                    break;
                }
                else choice = 0f;
        }
        return result;
    }

    // allows to spawn in certain items
    public void SpawnObject(List<(string, Vector3)> tupleList)
    {
        if (tupleList == null) return;
        if (tupleList.Count < 1) return;

        foreach ((string, Vector3) tuple in tupleList)
            PhotonNetwork.Instantiate(tuple.Item1, transform.position - tuple.Item2 + Vector3.up * 2 * tuple.Item2.y, Quaternion.identity);
    }
}
