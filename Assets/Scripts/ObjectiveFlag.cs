using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class ObjectiveFlag : MonoBehaviour
{
    PhotonView PV;
    public int defendTeam;

    public int otherTeam;
    public bool hasFlag;
    // set to the same value in the inspector
    public float captureDuration;
    public float timerInterval;
    public Material idleMaterial;
    public Material captureMaterial;
    public Material stalemateMaterial;
    public Material returnMaterial;
    // red <-> green
    // public GameObject otherObjectiveFlag;

    public List<GameMechanics.Player> playerList;
    public int numOfAttackers = 0, numOfDefenders = 0;
    public int firstEnteredId;

    private Image fill;
    private Text contested;
    private Text capturing;
    public GameObject captureTimer;
    private bool hasAlreadyStarted = false;
    private GameObject timer;

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
                if (hasAlreadyStarted)
                {
                    Destroy(timer);
                }
                StopAllCoroutines();
                break;
            case State.Capture:
                fieldRenderer.material = captureMaterial;
                // start the capture counter
                if (PhotonRoom.room.team == otherTeam)
                {
                    StartCoroutine(StartTimerFill(captureDuration, timerInterval));
                }
                StartCoroutine(StartCaptureCountDown(captureDuration, playerID, defendTeam));
                break;
            case State.Stalemate:
                if (PhotonRoom.room.team == otherTeam)
                {
                    ContestedTimer();
                }
                fieldRenderer.material = stalemateMaterial;
                // stop the progress bar
                StopAllCoroutines();
                break;
        }
    }

    IEnumerator StartCaptureCountDown(float time, int playerID, int defendTeam)
    {
        yield return new WaitForSeconds(time);
        GameMechanics.Player firstPlayerEntered = playerList.Find(player => player.team == otherTeam);
        int firstPlayerId = firstPlayerEntered.obj.GetComponent<Movement>().GetId();
        if (!firstPlayerEntered.obj.GetComponent<FlagHolder>().enabled)
        {
            gameMechanics.RPC_EnableFlagHolder(firstPlayerId, defendTeam);
            gameMechanics.RPC_DecreaseFlag(defendTeam);
        };

    }

    IEnumerator StartTimerFill(float time, float interval)
    {
        if (hasAlreadyStarted)
        {
            Destroy(timer);
        }
        hasAlreadyStarted = true;
        timer = Instantiate(captureTimer, InventoryUIManager.inventory.transform.parent);
        fill = timer.transform.GetChild(1).GetComponent<Image>();
        contested = timer.transform.GetChild(3).GetComponent<Text>();
        capturing = timer.transform.GetChild(4).GetComponent<Text>();
        fill.color = capturing.color;
        fill.fillAmount = 0f;
        contested.gameObject.SetActive(false);
        float totalIncrements = time / interval;
        float amountIncremented = 1 / totalIncrements;
        for (int i = 0; i < totalIncrements; i++)
        {
            fill.fillAmount += amountIncremented;
            yield return new WaitForSeconds(interval);
        }
        Destroy(timer);
        hasAlreadyStarted = false;
        yield return null;
    }

    void ContestedTimer()
    {
        if (hasAlreadyStarted)
        {
            capturing.gameObject.SetActive(false);
            contested.gameObject.SetActive(true);
            fill.color = contested.color;
        }
        else
        {
            timer = Instantiate(captureTimer, InventoryUIManager.inventory.transform.parent);
            hasAlreadyStarted = true;
            fill = timer.transform.GetChild(1).GetComponent<Image>();
            contested = timer.transform.GetChild(3).GetComponent<Text>();
            capturing = timer.transform.GetChild(4).GetComponent<Text>();
            fill.color = contested.color;
            capturing.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsConnected) return;
        if (PV == null) return;
        if (!PhotonNetwork.IsMasterClient) return;

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
                    int origin = playerEntered.GetComponent<FlagHolder>().ballOrigin;
                    gameMechanics.RPC_DisableFlagHolder(playerID);
                    Player[] target = { playerEntered.GetComponent<PhotonView>().Owner };
                    PlaySound.playSound.RPC_QueueVoice(18, target);
                    // scoring an enemy flag
                    // if (playerEntered.GetComponent<FlagHolder>().teamID != defendTeam)
                    // {
                    gameMechanics.RPC_IncreaseFlag(defendTeam, origin);


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
        if (!PhotonNetwork.IsMasterClient) return;

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
