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
        public float chance;

        public void modifyChance(float newChance)
        {
            chance = newChance;
        }
    }

    public struct Platform
    {
        public PlatformType board;
        public Transform transform;
        public float reach;
        public float verticalReach;

        public Platform(Vector3 position, Quaternion rotation, PlatformType type)
        {
            transform = Instantiate(type.prefab, position, rotation).transform;
            reach = type.reach;
            verticalReach = type.verticalReach;
            board = type;
        }
    }

    public struct TreeElement
    {
        public int index, root;
        public List<int> leafs;
        public Platform platform;

        public TreeElement(Vector3 position, Quaternion rotation, PlatformType type, int _index, int _root = -1)
        {
            index = _index;
            root = _root;
            platform = new Platform(position, rotation, type);

            leafs = new List<int>();
        }
    }
    public List<TreeElement> tree = new List<TreeElement>();
    public List<TreeElement> mirrortree = new List<TreeElement>();

    public Vector3 startingPosition;
    public List<PlatformType> platformTypes;
    public List<PlatformType> specialPlatforms;
    float chanceSum = 0f;

    public int width = 15;
    public int height = 15;

    [Range(1, 4)]
    public int method = 3;
    Platform[,] map;

    public GameObject greenbase;
    public GameObject redbase;

    void Start()
    {
        map = new Platform[width, height];
        foreach (PlatformType plt in platformTypes) chanceSum += plt.chance;

        Random.InitState(13);
        if (method == 1) first_method();
        else if (method == 2) second_method();
        else if (method == 3) third_method();
        else if (method == 4) fourth_method();
    }

    void first_method()
    {
        // Horizontal line + trees
        return;
    }

    void second_method()
    {
        // Mirror method

        Vector3 position;
        PlatformType chosen;
        Vector3 rotation = new Vector3(0, 180, 0);
        position = new Vector3(0, 0, 0);
        chosen = platformTypes[Random.Range(0, platformTypes.Count - 1)];
        tree.Add(new TreeElement(position, Quaternion.identity, chosen, 0));

         DrawLineOfPlatforms(tree[0], Vector3.right, width);
         DrawLineOfPlatforms(tree[width / 2], Vector3.forward, height / 2);
         DrawLineOfPlatforms(tree[width / 2], Vector3.back, height / 2);
         ReplacePlatform(width / 2, specialPlatforms[0]);

         DrawLineOfPlatforms(tree[1], Vector3.back, 3, Mathf.PI/36);
         ReplacePlatform(tree.Count - 1, specialPlatforms[1]);

         for (int i = 1; i < tree.Count; i++)
         {
             position = tree[i].platform.transform.position;
             position.x = -(position.x);
             chosen = tree[i].platform.board;

            mirrortree.Add(new TreeElement(position, Quaternion.identity, chosen, tree[i].index, tree[i].root));
            Transform settingup = mirrortree[mirrortree.Count - 1].platform.transform;

            if (settingup.gameObject.TryGetComponent(out BoardSetup BS))
                BS.Setup();
         }

         SpawnBases();

    }

    void third_method()
    {
        // Quadrant method

        Vector3 position;
        PlatformType chosen;

        position = new Vector3(0, 0, 0);
        chosen = platformTypes[Random.Range(0, platformTypes.Count - 1)];
        tree.Add(new TreeElement(position, Quaternion.identity, chosen, 0));

        //Transform tr = Instantiate(chosen.prefab, position, Quaternion.identity).transform;
        //map[0, height / 2] = new Platform(tr, chosen);

        DrawLineOfPlatforms(tree[0], Vector3.right, width);
        DrawLineOfPlatforms(tree[width / 2], Vector3.forward, height / 2);
        DrawLineOfPlatforms(tree[width / 2], Vector3.back, height / 2);
        ReplacePlatform(width / 2, specialPlatforms[0]);

        DrawLineOfPlatforms(tree[1], Vector3.back, 3, Mathf.PI/36);
        DrawLineOfPlatforms(tree[tree.Count - 2], Vector3.back, 2, 11 * Mathf.PI / 6);
        ReplacePlatform(tree.Count - 1, specialPlatforms[1]);
        Transform settingup = tree[tree.Count - 1].platform.transform;
        settingup.gameObject.GetComponent<BoardSetup>().Setup();
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
        PlatformType chosen = platformTypes[0];
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
            float num = Random.Range(0f, 1f);
            for(int i = 0; i < platformTypes.Count; i++)
            {
                if (num > platformTypes[i].chance) num -= platformTypes[i].chance;
                else
                {
                    chosen = platformTypes[i];
                    break;
                }
            }


            tree[previousIndex].leafs.Add(tree.Count);
            tree.Add(new TreeElement(position, Quaternion.identity, chosen, tree.Count, previousIndex));

            previousIndex = tree.Count - 1;
            previous = tree[tree.Count - 1].platform;
        }
    }

    void SpawnBases()
    {
        Transform tr;
        Platform previous = tree[0].platform;
        Vector3 pos = previous.transform.position;
        float max = pos.x;
        for (int i = 1; i < tree.Count; i++)
        {
            float a = tree[i].platform.transform.position.x;
            if (a > max)
            {
                max = a;
                previous = tree[i].platform;
                pos = previous.transform.position;
            }
        }
        pos = pos + Vector3.right.normalized * Random.Range(previous.reach * 1.85f, 2.1f * previous.reach);
        pos.y += Random.Range(-previous.verticalReach, previous.verticalReach);

        tr = Instantiate(redbase, pos, Quaternion.identity).transform;
        tr.Rotate(new Vector3(0, 180, 0),Space.Self);

        pos.x = -(pos.x);
        Instantiate(greenbase, pos, Quaternion.identity);

    }
}
