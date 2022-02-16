using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;


public class GameMechannicsPhoton : MonoBehaviour
{
    [Serializable]
    public struct Team           // This allows for Teams to be added via the inspector
    {
        public string name;
        public Text scoreText;
        public Text powerText;

        [HideInInspector]       // The score is hidden from inspector. This can be undone
        public int score;       // if we ever want teams to start with an advantage. Text will need change
    }

    [Serializable]
    public struct Player         // Allows for Players to be affiliated with teams in inspector
    {
        public GameObject obj;
        public int team;

        public Player(GameObject obj, int team)
        {
            this.obj = obj;
            this.team = team;
        }
    }

    public List<Team> teams;
    public List<Player> players;
    public List<Material> AllowedColours;

    // Tells all players their ID
    public void Start()
    {
        for (int i = 0; i < players.Count; i++)
            players[i].obj.GetComponent<MovementPhoton>().SetId(i);
    }

    // Increments the score of a team by one
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

    public void addPlayer(GameObject player, int team) 
    {
        Debug.Log("Hello");
        player.GetComponent<MovementPhoton>().SetId(players.Count);

        int randdomIndex = players.Count % AllowedColours.Count;
        Material randomMaterial = AllowedColours[randdomIndex];
        player.GetComponent<Renderer>().material = randomMaterial;

        if (team < 0) team = UnityEngine.Random.Range(0, teams.Count);
        Player _player = new Player(player, team);
        players.Add(_player);

        player.GetComponent<Movement>().currentPowerup = teams[team].powerText;
    }
}
