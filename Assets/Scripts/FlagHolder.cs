using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using Photon.Pun;
using Random = System.Random;

public class FlagHolder : MonoBehaviour
{
    // stores which team's flag it is holding

    public int teamID;
    // public int playerTeam;
    public Transform droppedBall;
    public GameObject ball;
    ObjectPooler poolOfObject;
    PhotonView PV;
    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        poolOfObject = ObjectPooler.OP;
    }

    void OnDisable()
    {
        ball.SetActive(false);
    }
    void OnEnable()
    {
        ball.SetActive(true);
    }

    public void RespawnFlag(Vector3 dropPosition, Quaternion dropRotation)
    {
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Ground");

        Transform respawnFLagPlatform = FindClosestDistance(platforms, dropPosition);
        Vector3 respawnFlagPosition = new Vector3(respawnFLagPlatform.position.x, respawnFLagPlatform.position.y + 10, respawnFLagPlatform.position.z);
        Quaternion respawnFLagRotation = Quaternion.identity;
        PV.RPC("RPC_SpawnDroppedFlag", RpcTarget.All, respawnFlagPosition, respawnFLagRotation);
        PlaySound.playSound.RPC_QueueVoice(GenerateCommentary(teamID), PhotonNetwork.PlayerList);
    }
    [PunRPC]
    public void RPC_SpawnDroppedFlag(Vector3 respawnFlagPosition, Quaternion respawnFLagRotation)
    {
        poolOfObject.SpawnFromPool("DroppedFlag", respawnFlagPosition, respawnFLagRotation);
    }


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
