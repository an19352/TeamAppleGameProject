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
        public Transform transform;
        public float reach;
        public float verticalReach;

        public Platform(Transform _transform, float _reach, float _verticalReach)
        {
            transform = _transform;
            reach = _reach;
            verticalReach = _verticalReach;
        }
        public Platform(Transform _transform, PlatformType type)
        {
            transform = _transform;
            reach = type.reach;
            verticalReach = type.verticalReach;
        }
    }

    public struct TreeElement
    {
        public int index, root;
        public List<int> leafs;
        public Platform platform;

        public TreeElement(Platform _platform, int _index, int _root = -1)
        {
            index = _index;
            root = _root;
            platform = _platform;

            leafs = new List<int>();
        }
        public TreeElement(Transform _transform, float _reach, float _verticalReach, int _index, int _root = -1)
        {
            index = _index;
            root = _root;
            platform = new Platform(_transform, _reach, _verticalReach);

            leafs = new List<int>();
        }
        public TreeElement(Transform _transform, PlatformType type, int _index, int _root = -1)
        {
            index = _index;
            root = _root;
            platform = new Platform(_transform, type);

            leafs = new List<int>();
        }
    }
    public List<TreeElement> tree = new List<TreeElement>();

    public Vector3 startingPosition; 
    public List<PlatformType> platformTypes;

    public int width = 15;
    public int height = 15;

    [Range(1, 4)]
    public int method = 3;
    Platform[,] map;

    void Start()
    {
        map = new Platform[width, height];

        Random.InitState(13);
        if (method == 1) first_method();
        else if (method == 2) second_method();
        else if (method == 3) third_method();
        else if (method == 4) fourth_method();
    }

    void first_method()
    {
        // Horiszontal line + trees
        return;
    }

    void second_method()
    {
        // Mirror method
        return;
    }

    void third_method()
    {
        // Quadtrant method

        Vector3 position;
        PlatformType chosen;

        position = new Vector3(0, 0, 0);
        chosen = platformTypes[Random.Range(0, platformTypes.Count - 1)];
        Transform tr = Instantiate(chosen.prefab, position, Quaternion.identity).transform;

        tree.Add(new TreeElement(new Platform(tr, chosen), 0));
        map[0, height / 2] = new Platform(tr, chosen);

        DrawLineOfPlatforms(tree[0], Vector3.right, width);
        DrawLineOfPlatforms(tree[width / 2], Vector3.forward, height / 2);
        DrawLineOfPlatforms(tree[width / 2], Vector3.back, height / 2);
    }

    void fourth_method()
    {
        // Grid strike out (graph theory)
        return;
    }
   
    int FindPlatform(Transform _transform)
    {
        foreach (TreeElement element in tree)
            if (element.platform.transform.Equals(_transform)) return element.index;

        return -1;
    }

    void DrawLineOfPlatforms(TreeElement from, Vector3 direction, int length = -1, int angleOverTime = -1)
    {
        if (length == 0) return;
        if (length < 0) length = Mathf.Min(width, height);

        Vector3 position;
        PlatformType chosen;
        Platform previous = from.platform;
        int previousIndex = from.index;
        Transform current;

        for (; length > 0; length--)
        {
            position = previous.transform.position + direction.normalized * Random.Range(previous.reach * 0.75f, previous.reach);
            position.y += Random.Range(-previous.verticalReach, previous.verticalReach);
            chosen = platformTypes[Random.Range(0, platformTypes.Count)];

            current = Instantiate(chosen.prefab, position, Quaternion.identity).transform;
            tree[previousIndex].leafs.Add(tree.Count);
            tree.Add(new TreeElement(current, chosen, tree.Count, previousIndex));

            previousIndex = tree.Count - 1;
            previous = tree[tree.Count - 1].platform;
        }
    }
}
