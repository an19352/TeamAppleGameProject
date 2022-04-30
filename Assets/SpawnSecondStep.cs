using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        return;
    }
}
