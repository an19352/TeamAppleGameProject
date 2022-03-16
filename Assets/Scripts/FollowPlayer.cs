using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [HideInInspector]
    public Transform player;

    public Vector3 offset;
    public float Zoffset;
    public bool smoothMotion = false;
    public float smoothSpeed = 1.25f;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (player == null) return;

        if (smoothMotion)
            MoveCameraSmoothly();
        else
            MoveCamera();
    }

    void MoveCamera() 
    {
        Vector3 newpos = new Vector3(player.position.x - offset.x, player.position.y - offset.y, player.position.z - offset.z);
        transform.position = newpos;

        transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z + Zoffset));
    }

    void MoveCameraSmoothly()
    {
        Vector3 newpos = new Vector3(player.position.x - offset.x, player.position.y - offset.y, player.position.z - offset.z);
        Vector3 smoothPosition = Vector3.Lerp(transform.position, newpos, smoothSpeed * Time.deltaTime);
        transform.position = smoothPosition;

        transform.LookAt(new Vector3(player.position.x, player.position.y, player.position.z + Zoffset));
    }
}
