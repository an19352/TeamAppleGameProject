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

    [Serializable]
    public struct FlagObjective
    {
        public int flagCount;
        public int numOfDefenders;
        public int numOfAttackers;
    }

    public List<Team> teams;
    public List<Player> players;
    public List<Transform> spawnPpoints;
    public List<Material> teamMaterials;

    public Timer timer;

    public List<GameObject> redgens;
    public List<GameObject> greengens;
    public Button redButton;
    public Button greenButton;
    public GameObject menuItem;
    public Canvas worldSpaceCanvas;
    public Transform greenFlags;
    public Transform redFlags;
    public FlagObjective[] flagObjectives;

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

        UpdateFlagUI();

    }
    /*
    private void Update()
    {
        foreach(Player player in players)
        {
            if (player.obj.GetPhotonView().Owner.IsInactive)
            {
                GameObject _obj = player.obj;
                players.Remove(player);
                Destroy(_obj);
            }
        }
    }*/



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
    public void Add_player(int playerViewId, int team)
    {
        Player _player = new Player { obj = PhotonView.Find(playerViewId).gameObject, team = team };
        players.Add(_player);
        _player.obj.GetComponent<Movement>().SetId(players.Count - 1);
    }

    public void RPC_RemovePlayer(int playerID)
    {
        PV.RPC("Remove_player", RpcTarget.All, playerID);
    }

    [PunRPC]
    public void Remove_player(int playerID)
    {
        Player _player = players[playerID];
        GameObject _obj = _player.obj;
        players.Remove(_player);
        Destroy(_obj);
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

        teams[teamID] = new Team { name = _name, score = _score, scoreText = _text };
    }

    #region FlagStuff
    [PunRPC]
    public void UpdateFlag(int teamID, bool isScore)
    {

        // if isScore is true, add one to flag count, else minus one 
        if (isScore)
        {
            for (int i = 0; i < flagObjectives.Length; i++)
            {
                if (teamID == i)
                {
                    flagObjectives[i].flagCount += 1;
                }
                else
                {
                    flagObjectives[i].flagCount -= 1;
                }
            }
        }
        // retrieve back a stolen flag, so only increment friendly flag count
        else
        {
            flagObjectives[teamID].flagCount += 1;
        }
    }

    [PunRPC]
    public void UpdateFlagUI()
    {
        foreach (Transform flag in greenFlags)
        {
            flag.GetComponent<Image>().enabled = false;
        }
        foreach (Transform flag in redFlags)
        {
            flag.GetComponent<Image>().enabled = false;
        }
        for (int i = 0; i < flagObjectives[0].flagCount; i++)
        {
            redFlags.GetChild(i).gameObject.GetComponent<Image>().enabled = true;
        }
        for (int i = 0; i < flagObjectives[1].flagCount; i++)
        {
            greenFlags.GetChild(i).gameObject.GetComponent<Image>().enabled = true;
        }
    }

    public void RPC_UpdateFlag(int teamID, bool isScore)
    {
        PV.RPC("UpdateFlag", RpcTarget.AllBuffered, teamID, isScore);
        // add something here to update the ui
        PV.RPC("UpdateFlagUI", RpcTarget.AllBuffered);
    }

    public void RPC_EnableFlagHolder(int playerID)
    {
        PV.RPC("EnableFlagHolder", RpcTarget.All, playerID);
    }
    [PunRPC]
    public void EnableFlagHolder(int playerID)
    {
        players[playerID].obj.GetComponent<FlagHolder>().enabled = true;
    }

    public void RPC_DisableFlagHolder(int playerID)
    {
        PV.RPC("DisableFlagHolder", RpcTarget.All, playerID);
    }
    [PunRPC]
    public void DisableFlagHolder(int playerID)
    {
        players[playerID].obj.GetComponent<FlagHolder>().enabled = false;
    }

    public void RPC_UpdateAttackers(int teamID, bool addition)
    {
        PV.RPC("UpdateAttackers", RpcTarget.AllBuffered, teamID, addition);
    }

    [PunRPC]
    public void UpdateAttackers(int teamID, bool addition)
    {
        if (addition)
        {
            flagObjectives[teamID].numOfAttackers++;
        }
        else
        {
            flagObjectives[teamID].numOfAttackers--;

        }
    }

    public void RPC_UpdateDefenders(int teamID, bool addition)
    {
        PV.RPC("UpdateDefenders", RpcTarget.AllBuffered, teamID, addition);
    }

    [PunRPC]
    public void UpdateDefenders(int teamID, bool addition)
    {
        if (addition)
        {
            flagObjectives[teamID].numOfDefenders++;
        }
        else
        {
            flagObjectives[teamID].numOfDefenders--;

        }
    }

    #endregion

    [PunRPC]
    void Sync(float game_time, int[] playerViewIds, int[] playerTeams, string[] teamNames, int[] teamScores, int[] flagCounts, int[] numsOfAttackers, int[] numsOfDefenders, int[] scoreViewIDs)
    {
        timer.UpdateTimer(game_time);
        List<Team> _teams = new List<Team>();
        for (int i = 0; i < teamNames.Length; i++) _teams.Add(new Team
        {
            name = teamNames[i],
            score = teamScores[i],
            scoreText = PhotonView.Find(scoreViewIDs[i]).gameObject.GetComponent<Text>(),
        });

        teams = _teams;

        List<Player> _players = new List<Player>();
        for (int i = 0; i < playerViewIds.Length; i++) _players.Add(new Player
        {
            obj = PhotonView.Find(playerViewIds[i]).gameObject,
            team = playerTeams[i]
        });
        players = _players;

        foreach (Player _player in players)
        {
            _player.obj.GetComponent<MeshRenderer>().material = teamMaterials[_player.team];
        }

        for (int i = 0; i < flagObjectives.Length; i++)
        {
            flagObjectives[i].flagCount = flagCounts[i];
            flagObjectives[i].numOfAttackers = numsOfAttackers[i];
            flagObjectives[i].numOfDefenders = numsOfDefenders[i];
        }
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

        int[] flagCounts = new int[teams.Count];
        int[] numsOfDefenders = new int[teams.Count];
        int[] numsOfAttackers = new int[teams.Count];

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
        foreach (KeyValuePair<int, UnityEngine.Vector3> keypair in activePowerups)
        {
            powerupsId[i] = keypair.Key;
            positions[i] = keypair.Value;
            i++;
        }

        i = 0;
        for (i = 0; i < flagObjectives.Length; i++)
        {
            flagCounts[i] = flagObjectives[i].flagCount;
            numsOfAttackers[i] = flagObjectives[i].numOfAttackers;
            numsOfDefenders[i] = flagObjectives[i].numOfDefenders;
        }


        PV.RPC("Sync", RpcTarget.Others, game_time, playerViewIds, playerTeams, teamNames, teamScores, flagCounts, numsOfAttackers, numsOfDefenders, scoreViewIDs);
    }

    public void SyncPowerupsNow()
    {
        PV.RPC("SendPowerups", RpcTarget.MasterClient);
    }

    [PunRPC]
    void SendPowerups()
    {
        int[] powerupsId = new int[activePowerups.Count];
        UnityEngine.Vector3[] positions = new UnityEngine.Vector3[activePowerups.Count];
        int i;

        i = 0;
        foreach (KeyValuePair<int, UnityEngine.Vector3> keypair in activePowerups)
        {
            powerupsId[i] = keypair.Key;
            positions[i] = keypair.Value;
            i++;
        }

        PV.RPC("SyncPowerups", RpcTarget.Others, powerupsId, positions);
    }

    [PunRPC]
    void SyncPowerups(int[] powerupsId, UnityEngine.Vector3[] positions)
    {
        Dictionary<int, UnityEngine.Vector3> _powerups = new Dictionary<int, UnityEngine.Vector3>();
        for (int i = 0; i < powerupsId.Length; i++)
        {
            _powerups.Add(powerupsId[i], positions[i]);

            GameObject _obj = PhotonView.Find(powerupsId[i]).gameObject;
            _obj.SetActive(true);
            _obj.transform.position = positions[i];
        }
        activePowerups = _powerups;
    }

    public void End_Game()
    {
        PV.RPC("EndGame", RpcTarget.AllBuffered);
    }

    public GameObject GetLocalPlayer()
    {
        foreach(Player player in players)
        {
            if (player.obj.GetComponent<PhotonView>().IsMine) return player.obj;
        }
        return null;
    }

    [PunRPC]
    void EndGame()
    {
        PhotonRoom.room.redScore = teams[0].score;
        PhotonRoom.room.greenScore = teams[1].score;
        SceneManager.LoadScene(2);
    }

    public void RPC_Destroy(GameObject obj)
    {
        if (obj.TryGetComponent(out PhotonView target))
            PV.RPC("DestroyProperty", RpcTarget.MasterClient, target.ViewID);
    }

    [PunRPC]
    void DestroyProperty(int PVID)
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(PhotonView.Find(PVID).gameObject);
    }

}
