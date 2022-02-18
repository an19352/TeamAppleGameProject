using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameMechanics : MonoBehaviour
{
    public static GameMechanics gameMechanics;

    #region Singleton

    private void Awake()
    {
        if (GameMechanics.gameMechanics == null) GameMechanics.gameMechanics = this;
        else if (GameMechanics.gameMechanics != this)
        {
            Destroy(GameMechanics.gameMechanics.gameObject);
            GameMechanics.gameMechanics = this;
        }
    }

    #endregion

    [Serializable]
    public struct Team           // This allows for Teams to be added via the inspector
    {
        public string name;
        public Text scoreText;

        [HideInInspector]       // The score is hidden from inspector. This can be undone
        public int score;       // if we ever want teams to start with an advantage. Text will need change
    }

    [Serializable]
    public struct Player         // Allows for Players to be affiliated with teams in inspector
    {
        public GameObject obj;
        public int team;
    }

    public List<Team> teams;      
    public List<Player> players;
    public List<Transform> spawnPpoints;

    PhotonView PV;

    [HideInInspector]
    public Dictionary<int, UnityEngine.Vector3> activePowerups;

    // Tells all players their ID
    public void Start()
    {
        PV = GetComponent<PhotonView>();
        activePowerups = new Dictionary<int, UnityEngine.Vector3>();

        if (!PV.Owner.IsMasterClient)
            PV.RPC("SendVariables", RpcTarget.MasterClient);

        for (int i = 0; i < players.Count; i++)
            players[i].obj.GetComponent<Movement>().SetId(i);
    }

    // Increments the score of a team by one
    [PunRPC]
    public void Score(int teamID)
    {
        string _name = teams[teamID].name;
        int _score = teams[teamID].score + 1;
        Text _text = teams[teamID].scoreText;
        _text.text = _score.ToString();

        teams[teamID] = new Team { name = _name, score = _score, scoreText = _text };
    }

    // For functions in other classes, returns a player's team
    public int checkTeam(int playerID)
    {
        return players[playerID].team;
    }

    public void Add_player (GameObject player, int team)
    {
        Player _player = new Player { obj = player, team = team };
        players.Add(_player);
    }

    public void RPC_Score(int teamID)
    { 
        PV.RPC("Score", RpcTarget.AllBuffered, teamID);
    }

    [PunRPC]
    void Sync(int teamScore0, int teamScore1, int[] playerViewIds, int[] playerTeams, int[] powerupsId, UnityEngine.Vector3[] positions)
    {
        List<Team> _teams = new List<Team>();
        List<int> scores = new List<int>();
        scores.Add(teamScore0);
        scores.Add(teamScore1);
        for (int i = 0; i < 2; i++) teams.Add(new Team
        {
            name = teams[i].name,
            score = scores[i],
            scoreText = teams[i].scoreText
        }); 
        teams = _teams;

        List<Player> _players = new List<Player>();
        for (int i = 0; i < playerViewIds.Length; i++) _players.Add(new Player
        {
            obj = PhotonView.Find(playerViewIds[i]).gameObject,
            team = playerTeams[i]
        });
        players = _players;

        Dictionary<int, UnityEngine.Vector3> _powerups = new Dictionary<int, UnityEngine.Vector3>();
        for (int i = 0; i < powerupsId.Length; i++) _powerups.Add(powerupsId[i], positions[i]);
        activePowerups = _powerups;
    }

    [PunRPC]
    void SendVariables()
    {
        int[] playerViewIds = new int[players.Count];
        int[] playerTeams = new int[players.Count];
        int[] powerupsId = new int[activePowerups.Count];
        UnityEngine.Vector3[] positions = new UnityEngine.Vector3[activePowerups.Count];
        int i;

        for (i = 0; i < players.Count; i++)
        {
            playerViewIds[i] = players[i].obj.GetComponent<PhotonView>().ViewID;
            playerTeams[i] = players[i].team;
        }

        i = 0;
        foreach(KeyValuePair<int, UnityEngine.Vector3> keypair in activePowerups)
        {
            powerupsId[i] = keypair.Key;
            positions[i] = keypair.Value;
        }

        PV.RPC("Sync", RpcTarget.Others, teams[0].score, teams[1].score, playerViewIds, playerTeams, powerupsId, positions);
    }
}
