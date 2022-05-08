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
    GameObject enemyObjective;
    GameObject ourObjective;
    void Start()
    {
        gameMechanics = GameMechanics.gameMechanics;
        enemyTeam = gameMechanics.GetLocalPlayerTeam() == 0 ? 1 : 0;
        enemyGens = enemyTeam == 0 ? gameMechanics.redgens : gameMechanics.greengens;
        enemyObjective = gameMechanics.flagObjectives[enemyTeam].objective;
        ourObjective = gameMechanics.flagObjectives[gameMechanics.GetLocalPlayerTeam()].objective;

        int i = 0;
        foreach (Transform child in transform)
        {
            if (i == 3)
            {
                child.GetComponent<CinemachineVirtualCamera>().Follow = enemyObjective.transform;
                child.GetComponent<CinemachineVirtualCamera>().LookAt = enemyObjective.transform;
            }
            else if (i == 4)
            {
                return;
            }
            else
            {
                child.GetComponent<CinemachineVirtualCamera>().Follow = enemyGens[i].transform;
                child.GetComponent<CinemachineVirtualCamera>().LookAt = enemyGens[i].transform;
            }
            i++;
        }

    }
}
