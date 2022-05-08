using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Cutscene : MonoBehaviour
{
    // Start is called before the first frame update
    int enemyTeam;
    GameMechanics gameMechanics;
    List<GameObject> enemyGens;
    void Start()
    {
        gameMechanics = GameMechanics.gameMechanics;
        enemyTeam = gameMechanics.GetLocalPlayerTeam() == 0 ? 1 : 0;
        enemyGens = enemyTeam == 0 ? gameMechanics.redgens : gameMechanics.greengens;
        int i = 0;
        foreach (Transform child in transform)
        {
            child.GetComponent<CinemachineVirtualCamera>().Follow = enemyGens[i].transform;
            child.GetComponent<CinemachineVirtualCamera>().LookAt = enemyGens[i].transform;
            i++;
        }
    }
}
