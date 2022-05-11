using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PhotonPlayer : MonoBehaviourPunCallbacks
{
    // Every client has one of these for every player
    private PhotonView PV;
    GameMechanics gameMechanics;
    Transform spawningPoint;

    [HideInInspector]
    public GameObject myAvatar;

    public GameObject playerPrefab;
    int team = -1;


    void Start()
    {
        PV = GetComponent<PhotonView>();
        if (!PV.IsMine) return;
        
        gameMechanics = GameMechanics.gameMechanics;

        //gameMechanics.redButton.onClick.AddListener(delegate { InitiatePlayer(0); });
        //gameMechanics.greenButton.onClick.AddListener(delegate { InitiatePlayer(1); });

        if (team == -1)
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.Destroy(PhotonRoom.room.gameObject);
            SceneManager.LoadScene(0);
        }

        //InitiatePlayer();
        GameMechanics.gameMechanics.SetPB(this); // Signal your friendly neighbourhood Game Mechanics to start the map generation
        

/*        if (gameMechanics.activePowerups.Count > 0)
            foreach (KeyValuePair<int, UnityEngine.Vector3> powerupID in gameMechanics.activePowerups)
            {
                GameObject powerup = PhotonView.Find(powerupID.Key).gameObject;
                powerup.SetActive(true);
                powerup.transform.position = powerupID.Value;
            }*/
    }

/*    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) InitiatePlayer();
    }*/

    public void SetTeam(int _team) { team = _team; }

    // Spawn in your avatar and tell Game Mechanics about it
    public void InitiatePlayer()
    {
        int playerLayer;

        if (PV.IsMine)
        {
            PV.RPC("SetSpawnPoint", RpcTarget.All, team);
            playerLayer = 12 + team;

            myAvatar = PhotonNetwork.Instantiate(playerPrefab.name, spawningPoint.position, spawningPoint.rotation, 0);
            myAvatar.layer = playerLayer;
            gameMechanics.RPC_AddPlayer(myAvatar, team);

            Transform arrow = myAvatar.transform.Find("ArrowCanvas");
            arrow.GetChild(0).gameObject.SetActive(true);
            //Debug.Log(15);

            if (team == 0)
            {
                myAvatar.GetComponent<Arrow>().getgens(gameMechanics.greengens, gameMechanics.bases[0], gameMechanics.bases[1], team);
            }
            else
            {
                myAvatar.GetComponent<Arrow>().getgens(gameMechanics.redgens, gameMechanics.bases[0], gameMechanics.bases[1], team);
            }
            //Debug.Log(16);
        }
    }

    [PunRPC]
    public void SetSpawnPoint(int _team)
    {
        spawningPoint = GameMechanics.gameMechanics.bases[_team].GetComponent<ListSpawningPoints>().BookSpawn();
    }
}
