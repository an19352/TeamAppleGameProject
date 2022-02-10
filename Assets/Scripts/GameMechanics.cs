using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

public class GameMechanics : MonoBehaviour
{
    public static GameMechanics gameMechanics;

    private void OnEnable()
    {
        if (GameMechanics.gameMechanics == null)
            GameMechanics.gameMechanics = this;
    }

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

    // Tells all players their ID
    public void Start()
    {
        for (int i = 0; i < players.Count; i++)
            players[i].obj.GetComponent<Movement>().SetId(i);
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
}
