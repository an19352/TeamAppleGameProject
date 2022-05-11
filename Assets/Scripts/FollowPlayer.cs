using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    // This is the camera script that follows the player

    //[HideInInspector]
    public Transform player;

    [Tooltip("Where will the camera be relative to the player")]
    public Vector3 offset;
    [Tooltip("Where will the camera be pointed relative to the player")]
    public Vector3 focalOffsets;
    [Tooltip("Makes the camera lag behind a tiny bit")]
    public bool smoothMotion = false;  
    public float smoothSpeed = 1.25f;
    public float rubberBand = 1f;
    public bool attachToPlayer = false;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (player == null) return;

        if (attachToPlayer)
        {
            transform.parent = player;
            offset = player.position - transform.position;
        }
        else
        {
            transform.parent = null;

            if (smoothMotion)
                MoveCameraSmoothly();
            else
                MoveCamera();
        }
    }

    // Just stifly follow the player
    void MoveCamera() 
    {
        Vector3 newpos = new Vector3(player.position.x - offset.x, player.position.y - offset.y, player.position.z - offset.z);
        transform.position = newpos;

        transform.LookAt(player.position + focalOffsets);
    }

    // Follow the player but with the smoothSpeed
    void MoveCameraSmoothly()
    {
        Vector3 newpos = new Vector3(player.position.x - offset.x, player.position.y - offset.y, player.position.z - offset.z);
        Vector3 smoothPosition = Vector3.Lerp(transform.position, newpos, smoothSpeed * Time.deltaTime);
        transform.position = smoothPosition;

        transform.LookAt(player.position + focalOffsets);
    }
}
