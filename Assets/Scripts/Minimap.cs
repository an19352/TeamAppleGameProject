using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public class Minimap : MonoBehaviour
{
    // Handles minimap logic
    public Transform playerIcon;
    public Transform generatorIcon;

    private Transform playerIconRef;

    private LineRenderer lineRenderer;
    private GameObject player;
    List<GameObject> enemyGens;
    PhotonView PV;
    GameMechanics gameMechanics;


    void OnEnable()
    {
        // find the transform of the player whoes owner is the same as the camera
        this.PV = GetComponent<PhotonView>();
        this.gameMechanics = GameMechanics.gameMechanics;

        int enemyTeam = gameMechanics.GetLocalPlayerTeam() == 0 ? 1 : 0;
        enemyGens = enemyTeam == 0 ? gameMechanics.redgens : gameMechanics.greengens;
        lineRenderer = GetComponent<LineRenderer>();
        playerIconRef = Instantiate(playerIcon, transform.position, transform.rotation);

        foreach (GameObject generator in enemyGens)
        {
            Vector3 generatorPos = new Vector3(generator.transform.position.x, 60f, generator.transform.position.z);
            Instantiate(generatorIcon, generatorPos, generatorIcon.rotation);
        }
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
            playerIconRef.position = new Vector3(tempX, 60, tempZ);

            //     lineRenderer.SetPositions(new Vector3[] {
            //     transform.position,
            //      transform.position,
            // });
        }
    }
}
