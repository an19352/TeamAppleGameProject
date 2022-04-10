using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FlagHolder : MonoBehaviour
{
    // stores which team's flag it is holding

    // public int flagTeam;
    // public int playerTeam;
    public Transform droppedBall;
    PhotonView PV;
    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    void OnDisable()
    {
        transform.Find("Ball").gameObject.SetActive(false);
    }
    void OnEnable()
    {
        transform.Find("Ball").gameObject.SetActive(true);
    }

    public void RespawnFlag(Vector3 dropPosition, Quaternion dropRotation)
    {

        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Ground");

        Transform respawnFLagPlatform = FindClosestDistance(platforms, dropPosition);

        Vector3 respawnFlagPosition = new Vector3(respawnFLagPlatform.position.x, respawnFLagPlatform.position.y + 10, respawnFLagPlatform.position.z);
        Quaternion respawnFLagRotation = respawnFLagPlatform.transform.rotation;

        // PV.RPC("InstantiateFlag", RpcTarget.All, respawnFLagPosition, respawnFLagRotation);
        PhotonNetwork.Instantiate(droppedBall.name, respawnFlagPosition, respawnFLagRotation);
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
}
