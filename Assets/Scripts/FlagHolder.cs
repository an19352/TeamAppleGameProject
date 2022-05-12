using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using Photon.Pun;
using Random = System.Random;

public class FlagHolder : MonoBehaviour
{
    // This script sits on the player and gets activated when he picks up the flag
    
    public int teamID;
    public int ballOrigin;  // stores which team's flag it is holding
    // public int playerTeam;
    public Transform droppedBall;
    public GameObject ball; // visual representation of the flag
    ObjectPooler poolOfObject;
    PhotonView PV;
    
    void Start()
    {
        PV = GetComponent<PhotonView>();
        poolOfObject = ObjectPooler.OP;

        teamID = GameMechanics.gameMechanics.checkTeam(GetComponent<Movement>().GetId());
    }

    void OnDisable()
    {
        ball.SetActive(false);
    }
    void OnEnable()
    {
        ball.SetActive(true);
    }

    // Drop the flag in the center of the closest platform, called upon the player's death
    public void RespawnFlag(Vector3 dropPosition, Quaternion dropRotation)
    {
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Ground");

        Transform respawnFLagPlatform = FindClosestDistance(platforms, dropPosition);
        Vector3 respawnFlagPosition = new Vector3(respawnFLagPlatform.position.x, respawnFLagPlatform.position.y + 10, respawnFLagPlatform.position.z);
        Quaternion respawnFLagRotation = Quaternion.identity;
        PV.RPC("RPC_SpawnDroppedFlag", RpcTarget.All, respawnFlagPosition, respawnFLagRotation);
        poolOfObject.SpawnFromPool("DroppedFlag", respawnFlagPosition, respawnFLagRotation).GetComponent<BallRecover>().teamID = ballOrigin;
        PlaySound.playSound.RPC_QueueVoice(GenerateCommentary(teamID), PhotonNetwork.PlayerList);
    }

    [PunRPC] // Drop the flag for everyone
    public void RPC_SpawnDroppedFlag(Vector3 respawnFlagPosition, Quaternion respawnFLagRotation)
    {
        GameMechanics.gameMechanics.drop = respawnFlagPosition;
        //Debug.Log(respawnFlagPosition);
    }

    // Find the closest platform
    Transform FindClosestDistance(GameObject[] platforms, Vector3 respawnFlagPosition)
    {

        GameObject closestPlatform = platforms[0];
        float minDistance = Vector3.Distance(respawnFlagPosition, platforms[0].transform.position);
        foreach (GameObject platform in platforms)
        {
            float dis = Vector3.Distance(respawnFlagPosition, platform.transform.position);
            // compare distance between the drop point and each of the platforms, find the closest one
            if (dis < minDistance)
            {
                minDistance = dis;
                closestPlatform = platform;
            }
        }
        return closestPlatform.transform;
    }

    // Play the right audio
    int GenerateCommentary(int teamNo)
    {
        Random ran = new Random();
        int commID;
        if (teamNo == 0)
        {
            commID = ran.Next(14, 17);
        }
        else
        {
            commID = ran.Next(11, 14);
        }

        return commID;
    }
}
