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
    public int firstEnteredId;
    private GameObject flag;
    private GameObject detectionField;
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
        flag = transform.Find("Ball").gameObject;
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
                // give the flag to the attacker which entered first (RPC)
                ResetCounter();
            }
        }
    }

    void ResetCounter()
    {
        // reset counter
        captureCounter = captureDuration;
        countDown = false;
    }

    // State EvaluateState()
    // {
    //     // the defending team has the flag
    //     if (hasFlag)
    //     {
    //         // first check if a defender has the other team's flag, if they have, score a point 
    //         foreach (var pair in playersList)
    //         {
    //             if (pair.Team == defendTeam && pair.Player.transform.Find("Flag") != null)
    //             {
    //                 return State.Return;
    //             };
    //         }
    //         if ((numOfAttackers + numOfDefenders) == 0)
    //         {
    //             return State.Idle;
    //         }
    //         // the flag can only be captured only when there are more attacker than defenders
    //         else if (numOfAttackers > numOfDefenders)
    //         {
    //             return State.Capture;
    //         }
    //         else if (numOfAttackers >= 1)
    //         {
    //             return State.Stalemate;
    //         }
    //         else return State.Idle;
    //     }
    //     else
    //     {
    //         // check if a defender has the flag from its own team to return 
    //         foreach (var pair in playersList)
    //         {
    //             if (pair.Player.transform.Find("Flag") != null)
    //             {
    //                 return State.Return;
    //             };
    //         }
    //         return State.Idle;
    //     }
    // }

    // based on the current state, apply changes
    // void ApplyChangesOnState()
    // {
    //     currentState = EvaluateState();
    //     switch (currentState)
    //     {
    //         case State.Idle:
    //             fieldRenderer.material = idleMaterial;
    //             if (countDown)
    //             {
    //                 ResetCounter();
    //             }
    //             break;
    //         case State.Capture:
    //             fieldRenderer.material = captureMaterial;
    //             // start the captureCounter
    //             countDown = true;
    //             break;
    //         case State.Return:
    //             fieldRenderer.material = returnMaterial;
    //             break;
    //         case State.Stalemate:
    //             fieldRenderer.material = stalemateMaterial;
    //             countDown = false;
    //             break;
    //     }
    // }

    void OnTriggerEnter(Collider other)
    {
        if (!PV.IsMine) return;
        // track players as they enter the detection field
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject playerEntered = other.gameObject;
            int playerID = playerEntered.GetComponent<Movement>().GetId();
            gameMechanics.RPC_PlayerEnter(playerID, defendTeam);
        }
        // ApplyChangesOnState();
    }

    void OnTriggerExit(Collider other)
    {
        if (!PV.IsMine) return;

        // Untrack players as they leave the detection field
        if (other.gameObject.CompareTag("Player"))
        {
            int playerID = other.gameObject.GetComponent<Movement>().GetId();
            gameMechanics.RPC_PlayerExit(playerID, defendTeam);
            // playersList.Remove(playerExited);
        }
    }
    // ApplyChangesOnState();
}
