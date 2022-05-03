using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PhotonPlayer : MonoBehaviourPunCallbacks
{
    private PhotonView PV;
    GameMechanics gameMechanics;

    [HideInInspector]
    public GameObject myAvatar;

    public GameObject playerPrefab;
    int team = -1;


    // Start is called before the first frame update
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

        InitiatePlayer();
        //GameMechanics.gameMechanics.SetPB(this);
        

/*        if (gameMechanics.activePowerups.Count > 0)
            foreach (KeyValuePair<int, UnityEngine.Vector3> powerupID in gameMechanics.activePowerups)
            {
                GameObject powerup = PhotonView.Find(powerupID.Key).gameObject;
                powerup.SetActive(true);
                powerup.transform.position = powerupID.Value;
            }*/
    }

    public void SetTeam(int _team) { team = _team; }

    // Update is called once per frame
    public void InitiatePlayer()
    {
        float x;
        float z;
        float greenX = Random.Range(-145, -145);
        float greenZ = Random.Range(-16, 16);
        float redX = Random.Range(145, 145);
        float redZ = Random.Range(-16, 16);
        int playerLayer;

        if (PV.IsMine)
        {
            if (team == 1)
            {
                playerLayer = 13;
                z = greenZ;
                x = greenX;
            }
            else
            {
                playerLayer = 12;
                z = redZ;
                x = redX;

            }
            myAvatar = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(x, 6, z), Quaternion.identity, 0);
            myAvatar.layer = playerLayer;
            gameMechanics.RPC_AddPlayer(myAvatar, team);

            /*Transform arrow = myAvatar.transform.Find("ArrowCanvas");
            arrow.GetChild(0).gameObject.SetActive(true);
            Debug.Log(15);

            if (team == 0)
            {
                myAvatar.GetComponent<Arrow>().getgens(gameMechanics.greengens, gameMechanics.bases[0], gameMechanics.bases[1], team);
            }
            else
            {
                myAvatar.GetComponent<Arrow>().getgens(gameMechanics.redgens, gameMechanics.bases[0], gameMechanics.bases[1], team);
            }
            Debug.Log(16);*/
        }
    }
}
