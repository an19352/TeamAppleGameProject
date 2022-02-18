using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PhotonPlayer : MonoBehaviour
{
    private PhotonView PV;
    public static GameMechanics gameMechanics;

    [HideInInspector]
    public GameObject myAvatar;

    public GameObject playerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        gameMechanics = GameMechanics.gameMechanics;

        float x = Random.Range(-14, 14);
        float z = Random.Range(-13, 14);

        if (PV.IsMine)
        {
            myAvatar = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(x, 6, z), Quaternion.identity, 0);
            PV.RPC("Add_player", RpcTarget.AllBuffered, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    void Add_player(int team)
    {
        gameMechanics.Add_player(myAvatar, team);
    }
}
