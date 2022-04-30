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
   // public GameObject otherObjectiveFlag;

    public List<GameMechanics.Player> playerList;
    public int numOfAttackers = 0, numOfDefenders = 0;
    public int firstEnteredId;


    public GameObject flag;
    public GameObject detectionField;
    private GameMechanics gameMechanics;
    private enum State
    {
        // Idle: nothing happening 
        // Capture: attacker getting the flag
        // Stalemate: same/less amount of attackers & defenders
        // Return: defender returning the flag / attacker returning the flag
        Idle, Capture, Stalemate, Return
    };

    [SerializeField]
    private State currentState;
    private Renderer fieldRenderer;
    void Start()
    {
        PV = GetComponent<PhotonView>();
        hasFlag = true;
        //flag = transform.Find("Ball").gameObject;
        //detectionField = transform.Find("DetectionField").gameObject;
        gameMechanics = GameMechanics.gameMechanics;
        if (gameMechanics == null) return;

        float distanceRed = Vector3.Distance(transform.position, gameMechanics.bases[0].transform.position);
        float distanceBlue = Vector3.Distance(transform.position, gameMechanics.bases[1].transform.position);

        if (distanceRed > distanceBlue) defendTeam = 0;
        else defendTeam = 1;

        gameMechanics.flagObjectives[defendTeam] = new GameMechanics.FlagObjective(gameObject);

        currentState = State.Idle;
        fieldRenderer = detectionField.GetComponent<Renderer>();
    }

    State EvaluateState()
    {
        numOfDefenders = gameMechanics.flagObjectives[defendTeam].numOfDefenders;
        numOfAttackers = gameMechanics.flagObjectives[defendTeam].numOfAttackers;

        // the defending team has the flag
        if (hasFlag)
        {
            // first check if a defender has the other team's flag, if they have, score a point 
            // foreach (var player in playerList)
            // {
            //     if (player.team == defendTeam && player.obj.transform.Find("Flag") != null)
            //     {
            //         return State.Return;
            //     };
            // }
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
            foreach (var pair in playerList)
            {
                if (pair.obj.transform.Find("Flag") != null)
                {
                    return State.Return;
                };
            }
            return State.Idle;
        }
    }

    public void RPC_ApplyChangesOnState(int playerID)
    {
        PV.RPC("ApplyChangesOnState", RpcTarget.All, playerID);
    }

    // based on the current state, apply changes
    [PunRPC]
    void ApplyChangesOnState(int playerID)
    {
        currentState = EvaluateState();
        switch (currentState)
        {
            case State.Idle:
                fieldRenderer.material = idleMaterial;
                StopAllCoroutines();
                break;
            case State.Capture:
                fieldRenderer.material = captureMaterial;
                // start the capture counter
                StartCoroutine(StartCaptureCountDown(captureDuration, playerID));
                break;
            case State.Return:
                fieldRenderer.material = returnMaterial;
                break;
            case State.Stalemate:
                fieldRenderer.material = stalemateMaterial;
                StopAllCoroutines();
                break;
        }
    }

    IEnumerator StartCaptureCountDown(float time, int playerID)
    {
        yield return new WaitForSeconds(time);
        hasFlag = false;
        gameMechanics.RPC_EnableFlagHolder(playerID);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsConnected) return;
        if (!PV.IsMine) return;
        // track players as they enter the detection field
        if (other.gameObject.CompareTag("Player") && !playerList.Exists(player => player.obj == other.gameObject))
        {
            GameObject playerEntered = other.gameObject;

            int playerID = playerEntered.GetComponent<Movement>().GetId();
            int teamID = gameMechanics.checkTeam(playerID);

            playerList.Add(new GameMechanics.Player { team = teamID, obj = other.gameObject });
            // attacking
            if (teamID != defendTeam)
            {
                gameMechanics.RPC_UpdateAttackers(defendTeam, true);
            }
            // defending
            else
            {
                gameMechanics.RPC_UpdateDefenders(teamID, true);
                if (playerEntered.GetComponent<FlagHolder>().enabled)
                {
                    gameMechanics.RPC_DisableFlagHolder(playerID);
                    gameMechanics.RPC_UpdateFlag(defendTeam, true);

                }
            }
            RPC_ApplyChangesOnState(playerID);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!PV.IsMine) return;
        // Untrack players as they leave the detection field
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject playerExited = other.gameObject;
            int playerID = other.gameObject.GetComponent<Movement>().GetId();
            int teamID = gameMechanics.checkTeam(playerID);
            if (teamID != defendTeam)
            {
                gameMechanics.RPC_UpdateAttackers(defendTeam, false);
            }
            else
            {
                gameMechanics.RPC_UpdateDefenders(defendTeam, false);
            }
            playerList.RemoveAll(player => player.obj == other.gameObject);
            RPC_ApplyChangesOnState(playerID);
        }
    }

}
