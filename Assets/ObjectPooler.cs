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

    private void Start()
    {
        PV = GetComponent<PhotonView>();
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach(Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                if (pool.prefab.GetComponent<PhotonView>())
                {
                    if (PhotonNetwork.IsMasterClient)
                        obj = PhotonNetwork.Instantiate(pool.prefab.name, transform.position, Quaternion.identity);
                }
                else
                    obj = Instantiate(pool.prefab);
                
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag)) 
        {
            Debug.LogWarning("Pool " + tag + " does not exist!");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();
        
        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        poolDictionary[tag].Enqueue(objectToSpawn);
        return objectToSpawn;
    }

}
