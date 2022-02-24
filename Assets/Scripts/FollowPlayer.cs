using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [HideInInspector]
    public Transform player;

    public Vector2 offset;

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        Vector3 newpos = new Vector3(player.position.x - offset.x, transform.position.y, player.position.z - offset.y);
        transform.position = newpos;
    }
}
