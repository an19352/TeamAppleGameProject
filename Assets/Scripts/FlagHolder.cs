using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: needs to identify which team's flag it is holding 

public class FlagHolder : MonoBehaviour
{
    // stores which team's flag it is holding
    // public int flagTeam;
    // public int playerTeam;

    // Start is called before the first frame update
    void Start()
    {
        transform.Find("Flag").gameObject.SetActive(true);
        // flagTeam = gameObject.GetComponent<Movement>().GetId();
    }

    void OnDisable()
    {
        transform.Find("Flag").gameObject.SetActive(false);
    }
}
