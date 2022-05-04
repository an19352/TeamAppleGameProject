using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListSpawningPoints : MonoBehaviour
{
    [SerializeField]
    public List<Transform> playerSpawningPoints;
    Queue<Transform> spawningPoints = new Queue<Transform>();

    private void Start()
    {
        foreach (Transform spawn in playerSpawningPoints) spawningPoints.Enqueue(spawn);
    }

    public Transform BookSpawn()
    {
        return spawningPoints.Dequeue();
    }
}
