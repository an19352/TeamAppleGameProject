using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Random = System.Random;
using Vector3 = UnityEngine.Vector3;

public class GameMechanics : MonoBehaviour
{
    // The script that does it all. This singleton shares variables with and synchronises almost all other objects

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

        [HideInInspector]       // The score is hidden from inspector. This can be undone
        public int score;       // if we ever want teams to start with an advantage. Text will need change
    }

    [Serializable]
    public struct Player         // Allows for Players to be affiliated with teams
    {
        public GameObject obj;
        public int team;
    }

    [Serializable]
    public struct FlagObjective   // The areas where you capture a flag
    {
        public GameObject objective;
        public int flagCount;
        public int numOfDefenders;
        public int numOfAttackers;

        public FlagObjective(GameObject _obj)
        {
            objective = _obj;
            flagCount = 3;
            numOfAttackers = 0;
            numOfDefenders = 0;
        }
    }

    public List<Team> teams;
    public List<Player> players;
    public List<Transform> spawnPpoints;  // Where powerups spawn
    public List<Material> teamMaterials;

    public Timer timer;

    public List<GameObject> redgens = new List<GameObject>();
    public List<GameObject> greengens = new List<GameObject>();
    public Canvas worldSpaceCanvas;
    public Transform blueFlags;
    public Transform redFlags;
    public FlagObjective[] flagObjectives = new FlagObjective[2];
    public List<GameObject> bases;
    public PhotonPlayer PB;
    public bool readyToDeploy = false;
    public GameObject MapGenerator;
    public GameObject mini;
    public GameObject CMvcams;
    public GameObject Powerupgens;
    [HideInInspector] public Vector3 drop = new Vector3(0,0,0);

    PhotonView PV;

    [HideInInspector]
    public Dictionary<int, UnityEngine.Vector3> activePowerups;

    // Tells all players their ID
    public void Start()
    {
        PV = GetComponent<PhotonView>();

        activePowerups = new Dictionary<int, UnityEngine.Vector3>();


        //for (int i = 0; i < players.Count; i++)
        //  players[i].obj.GetComponent<Movement>().SetId(i);

    }

    // The Local Player has loaded and so the map generation can begin on this client
    public void SetPB(PhotonPlayer _new)
    {
        PB = _new;
        MapGenerator.SetActive(true);
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
        //if (players.Count > 0)
        Player _player = new Player { obj = PhotonView.Find(playerViewId).gameObject, team = team };
        players.Add(_player);
        _player.obj.GetComponent<Movement>().SetId(players.Count - 1);

        PhotonView PPV = PhotonView.Find(playerViewId);

        // Continue the chain of player initialisation
        if (PPV.IsMine)
            if (PPV.OwnerActorNr < PhotonNetwork.PlayerList.Length)
            {
                int next = ((int)PPV.OwnerActorNr);
                PV.RPC("InitiatePlayer", PhotonNetwork.PlayerList[next]);
            }
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
        PV.RPC("Score", RpcTarget.All, teamID);
    }

    // Increments the score of a team by one
    [PunRPC]
    public void Score(int teamID)
    {
        string _name = teams[teamID].name;
        int _score = teams[teamID].score + 1;

        teams[teamID] = new Team { name = _name, score = _score };
    }

    #region FlagStuff
    public void RPC_IncreaseFlag(int teamID, int origin)   // teamID just brought a flag home
    {
        int voiceID = GenerateCommentary(teamID);
        PlaySound.playSound.RPC_QueueVoice(voiceID, PhotonNetwork.PlayerList);
        PV.RPC("IncreaseFlag", RpcTarget.All, teamID);
        PV.RPC("UpdateFlagUI", RpcTarget.All);
        int otherTeam = teamID == 0 ? 1 : 0;

        flagObjectives[origin].objective.GetComponent<ObjectiveFlag>().hasFlag = true; // returns the flag to the flag objective

    }
    public void RPC_DecreaseFlag(int teamID)    // teamID just lost their flag
    {
        PV.RPC("DecreaseFlag", RpcTarget.All, teamID);
        PV.RPC("UpdateFlagUI", RpcTarget.All);
        flagObjectives[teamID].objective.GetComponent<ObjectiveFlag>().hasFlag = false; // the flag was taken
    }

    [PunRPC]
    public void IncreaseFlag(int teamID)
    {
        flagObjectives[teamID].flagCount += 1;


        // restore generators
        if (teamID == 1)
        {
            for (int j = 0; j < 3; j++)
            {
                redgens[j].SetActive(true);
            }
        }
        else
        {
            for (int j = 0; j < 3; j++)
            {
                greengens[j].SetActive(true);
            }
        }


    }

    [PunRPC]
    public void DecreaseFlag(int teamID)
    {
        flagObjectives[teamID].flagCount -= 1;
        flagObjectives[teamID].objective.GetComponent<ObjectiveFlag>().hasFlag = false;
    }


    [PunRPC]
    public void UpdateFlagUI()
    {
        redFlags.gameObject.SetActive(true);
        blueFlags.gameObject.SetActive(true);
        // 12 => icon + border
        var redImgs = redFlags.gameObject.GetComponentsInChildren<Image>();
        // 12 => icon + border
        var greenImgs = blueFlags.gameObject.GetComponentsInChildren<Image>();
        foreach (Transform flag in blueFlags)
        {
            var icons = flag.GetComponentsInChildren<Image>();
            foreach (var icon in icons)
            {
                icon.enabled = false;
            }
        }
        foreach (Transform flag in redFlags)
        {
            var icons = flag.GetComponentsInChildren<Image>();
            foreach (var icon in icons)
            {
                icon.enabled = false;
            }
        }
        for (int i = 0; i < flagObjectives[0].flagCount * 2; i++)
        {
            redImgs[i].enabled = true;
        }
        for (int i = 0; i < flagObjectives[1].flagCount * 2; i++)
        {
            greenImgs[i].enabled = true;
        }

        if (PhotonNetwork.IsMasterClient)
            if (flagObjectives[0].flagCount == 6 || flagObjectives[1].flagCount == 6)
                End_Game();
    }


    // Tells all clients that playerID has taken a flag and so, it should be displayed above its head
    public void RPC_EnableFlagHolder(int playerID, int flagOrigin)
    {
        Photon.Realtime.Player[] target = { players[playerID].obj.GetComponent<PhotonView>().Owner };
        PlaySound.playSound.RPC_QueueVoice(20, target);
        PV.RPC("EnableFlagHolder", RpcTarget.All, playerID, flagOrigin);
    }
    [PunRPC]
    public void EnableFlagHolder(int playerID, int flagOrigin)
    {
        FlagHolder fh = players[playerID].obj.GetComponent<FlagHolder>();
        fh.enabled = true;
        fh.ballOrigin = flagOrigin;
    }

    // Tells all clients that playerID just lost the flag
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
        PV.RPC("UpdateAttackers", RpcTarget.All, teamID, addition);
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
        PV.RPC("UpdateDefenders", RpcTarget.All, teamID, addition);
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

    [PunRPC] // Called by the MasterClient on everyone at the start of the game to make sure everyone is on the same page
    void Sync(float game_time, int[] playerViewIds, int[] playerTeams, string[] teamNames, int[] teamScores, int[] flagCounts, int[] numsOfAttackers, int[] numsOfDefenders)
    {
        timer.UpdateTimer(game_time);
        List<Team> _teams = new List<Team>();
        for (int i = 0; i < teamNames.Length; i++) _teams.Add(new Team
        {
            name = teamNames[i],
            score = teamScores[i],
        });

        teams = _teams;

        List<Player> _players = new List<Player>();
        for (int i = 0; i < playerViewIds.Length; i++) _players.Add(new Player
        {
            obj = PhotonView.Find(playerViewIds[i]).gameObject,
            team = playerTeams[i]
        });
        players = _players;

        /*foreach (Player _player in players)
        {
            _player.obj.GetComponent<MeshRenderer>().material = teamMaterials[_player.team];
        }*/

        for (int i = 0; i < flagObjectives.Length; i++)
        {
            flagObjectives[i].flagCount = flagCounts[i];
            flagObjectives[i].numOfAttackers = numsOfAttackers[i];
            flagObjectives[i].numOfDefenders = numsOfDefenders[i];
        }
    }

    [PunRPC] // Called after the map generation is done, only on the Master Client to synchronise everyone else with him
    void SendVariables()
    {
        float game_time = timer.GetTimer();

        int[] playerViewIds = new int[players.Count];
        int[] playerTeams = new int[players.Count];

        string[] teamNames = new string[teams.Count];
        int[] teamScores = new int[teams.Count];

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


        PV.RPC("Sync", RpcTarget.Others, game_time, playerViewIds, playerTeams, teamNames, teamScores, flagCounts, numsOfAttackers, numsOfDefenders);
    }

    // Used to be usefull when we allowed players to enter an ongoing match. Synchronises the powerup generator
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

    // Ends the game and takes everyone to the next scene
    public void End_Game()
    {
        PV.RPC("EndGame", RpcTarget.All);
    }

    // Just a handy function to find the player avatar of your client
    public GameObject GetLocalPlayer()
    {
        foreach (Player player in players)
        {
            if (player.obj.GetComponent<PhotonView>().IsMine) return player.obj;
        }
        return null;
    }

    // Just a handy function to find the team your player avatar is on
    public int GetLocalPlayerTeam()
    {
        foreach (Player player in players)
        {
            if (player.obj.GetComponent<PhotonView>().IsMine) return player.team;
        }
        return -1;
    }
    [PunRPC]
    void EndGame()
    {
        PhotonRoom.room.redFlag = flagObjectives[0].flagCount;
        PhotonRoom.room.blueFlag = flagObjectives[1].flagCount;
        PhotonRoom.room.redScore = teams[1].score;
        PhotonRoom.room.blueScore = teams[0].score;
        SceneManager.LoadScene(3);
    }

    // Only the Master Client can destroy any object. So if we ever need that power, Game Mechanics could do it
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

    int GenerateCommentary(int capTeamID)
    {
        int commentaryID = 0;
        Random ran = new Random();
        int randomNum;

        if (capTeamID == 1)
        {
            if (flagObjectives[1].flagCount < flagObjectives[0].flagCount)
            {
                randomNum = ran.Next(2, 6);
            }

            else
            {
                if (flagObjectives[1].flagCount > (flagObjectives[0].flagCount + 1))
                {
                    randomNum = ran.Next(1, 3);
                }
                else
                {
                    randomNum = 2;
                }

            }
            

            if (randomNum == 1)
            {
                commentaryID = 7;
            }

            if (randomNum == 2)
            {
                commentaryID = 6;
            }

            if (randomNum > 2)
            {
                commentaryID = 5;
            }
        }

        if (capTeamID == 0)
        {
            if (flagObjectives[0].flagCount < flagObjectives[1].flagCount)
            {
                randomNum = ran.Next(2, 6);
            }

            else
            {
                if (flagObjectives[0].flagCount > (flagObjectives[1].flagCount + 1))
                {
                    randomNum = ran.Next(1, 3);
                }
                else
                {
                    randomNum = 2;
                }

            }

            if (randomNum == 1)
            {
                commentaryID = 10;
            }

            if (randomNum == 2)
            {
                commentaryID = 9;
            }

            if (randomNum > 2)
            {
                commentaryID = 8;
            }
        }
        
        return commentaryID;
    }

    // Starts the chain of spawning players one by one. This gets called after the map generation finishes
    public void RPC_InitiatePlayer()
    {
        PV.RPC("InitiatePlayer", PhotonNetwork.PlayerList[0]);
    }

    [PunRPC]
    void InitiatePlayer()
    {
        UpdateFlagUI();

        if (PhotonNetwork.IsMasterClient)
            SendVariables();

        PB.InitiatePlayer();
        mini.SetActive(true);
        CMvcams.SetActive(true);
        Powerupgens.SetActive(true);
    }
}
