using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ObjectiveFlag : MonoBehaviour
{
    PhotonView PV;
    public int defendTeam;

    public int otherTeam;
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
        Idle, Capture, Stalemate
    };

    [SerializeField]
    private State currentState;
    private Renderer fieldRenderer;
    void Start()
    {
        PV = GetComponent<PhotonView>();
        hasFlag = true;
        gameMechanics = GameMechanics.gameMechanics;
        if (gameMechanics == null) return;

        float distanceRed = Vector3.Distance(transform.position, gameMechanics.bases[0].transform.position);
        float distanceBlue = Vector3.Distance(transform.position, gameMechanics.bases[1].transform.position);

        if (distanceRed > distanceBlue) defendTeam = 1;
        else defendTeam = 0;

        gameMechanics.flagObjectives[defendTeam] = new GameMechanics.FlagObjective(gameObject);

        currentState = State.Idle;
        fieldRenderer = detectionField.GetComponent<Renderer>();

        otherTeam = defendTeam == 0 ? 1 : 0;
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
            return State.Idle;
        }
    }

    public void RPC_ApplyChangesOnState(int playerID)
    {
        PV.RPC("ApplyChangesOnState", RpcTarget.MasterClient, playerID);
    }

    // based on the current state, apply changes
    [PunRPC]
    void ApplyChangesOnState(int playerID)
    {
        currentState = EvaluateState();
        //Debug.Log(currentState);
        switch (currentState)
        {
            case State.Idle:
                fieldRenderer.material = idleMaterial;
                StopAllCoroutines();
                break;
            case State.Capture:
                fieldRenderer.material = captureMaterial;
                // start the capture counter
                StartCoroutine(StartCaptureCountDown(captureDuration, playerID, defendTeam));
                break;
            case State.Stalemate:
                fieldRenderer.material = stalemateMaterial;
                StopAllCoroutines();
                break;
        }
    }

    IEnumerator StartCaptureCountDown(float time, int playerID, int defendTeam)
    {
        yield return new WaitForSeconds(time);
        GameMechanics.Player firstPlayerEntered = playerList.Find(player => player.team == otherTeam);
        int firstPlayerId = firstPlayerEntered.obj.GetComponent<Movement>().GetId();
        gameMechanics.RPC_EnableFlagHolder(firstPlayerId);
        gameMechanics.RPC_DecreaseFlag(defendTeam);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsConnected) return;
        if (PV == null) return;
        if (!PV.IsMine) return;

        // track players as they enter the detection field
        if (other.gameObject.CompareTag("Player") && !playerList.Exists(player => player.obj == other.gameObject))
        {
            GameObject playerEntered = other.gameObject;

            int playerID = playerEntered.GetComponent<Movement>().GetId();
            int teamID = gameMechanics.checkTeam(playerID);
            playerList.Add(new GameMechanics.Player { team = teamID, obj = other.gameObject });

            // an enemy enter 
            if (teamID != defendTeam)
            {
                gameMechanics.RPC_UpdateAttackers(defendTeam, true);
            }
            // a friendly player enter 
            else
            {
                gameMechanics.RPC_UpdateDefenders(teamID, true);
                // if the friendly player enters with a flag
                if (playerEntered.GetComponent<FlagHolder>().enabled)
                {
                    gameMechanics.RPC_DisableFlagHolder(playerID);
                    Player[] target = { playerEntered.GetComponent<PhotonView>().Owner };
                    PlaySound.playSound.RPC_QueueVoice(18, target);
                    // scoring an enemy flag
                    // if (playerEntered.GetComponent<FlagHolder>().teamID != defendTeam)
                    // {
                    gameMechanics.RPC_IncreaseFlag(defendTeam);
                    // }
                    // return a friendly flag
                    // else { gameMechanics.RPC_UpdateFlag(defendTeam, false); }
                }
            }
            RPC_ApplyChangesOnState(playerID);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!PhotonNetwork.IsConnected) return;
        if (PV == null) return;
        if (!PV.IsMine) return;

        // Untrack players as they leave the detection field
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject playerExited = other.gameObject;
            int playerID = other.gameObject.GetComponent<Movement>().GetId();
            int teamID = gameMechanics.checkTeam(playerID);
            // an enemy player exits 
            if (teamID != defendTeam)
            {
                gameMechanics.RPC_UpdateAttackers(defendTeam, false);
            }
            // a friendly player exits 
            else
            {
                gameMechanics.RPC_UpdateDefenders(defendTeam, false);
            }
            // remove the player exit from the list
            playerList.RemoveAll(player => player.obj == other.gameObject);
            RPC_ApplyChangesOnState(playerID);
        }
    }

}
