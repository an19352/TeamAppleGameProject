using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PhotonPlayer : MonoBehaviourPunCallbacks
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

        gameMechanics.redButton.onClick.AddListener(delegate { InitiatePlayer(0); });
        gameMechanics.greenButton.onClick.AddListener(delegate { InitiatePlayer(1); });

        if (gameMechanics.activePowerups.Count > 0)
            foreach (KeyValuePair<int, UnityEngine.Vector3> powerupID in gameMechanics.activePowerups)
            {
                GameObject powerup = PhotonView.Find(powerupID.Key).gameObject;
                powerup.SetActive(true);
                powerup.transform.position = powerupID.Value;
            }
    }

    // Update is called once per frame
    public void InitiatePlayer(int team)
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
            Arrow arrow = myAvatar.GetComponent<Arrow>();
            arrow.enabled = true;
            if (team == 0)
            {
                arrow.getgens(gameMechanics.greengens);
            }
            else
            {
                arrow.getgens(gameMechanics.redgens);
            }
        }

        gameMechanics.menuItem.SetActive(false);
    }
}
