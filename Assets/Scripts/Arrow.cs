using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

    private GameMechanics gameMechanics;
    private int teamId;
    // Start is called before the first frame update
    void Start()
    {
        this.gameMechanics = GameMechanics.gameMechanics;
        int playerId = gameObject.GetComponent<Movement>().GetId();
        teamId = gameMechanics.checkTeam(playerId);
    }

    // Update is called once per frame
    void Update()
    {
        float dist1;
        float dist2;
        float dist3;
        var dist = new Dictionary<float, GameObject>();

        if (teamId == 0)
        {
            flag = GameObject.Find("StartingBoardGreen");
        }
        else
        {
            flag = GameObject.Find("StartingBoardRed");

        }


        if (generator1 != null)
        {
            dist1 = Vector3.Distance(player.transform.position, generator1.transform.position);
            dist.Add(dist1, generator1);
        }

        if (generator2 != null)
        {
            dist2 = Vector3.Distance(player.transform.position, generator2.transform.position);
            dist.Add(dist2, generator2);
        }

        if (generator3 != null)
        {
            dist3 = Vector3.Distance(player.transform.position, generator3.transform.position);
            dist.Add(dist3, generator3);
        }

        var list = dist.Keys.ToList();
        if (list.Count == 0)
        {
            generator = flag;
        }
        else
        {
            list.Sort();
            generator = dist[list[0]];
        }

        Vector3 dir = player.transform.InverseTransformPoint(generator.transform.position);
        float a = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        a += 180;
        arrow.transform.localEulerAngles = new Vector3(0, 180, a);
    }

    public void getgens(List<GameObject> gens)
    {
        generator1 = gens[0];
        generator2 = gens[1];
        generator3 = gens[2];
    }
}
