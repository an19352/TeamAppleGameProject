using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [System.Serializable]
    public struct PlatformType
    {
        public GameObject prefab;
        public float reach;
        public float verticalReach;
    }

    public struct Platform
    {
        public Vector3 position;
        public float reach;
        public float verticalReach;

        public Platform(Vector3 _position, float _reach, float _verticalReach)
        {
            position = _position;
            reach = _reach;
            verticalReach = _verticalReach;
        }
    }

    public Vector3 startingPosition; 
    public List<PlatformType> platformTypes;

    public int width = 15;
    public int height = 15;
    Platform[,] map;

    void Start()
    {
        map = new Platform[width, height];

        Random.InitState(13);

        Vector3 position;
        PlatformType chosen;
        Platform previous;

        position = new Vector3(0, 0, 0);
        chosen = platformTypes[Random.Range(0, platformTypes.Count - 1)];
        Instantiate(chosen.prefab, position, Quaternion.identity);
        map[0, height / 2] = new Platform(position, chosen.reach, chosen.verticalReach);

        for (int i = 1; i < width; i++)
        {
            previous = map[i - 1, height / 2];
            position = map[i - 1, height / 2].position + new Vector3(Random.Range(previous.reach * 0.75f, previous.reach), Random.Range(-previous.verticalReach, previous.verticalReach), 0);
            chosen = platformTypes[Random.Range(0, platformTypes.Count)];
            Instantiate(chosen.prefab, position, Quaternion.identity);
            map[i, height / 2] = new Platform(position, chosen.reach, chosen.verticalReach);
        }
    }
}
