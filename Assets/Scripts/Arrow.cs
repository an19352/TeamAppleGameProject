using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Arrow : MonoBehaviour
{
    public GameObject arrow;

    public GameObject player;
    [HideInInspector]
    public GameObject generator1;
    [HideInInspector]
    public GameObject generator2;
    [HideInInspector]
    public GameObject generator3;
    private GameObject generator;
    [HideInInspector]
    public GameObject flag;

    [HideInInspector]
    public GameObject home;

    private Vector3 nothing = new Vector3(0,0,0);

    

    void Update()
    {
        if (!gameObject.GetComponent<PhotonView>().IsMine)
            return;

        float dist1;
        float dist2;
        float dist3;
        float dist4;
        Vector3 dir;
        Dictionary<float, GameObject> dist = new Dictionary<float, GameObject>();

        if (generator1 !=null && generator1.activeSelf)
        {
            dist1 = Vector3.Distance(player.transform.position, generator1.transform.position);
            dist.Add(dist1, generator1);
        }

        if (generator1 !=null && generator2.activeSelf)
        {
            dist2 = Vector3.Distance(player.transform.position, generator2.transform.position);
            dist.Add(dist2, generator2);
        }

        if (generator1 !=null && generator3.activeSelf)
        {
            dist3 = Vector3.Distance(player.transform.position, generator3.transform.position);
            dist.Add(dist3, generator3);
        }

        if (GameMechanics.gameMechanics.drop != nothing)
        {
            dist4 = Vector3.Distance(player.transform.position, GameMechanics.gameMechanics.drop);
            dist.Add(dist4, null);
        }

        List<float> list = dist.Keys.ToList();
        if (list.Count == 0)
        {
            if (gameObject.GetComponent<FlagHolder>().enabled)
            {
                generator = home;
            }
            else
            {
                generator = flag;
            }
        }
        else
        {
            list.Sort();
            generator = dist[list[0]];
        }

        if (generator == null)
        { 
            dir = player.transform.InverseTransformPoint(GameMechanics.gameMechanics.drop);
        }
        else
        {
            dir = player.transform.InverseTransformPoint(generator.transform.position);
        }
        
        float a = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        a += 180;
        arrow.transform.localEulerAngles = new Vector3(0, 180, a);
    }

    public void getgens(List<GameObject> gens, GameObject redbase, GameObject greenbase, int t)
    {
        if (t == 0)
        {
            flag = greenbase.transform.GetChild(2).gameObject;
            home = redbase.transform.GetChild(2).gameObject; 
        }
        else
        {
            flag = redbase.transform.GetChild(2).gameObject;
            home = greenbase.transform.GetChild(2).gameObject;
        }
        generator1 = gens[0];
        generator2 = gens[1];
        generator3 = gens[2];
    }
}
