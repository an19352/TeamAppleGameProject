using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public class Minimap : MonoBehaviour
{
    public Transform playerIcon;
    public Transform generatorIcon;

    private Transform playerIconRef;

    private LineRenderer lineRenderer;
    private GameObject player;
    private List <GameObject> generators;
    PhotonView PV;
    GameMechanics gameMechanics;


    // Start is called before the first frame update
    void OnEnable()
    {
        // find the transform of the player whoes owner is the same as the camera
        this.PV = GetComponent<PhotonView>();
        this.gameMechanics = GameMechanics.gameMechanics;
        lineRenderer = GetComponent<LineRenderer>();
        playerIconRef = Instantiate(playerIcon, transform.position, transform.rotation);
        generators = gameMechanics.greengens;
        generators.AddRange(gameMechanics.redgens);
        foreach (GameObject generator in generators)
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
