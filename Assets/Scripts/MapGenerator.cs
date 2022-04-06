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
        public Platform(Vector3 position, Quaternion rotation, PlatformType type)
        {
            transform = Instantiate(type.prefab, position, rotation).transform;
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
        public TreeElement(Vector3 position, Quaternion rotation, PlatformType type, int _index, int _root = -1)
        {
            index = _index;
            root = _root;
            platform = new Platform(position, rotation, type);

            leafs = new List<int>();
        }
    }
    public List<TreeElement> tree = new List<TreeElement>();

    public Vector3 startingPosition; 
    public List<PlatformType> platformTypes;
    public List<PlatformType> specialPlatforms;

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
        tree.Add(new TreeElement(position, Quaternion.identity, chosen, 0));

        Transform tr = Instantiate(chosen.prefab, position, Quaternion.identity).transform;
        map[0, height / 2] = new Platform(tr, chosen);

        DrawLineOfPlatforms(tree[0], Vector3.right, width);
        DrawLineOfPlatforms(tree[width / 2], Vector3.forward, height / 2);
        DrawLineOfPlatforms(tree[width / 2], Vector3.back, height / 2);
        ReplacePlatform(width / 2, specialPlatforms[0]);

        DrawLineOfPlatforms(tree[1], Vector3.back, 3, Mathf.PI/36);
        DrawLineOfPlatforms(tree[tree.Count - 2], Vector3.back, 2, 11 * Mathf.PI / 6);
        ReplacePlatform(tree.Count - 1, specialPlatforms[1]);
        Transform settingup = tree[tree.Count - 1].platform.transform;
        settingup.gameObject.GetComponent<GeneratorBoardSetup>().Setup(new Vector3(-7f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 7f), Quaternion.identity);
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

    void ReplacePlatform(int originalIndex, PlatformType newPlatform)
    {
        TreeElement original = tree[originalIndex];
        tree[originalIndex] = new TreeElement(original.platform.transform.position, Quaternion.identity, newPlatform, originalIndex, original.root);
        Destroy(original.platform.transform.gameObject);
    }

    void DrawLineOfPlatforms(TreeElement from, Vector3 direction, int length = -1, float angleOverTime = -1f)
    {
        if (length == 0) return;
        if (length < 0) length = Mathf.Min(width, height);

        Vector3 position;
        PlatformType chosen;
        Platform previous = from.platform;
        int previousIndex = from.index;
        float angle = angleOverTime;

        for (; length > 0; length--)
        {
            if (angleOverTime > 0) 
            {
                direction = new Vector3(direction.x * Mathf.Cos(angle) - direction.z * Mathf.Sin(angle), 0f,
                                        direction.x * Mathf.Sin(angle) + direction.z * Mathf.Cos(angle));
                angle += angleOverTime;
            }

            position = previous.transform.position + direction.normalized * Random.Range(previous.reach * 0.75f, previous.reach);
            position.y += Random.Range(-previous.verticalReach, previous.verticalReach);
            chosen = platformTypes[Random.Range(0, platformTypes.Count)];

            tree[previousIndex].leafs.Add(tree.Count);
            tree.Add(new TreeElement(position, Quaternion.identity, chosen, tree.Count, previousIndex));

            previousIndex = tree.Count - 1;
            previous = tree[tree.Count - 1].platform;
        }
    }
}
