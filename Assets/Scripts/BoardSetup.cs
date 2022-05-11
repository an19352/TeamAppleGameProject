using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BoardSetup : MonoBehaviour
{
    // This script is used for mirroring boards and marking all of their components that need to be synchrinsed for the second step.
    public List<Transform> transforms; // When you create a board prefab, give it the transforms of ant object that sits on it
    
    public struct PhotonSpawnable  // Things that need to be synchronised when instantiated
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

    // Mirror any element on the x axis and destroy anyhting that has Photon View and return a list commemorating what was where
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
