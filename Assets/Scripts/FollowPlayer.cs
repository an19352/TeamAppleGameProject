using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [HideInInspector]
    public Transform player;

    public Vector3 offset;
    public float Zoffset;

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        Vector3 newpos = new Vector3(player.position.x - offset.x, player.position.y - offset.y, player.position.z - offset.z);
        transform.position = newpos;

        transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z + Zoffset));
    }
}
