using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PhotonPlayer : MonoBehaviour
{
    private PhotonView PV;

    [HideInInspector]
    public GameObject myAvatar;

    public GameObject playerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        float x = Random.Range(-14, 14);
        float z = Random.Range(-13, 14);

        if (PV.IsMine)
        {
            myAvatar = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(x, 6, z), Quaternion.identity, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
