using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

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
        
        gameMechanics.redButton.onClick.AddListener(delegate { InitiatePlayer(0); });
        gameMechanics.greenButton.onClick.AddListener(delegate { InitiatePlayer(1); });

        if(gameMechanics.activePowerups.Count > 0)
        foreach(KeyValuePair<int, UnityEngine.Vector3> powerupID in gameMechanics.activePowerups)
        {
                GameObject powerup = PhotonView.Find(powerupID.Key).gameObject;
            powerup.SetActive(true);
            powerup.transform.position = powerupID.Value;
        }
    }

    // Update is called once per frame
    public void InitiatePlayer(int team)
    {
        float x = Random.Range(-7, 7);
        float z = Random.Range(-7, 3);

        if (PV.IsMine)
        {
            myAvatar = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(x, 6, z), Quaternion.identity, 0);
            gameMechanics.RPC_AddPlayer(myAvatar, team);
        }

        gameMechanics.menuItem.SetActive(false);
    }
}
