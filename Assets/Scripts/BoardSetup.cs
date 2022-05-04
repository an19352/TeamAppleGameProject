using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BoardSetup : MonoBehaviour
{
    public List<Transform> transforms;
    
    public struct PhotonSpawnable
    {
        public string prefab;
        public Vector3 position;
        public Quaternion rotation;
        public PhotonSpawnable(string _prefab, Vector3 _position, Quaternion _rotation)
        {
            prefab = _prefab;
            position = _position;
            rotation = _rotation;
        }
    }

    // Update is called once per frame
    public List<PhotonSpawnable> Setup()
    {
        List<PhotonSpawnable> result = new List<PhotonSpawnable>();

        Vector3 helper;
        foreach(Transform trans in transforms)
        {
            if (trans.GetComponent<PhotonView>() != null)
            {
                result.Add(new PhotonSpawnable(trans.gameObject.name, trans.position, trans.rotation));
                Destroy(trans.gameObject);
            }
            else
            {
                helper = trans.position;
                helper.x = transform.position.x - (trans.position.x - transform.position.x);
                trans.position = helper;
            }
        }

        if (result.Count <= 0) return null;
        else return result;
    }
}
