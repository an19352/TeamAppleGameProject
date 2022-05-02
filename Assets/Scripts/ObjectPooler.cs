using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public static ObjectPooler OP;

    #region Singleton

    private void Awake()
    {
        if (ObjectPooler.OP == null) ObjectPooler.OP = this;
        else if (ObjectPooler.OP != this)
        {
            Destroy(ObjectPooler.OP.gameObject);
            ObjectPooler.OP = this;
        }
    }

    #endregion

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    GameObject obj;
    PhotonView PV;
    bool synced;

    private void Start()
    {
        synced = false;
        PV = GetComponent<PhotonView>();
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach(Pool pool in pools)
        {
            if (!pool.prefab.GetComponent<PhotonView>())
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();
                for (int i = 0; i < pool.size; i++)
                {
                    obj = Instantiate(pool.prefab);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }
                poolDictionary.Add(pool.tag, objectPool);
            }
        }

/*        if (!PhotonNetwork.IsMasterClient)
        {
            PV.RPC("SendPoolDic", RpcTarget.MasterClient);
            return;
        }

        synced = true;

        foreach (Pool pool in pools)
            if (pool.prefab.GetComponent<PhotonView>())
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.size; i++)
                {
                    obj = PhotonNetwork.Instantiate(pool.prefab.name, transform.position, Quaternion.identity);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }
                poolDictionary.Add(pool.tag, objectPool);
            }*/
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag)) 
        {
            Debug.LogError("Pool " + tag + " does not exist!");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolDictionary[tag].Enqueue(objectToSpawn);
        return objectToSpawn;
    }

    public bool isSynced()
    {
        return synced;
    }

    [PunRPC]
    void SyncPoolDic(string[] tags, int[] queueViewID, int[] queueLengths)
    {
        if (synced) return;

        int filler = 0;

        for (int i = 0; i < tags.Length; i++)
        {
            Queue<GameObject> _queue = new Queue<GameObject>();
            for (int j = 0; j < queueLengths[i]; j++) 
            {
                GameObject _obj = PhotonView.Find(queueViewID[j + filler]).gameObject;
                _queue.Enqueue(_obj);
                if (!_obj.transform.parent)
                    _obj.SetActive(false);
            }
            poolDictionary.Add(tags[i], _queue);
            filler += queueLengths[i];
        }

        GameMechanics.gameMechanics.SyncPowerupsNow();
        synced = true;
    }

    [PunRPC]
    void SendPoolDic()
    {
        string[] tags = new string[poolDictionary.Count];

        int queueViewIDsLength = 0;
        foreach (KeyValuePair<string, Queue<GameObject>> kvp in poolDictionary)
            queueViewIDsLength += kvp.Value.Count;

        int[] queueViewIDs = new int[queueViewIDsLength];
        int[] queueLengths = new int[poolDictionary.Count];

        int i = 0, j = 0;
        foreach (KeyValuePair<string, Queue<GameObject>> kvp in poolDictionary)
        {
            if (kvp.Value.Peek().GetComponent<PhotonView>())
            {
                foreach (GameObject obj in kvp.Value)
                {
                    queueViewIDs[j] = obj.GetComponent<PhotonView>().ViewID;
                    j++;
                }
                tags[i] = kvp.Key;
                queueLengths[i] = kvp.Value.Count;
                i++;
            }
        }

        PV.RPC("SyncPoolDic", RpcTarget.Others, tags, queueViewIDs, queueLengths);
    }
}
