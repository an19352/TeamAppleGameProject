using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Minimap : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private GameObject player;
    PhotonView PV;
    GameMechanics gameMechanics;

    // Start is called before the first frame update
    void Start()
    {
        // find the transform of the player whoes owner is the same as the camera
        this.PV = GetComponent<PhotonView>();
        this.gameMechanics = GameMechanics.gameMechanics;
        lineRenderer = GetComponent<LineRenderer>();

    }

    void Update()
    {
        if (player != null) return;
        player = gameMechanics.GetLocalPlayer();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 tempVector = player.transform.position;
            float tempX = player.transform.position.x;
            float tempZ = player.transform.position.z;
            transform.position = new Vector3(tempX, 80, tempZ);
            // TODO: add some ui indication of players pos and stuff
            //     lineRenderer.SetPositions(new Vector3[] {
            //     transform.position,
            //      transform.position,
            // });
        }
    }
}
