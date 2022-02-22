using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    public Timer timer;

    public Button redButton;
    public Button greenButton;
    public GameObject menuItem;

    PhotonView PV;

    [HideInInspector]
    public Dictionary<int, UnityEngine.Vector3> activePowerups;

    // Tells all players their ID
    public void Start()
    {
        PV = GetComponent<PhotonView>();
        activePowerups = new Dictionary<int, UnityEngine.Vector3>();

        if (!PhotonNetwork.IsMasterClient)
            PV.RPC("SendVariables", RpcTarget.MasterClient);

        for (int i = 0; i < players.Count; i++)
            players[i].obj.GetComponent<Movement>().SetId(i);
    }

    // For functions in other classes, returns a player's team
    public int checkTeam(int playerID)
    {
        return players[playerID].team;
    }

    public void RPC_AddPlayer(GameObject player, int team)
    {
        PV.RPC("Add_player", RpcTarget.All, player.GetComponent<PhotonView>().ViewID, team);
        // If a player order inconsistency arises, add a sync here
    }

    [PunRPC]
    public void Add_player (int playerViewId, int team)
    {
        Player _player = new Player { obj = PhotonView.Find(playerViewId).gameObject, team = team };
        players.Add(_player);
        _player.obj.GetComponent<Movement>().SetId(players.Count - 1);
    }

    public void RPC_Score(int teamID)
    {
        PV.RPC("Score", RpcTarget.AllBuffered, teamID);
    }

    // Increments the score of a team by one
    [PunRPC]
    public void Score(int teamID)
    {
        string _name = teams[teamID].name;
        int _score = teams[teamID].score + 1;
        Text _text = teams[teamID].scoreText;
        _text.text = _score.ToString();
        Debug.Log("Here is the issue: " + teamID.ToString());

        teams[teamID] = new Team { name = _name, score = _score, scoreText = _text };
    }

    [PunRPC]
    void Sync(float game_time, int[] playerViewIds, int[] playerTeams, string[] teamNames, int[] teamScores, int[] scoreViewIDs, int[] powerupsId, UnityEngine.Vector3[] positions)
    {
        timer.UpdateTimer(game_time);

        List<Team> _teams = new List<Team>();
        for (int i = 0; i < teamNames.Length; i++)
        {
            teams.Add(new Team
            {
                name = teamNames[i],
                score = teamScores[i],
                scoreText = PhotonView.Find(scoreViewIDs[i]).gameObject.GetComponent<Text>()
            });
            Debug.LogError(teamNames[i]);
        }
        Debug.LogError(teamNames.Length);
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
        float game_time = timer.GetTimer();

        int[] playerViewIds = new int[players.Count];
        int[] playerTeams = new int[players.Count];

        string[] teamNames = new string[teams.Count];
        int[] teamScores = new int[teams.Count]; 
        int[] scoreViewIDs = new int[teams.Count];

        int[] powerupsId = new int[activePowerups.Count];
        UnityEngine.Vector3[] positions = new UnityEngine.Vector3[activePowerups.Count];
        int i;

        for (i = 0; i < players.Count; i++)
        {
            playerViewIds[i] = players[i].obj.GetComponent<PhotonView>().ViewID;
            playerTeams[i] = players[i].team;
        }

        for (i = 0; i < teams.Count; i++)
        {
            teamNames[i] = teams[i].name;
            teamScores[i] = teams[i].score;
            scoreViewIDs[i] = teams[i].scoreText.gameObject.GetComponent<PhotonView>().ViewID;
        }

        i = 0;
        foreach(KeyValuePair<int, UnityEngine.Vector3> keypair in activePowerups)
        {
            powerupsId[i] = keypair.Key;
            positions[i] = keypair.Value;
            i++;
        }

        PV.RPC("Sync", RpcTarget.Others, game_time, playerViewIds, playerTeams, teamNames, teamScores, scoreViewIDs, powerupsId, positions);
    }

    public void End_Game()
    {
        PV.RPC("EndGame", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void EndGame()
    {
        PhotonRoom.room.redScore = teams[0].score;
        PhotonRoom.room.greenScore = teams[1].score;
        SceneManager.LoadScene(2);
    }
}
