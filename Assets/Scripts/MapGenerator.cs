using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MapGenerator : MonoBehaviour
{
    PhotonView PV;
    [HideInInspector]
    public int indicator = 0;

    [System.Serializable]
    public struct PlatformType
    {
        public GameObject prefab;
        public float reach;
        public float verticalReach;
        public float chance;

        public void modifyChance(float newChance)
        {
            Debug.LogWarning("Chance modified for " + prefab.name + " from " + chance.ToString() + " to " + newChance.ToString());
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
    public int ones = 0;
    public int zeros = 0;
    public int difference = 0;

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
    List<BoardSetup.PhotonSpawnable> photonSpawnables = new List<BoardSetup.PhotonSpawnable>();

    public Vector3 startingPosition;
    public List<PlatformType> platformTypes;
    public List<PlatformType> specialPlatforms;
    //float chanceSum = 0f;

    public int width = 15;
    public int height = 15;

    [Range(1, 4)]
    public int method = 3;
    //Platform[,] map;

    public GameObject greenbase;
    public GameObject redbase;
    public GameObject objectivePrefab; 
    [SerializeField]

    public int InitState = 13;
    public bool Generate_Again = false;
    int[] PVIDs = new int[2];

/*    public void OnValidate()
    {
        if (Generate_Again)
        {

            foreach (TreeElement TE in tree)
            {
                Destroy(TE.platform.transform.gameObject);
            }
            tree = new List<TreeElement>();

            //Random.InitState(InitState);
            if (method == 1) first_method();
            else if (method == 2) second_method();
            else if (method == 3) third_method();
            else if (method == 4) fourth_method();

            Generate_Again = false;
        }
    }*/

    void Start()
    {
        //map = new Platform[width, height];
        //foreach (PlatformType plt in platformTypes) chanceSum += plt.chance;

        PV = GetComponent<PhotonView>();
        PV.RPC("Awaken", RpcTarget.MasterClient);

/*        //Random.InitState(InitState);
        if (Random.Range(0.0f, 1.0f) > 0.5f) second_method();
        else third_method();*/
    }

    [PunRPC]
    void Awaken()
    {
        indicator++;

        if (indicator < PhotonNetwork.PlayerList.Length) return;

        indicator = 0;
        PV.RPC("firstStepGeneration", RpcTarget.All, (int)Random.Range(0, 1000));
    }

    [PunRPC]
    void firstStepGeneration(int seed)
    {
        Random.InitState(seed);

        if (Random.Range(0.0f, 1.0f) > 0.5f) second_method();
        else third_method();

        PV.RPC("SignalMaster", RpcTarget.MasterClient);
    }

    [PunRPC]
    void SignalMaster()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        indicator++;

        if (indicator >= PhotonNetwork.PlayerList.Length) secondStepGeneration();
    }

    void secondStepGeneration()
    {
        Debug.Log("Doing step two or whatever");

        foreach (BoardSetup.PhotonSpawnable spawnable in photonSpawnables)
            PhotonNetwork.Instantiate(spawnable.prefab, spawnable.position, spawnable.rotation);

        foreach (TreeElement TE in tree)
            if (TE.platform.transform.gameObject.TryGetComponent(out SpawnSecondStep SSS))
                SSS.SpawnObject();

        GameMechanics.gameMechanics.RPC_InitiatePlayer();
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
         photonSpawnables.AddRange(tree[tree.Count - 1].platform.transform.GetComponent<BoardSetup>().Setup());

         for (int i = 1; i < tree.Count; i++)
         {
             position = tree[i].platform.transform.position;
             position.x = -(position.x);
             chosen = tree[i].platform.board;

            mirrortree.Add(new TreeElement(position, Quaternion.identity, chosen, tree[i].index, tree[i].root));
            Transform settingup = mirrortree[mirrortree.Count - 1].platform.transform;

            if (settingup.gameObject.TryGetComponent(out BoardSetup BS))
                photonSpawnables.AddRange(BS.Setup());
         }

         SpawnBases();

    }

    void third_method()
    {
        // Quadrant method

        Vector3 position;
        PlatformType chosen;
        //Transform settingup;
        int top, bottom;

        position = new Vector3(0, 0, 0);
        chosen = platformTypes[Random.Range(0, platformTypes.Count - 1)];
        tree.Add(new TreeElement(position, Quaternion.identity, chosen, 0));

        //Transform tr = Instantiate(chosen.prefab, position, Quaternion.identity).transform;
        //map[0, height / 2] = new Platform(tr, chosen);

        DrawLineOfPlatforms(tree[0], Vector3.right, width - 1); // Draw a line from left to right


        position = position + Vector3.left * 100f;
        GameObject blueBase = Instantiate(greenbase, position, Quaternion.identity);               // Add Gree... I mean Blue Base
        position = tree[width - 1].platform.transform.position + Vector3.right * 100f;
        //GameMechanics.gameMechanics.shields.Add(
        GameObject redBase = Instantiate(redbase, position, Quaternion.identity);
        redBase.transform.Rotate(new Vector3(0, 180, 0), Space.Self);   // Add Red Base

        GameMechanics.gameMechanics.bases.Add(redBase.gameObject);
        GameMechanics.gameMechanics.bases.Add(blueBase.gameObject);
        GameMechanics.gameMechanics.flagObjectives = new GameMechanics.FlagObjective[2];
        GameMechanics.gameMechanics.flagObjectives[1] = new GameMechanics.FlagObjective(blueBase.transform.GetChild(2).gameObject);
        GameMechanics.gameMechanics.flagObjectives[0] = new GameMechanics.FlagObjective(redBase.transform.GetChild(2).gameObject);

        DrawLineOfPlatforms(tree[width / 2], Vector3.forward, height / 2); // Draw a line up
        top = tree.Count - 1;
        DrawLineOfPlatforms(tree[width / 2], Vector3.back, height / 2);    // Draw a line down
        bottom = tree.Count - 1;
        ReplacePlatform(width / 2, specialPlatforms[2]);                   // Mark middle

        List<System.IFormattable[]> possibilities = new List<System.IFormattable[]>();

        for (int i = 0; i < width / 6; i++)
        {
            possibilities.Add(new System.IFormattable[] { Vector3.back, 0, bottom, i * 3 });
            possibilities.Add(new System.IFormattable[] { Vector3.forward, 340, top, i * 3 });
            possibilities.Add(new System.IFormattable[] { Vector3.back, 350, bottom, width - 2 - i * 3 });
            possibilities.Add(new System.IFormattable[] { Vector3.forward, 0, top, width - 2 - i * 3 });
        }

        foreach (System.IFormattable[] combination in possibilities)
        {
            Vector3 towardsBase = Vector3.left;
            if ((int)combination[3] > width/2) towardsBase = Vector3.right;
            
            DrawLineOfPlatforms(tree[Random.Range((int)combination[3], (int)combination[3] + 1)], (Vector3)combination[0], 3, (Mathf.PI / 180) * (Random.Range(5, 10) + (int)combination[1]));
            while (Mathf.Abs(tree[tree.Count - 1].platform.transform.position.z) - Mathf.Abs(tree[(int)combination[2]].platform.transform.position.z) < -30f)
            {
                DrawLineOfPlatforms(tree[tree.Count - 2], towardsBase, 2, (Mathf.PI / 180) * (5 + (int)combination[1]));
                DrawLineOfPlatforms(tree[tree.Count - 2], (Vector3)combination[0], 3, (Mathf.PI / 180) * (Random.Range(5, 10) + (int)combination[1]));
            }
            ReplacePlatform(tree.Count - 2, specialPlatforms[0]);
            DrawLineOfPlatforms(tree[tree.Count - 2], towardsBase, 2, (Mathf.PI / 180) * (5 + (int)combination[1]));

            if (possibilities.IndexOf(combination) < 4)
            {
                ReplacePlatform(tree.Count - 1, specialPlatforms[1]);
                photonSpawnables.AddRange(tree[tree.Count - 1].platform.transform.gameObject.GetComponent<BoardSetup>().Setup());
            }
        }

        ReplacePlatform(0, specialPlatforms[0]);
        ReplacePlatform(width - 1, specialPlatforms[0]);

        DrawLineOfPlatforms(tree[width / 2], Vector3.left, 2);
        ReplacePlatform(tree.Count - 1, specialPlatforms[1]);
        photonSpawnables.AddRange(tree[tree.Count - 1].platform.transform.gameObject.GetComponent<BoardSetup>().Setup()); 
        
        DrawLineOfPlatforms(tree[width / 2], Vector3.right, 2);
        ReplacePlatform(tree.Count - 1, specialPlatforms[1]);
        photonSpawnables.AddRange(tree[tree.Count - 1].platform.transform.gameObject.GetComponent<BoardSetup>().Setup());

        if (PhotonNetwork.IsMasterClient)
        {
            PVIDs[0] = PhotonNetwork.Instantiate(objectivePrefab.name, redBase.transform.GetChild(2).position, redBase.transform.GetChild(2).rotation).GetComponent<PhotonView>().ViewID;
            PVIDs[1] = PhotonNetwork.Instantiate(objectivePrefab.name, blueBase.transform.GetChild(2).position, blueBase.transform.GetChild(2).rotation).GetComponent<PhotonView>().ViewID;

            PV.RPC("flagObjectiveSyc", RpcTarget.All, PVIDs);
        }
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
            if (previous.verticalReach > 4f) position.y += Random.Range(3 * previous.verticalReach / 2, previous.verticalReach);
            else position.y += Random.Range(-previous.verticalReach, previous.verticalReach);   
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

        GameMechanics.gameMechanics.flagObjectives = new GameMechanics.FlagObjective[2];

        tr = Instantiate(redbase, pos, Quaternion.identity).transform;
        tr.Rotate(new Vector3(0, 180, 0), Space.Self);
        GameMechanics.gameMechanics.bases.Add(tr.gameObject);
        if (PhotonNetwork.IsMasterClient)
            PVIDs[0] = PhotonNetwork.Instantiate(objectivePrefab.name, tr.GetChild(2).position, tr.GetChild(2).rotation).GetComponent<PhotonView>().ViewID;

        pos.x = -(pos.x);
        tr = Instantiate(greenbase, pos, Quaternion.identity).transform;
        GameMechanics.gameMechanics.bases.Add(tr.gameObject);

        if (PhotonNetwork.IsMasterClient)
        {
            PVIDs[1] = PhotonNetwork.Instantiate(objectivePrefab.name, tr.GetChild(2).position, tr.GetChild(2).rotation).GetComponent<PhotonView>().ViewID;

            PV.RPC("flagObjectiveSyc", RpcTarget.All, PVIDs);
        }
    }

    [PunRPC]
    void flagObjectiveSyc(int[] PVIDs)
    {
        Debug.Log(PhotonView.Find(PVIDs[0]).gameObject);
        Debug.Log(PhotonView.Find(PVIDs[1]).gameObject);
        GameMechanics.gameMechanics.flagObjectives[0] = new GameMechanics.FlagObjective(PhotonView.Find(PVIDs[0]).gameObject);
        GameMechanics.gameMechanics.flagObjectives[1] = new GameMechanics.FlagObjective(PhotonView.Find(PVIDs[1]).gameObject);
    }
}
