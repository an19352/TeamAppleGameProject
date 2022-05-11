using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class teamSelectionScreenManager : MonoBehaviour
{
    // Makes sure no more than 3 people join each team and everything is displayed correctly
    PhotonView PV;
    public List<Transform> contexts;
    public List<Button> buttons;

    [HideInInspector]
    public List<GameObject> redplayers = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> blueplayers = new List<GameObject>();

    [SerializeField]
    public List<List<GameObject>> players;

    public int maxPeoplePerTeam = 3;
    public Button startButton;
    public GameObject Nickname;
    public PhotonRoom PR;

    private void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    private void OnEnable()
    {
        if (PhotonNetwork.IsMasterClient) startButton.interactable = true;
    }

    public void RPC_LeaveTeam(int team)
    {
        PV.RPC("LeaveTeam", RpcTarget.AllBuffered, team, PhotonNetwork.LocalPlayer.ActorNumber - 1);
        buttons[team].gameObject.SetActive(true);
        if (PR.team == team) PR.team = -1;
    }

    [PunRPC]
    public void LeaveTeam(int team, int playerActorNumber)
    {
        List<GameObject> players = lookUp(team);
        foreach(GameObject playerNick in players.ToArray())
            if (playerNick.GetComponent<TMP_Text>().text == PhotonNetwork.PlayerList[playerActorNumber].NickName)
            {
                players.Remove(playerNick);
                Destroy(playerNick);
                buttons[team].interactable = true;
                //startButton.interactable = false;
            }
    }

    public List<GameObject> lookUp(int team)
    {
        if (team == 0) return redplayers;
        else return blueplayers;
    }

    public void RPC_JoinTeam(int team)
    {
        List<GameObject> players = lookUp(team);
        if (players.Count >= maxPeoplePerTeam) return;

        PV.RPC("JoinTeam", RpcTarget.AllBuffered, team, PhotonNetwork.LocalPlayer.ActorNumber - 1);
        PR.team = team;
        buttons[team].gameObject.SetActive(false);
        team = (team + 1) % 2;
        foreach (GameObject playerNick in players.ToArray()) if (playerNick.GetComponent<TMP_Text>().text == PhotonNetwork.NickName)
                RPC_LeaveTeam(team);
    }

    [PunRPC]
    public void JoinTeam(int team, int playerActorNumber)
    {
        List<GameObject> players = lookUp(team);
        if (players.Count >= maxPeoplePerTeam) return;
        
        GameObject _obj = Instantiate(Nickname, contexts[team]);
        players.Add(_obj);
        
        _obj.GetComponent<TMP_Text>().text = PhotonNetwork.PlayerList[playerActorNumber].NickName;
        if (players.Count >= maxPeoplePerTeam) buttons[team].interactable = false;

        team = (team + 1) % 2;
        if (redplayers.Count + blueplayers.Count == maxPeoplePerTeam * 2 && PhotonNetwork.IsMasterClient) 
            Debug.Log("Now you'd be able to start the game");
    }

    private void OnDisable()
    {
        startButton.interactable = false;
    }
}
