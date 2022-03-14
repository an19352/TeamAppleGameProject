using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ObjectiveFlag : MonoBehaviour
{
    PhotonView PV;
    public int defendTeam;
    public bool hasFlag;
    // set to the same value in the inspector
    public float captureDuration;
    public float captureCounter;
    public Material idleMaterial;
    public Material captureMaterial;
    public Material stalemateMaterial;
    public Material returnMaterial;
    // red <-> green 
    public GameObject otherObjectiveFlag;
    private GameObject flag;
    private GameObject detectionField;
    private List<(GameObject Player, int Team)> playersList = new List<(GameObject Player, int Team)>();
    private GameMechanics gameMechanics;
    private int numOfAttackers, numOfDefenders;
    private bool countDown;
    private enum State
    {
        // Idle: nothing happening 
        // Capture: attacker getting the flag
        // Stalemate: same/less amount of attackers & defenders
        // Return: defender returning the flag / attacker returning the flag
        Idle, Capture, Stalemate, Return
    };
    private State currentState;
    private Renderer fieldRenderer;
    void Start()
    {
        PV = GetComponent<PhotonView>();
        hasFlag = true;
        flag = transform.Find("Flag").gameObject;
        detectionField = transform.Find("DetectionField").gameObject;
        gameMechanics = GameMechanics.gameMechanics;
        numOfAttackers = 0;
        numOfDefenders = 0;
        currentState = State.Idle;
        fieldRenderer = detectionField.GetComponent<Renderer>();
        countDown = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine) return;
        if (countDown)
        {
            // the attacker has to stay for a certain duration before can capture flag
            captureCounter -= Time.deltaTime;
            if (captureCounter <= 0f)
            {
                hasFlag = false;
                flag.SetActive(false);
                // give the flag to the attacker which entered first 
                playersList[0].Player.GetComponent<FlagHolder>().enabled = true;
                ResetCounter();
                // call gameMechanics 

            }
        }
    }

    void ResetCounter()
    {
        // reset counter
        captureCounter = captureDuration;
    }

    State EvaluateState()
    {
        // the defending team has the flag
        if (hasFlag)
        {
            // first check if a defender has the other team's flag, if they have, score a point 
            foreach (var pair in playersList)
            {
                if (pair.Team == defendTeam && pair.Player.transform.Find("FLag") != null)
                {
                    return State.Return;
                };
            }
            if ((numOfAttackers + numOfDefenders) == 0)
            {
                return State.Idle;
            }
            // the flag can only be captured only when there are more attacker than defenders
            else if (numOfAttackers > numOfDefenders)
            {
                return State.Capture;
            }
            else if (numOfAttackers >= 1)
            {
                return State.Stalemate;
            }
            else return State.Idle;
        }
        else
        {
            // check if a defender has the flag from its own team to return 
            foreach (var pair in playersList)
            {
                if (pair.Player.transform.Find("Flag") != null)
                {
                    return State.Return;
                };
            }
            return State.Idle;
        }
    }

    // based on the current state, apply changes
    void ApplyChangesOnState()
    {
        currentState = EvaluateState();
        switch (currentState)
        {
            case State.Idle:
                fieldRenderer.material = idleMaterial;
                if (countDown)
                {
                    countDown = false;
                    ResetCounter();
                }
                break;
            case State.Capture:
                fieldRenderer.material = captureMaterial;
                // start the captureCounter
                countDown = true;
                break;
            case State.Return:
                fieldRenderer.material = returnMaterial;
                break;
            case State.Stalemate:
                fieldRenderer.material = stalemateMaterial;
                countDown = false;
                break;
        }
    }

    void OnTriggerEnter(Collider other)
    {

        // track players as they enter the detection field
        if (!playersList.Exists(pair => pair.Player == other.gameObject) && other.gameObject.CompareTag("Player"))
        {

            GameObject playerEntered = other.gameObject;
            int playerId = playerEntered.GetComponent<Movement>().GetId();
            int teamId = gameMechanics.checkTeam(playerId);
            // First and formost, if any friendly player enters holding a flag,
            // score a point for the team and disable its flag
            if (playerEntered.GetComponent<FlagHolder>().enabled)
            {

                gameMechanics.UpdateFlag(teamId, true);
                playerEntered.GetComponent<FlagHolder>().enabled = false;
                otherObjectiveFlag.GetComponent<ObjectiveFlag>().hasFlag = true;
                otherObjectiveFlag.transform.Find("Flag").gameObject.SetActive(true);
            }

            // update attacker/defender counter
            if (teamId == defendTeam)
            {
                numOfDefenders++;
            }
            else
            {
                numOfAttackers++;
            }
            playersList.Add((playerEntered, teamId));
        }
        ApplyChangesOnState();
    }

    void OnTriggerExit(Collider other)
    {
        // Untrack players as they leave the detection field
        if (other.gameObject.CompareTag("Player"))
        {
            (GameObject Player, int Team) playerExited = playersList.Find(pair => pair.Player == other.gameObject);
            if (playerExited.Player != null)
            {
                // update attacker/defender counter
                if (playerExited.Team == defendTeam)
                {
                    numOfDefenders--;
                }
                else
                {
                    numOfAttackers--;
                }
                playersList.Remove(playerExited);
            }
        }
        ApplyChangesOnState();
    }
}
