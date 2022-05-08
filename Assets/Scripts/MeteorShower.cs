using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Random = System.Random;

public class MeteorShower : MonoBehaviour
{
    //This script is currently being tested and does not yet need to be properly implemented
    ObjectPooler poolOfObject;
    
    private GameObject meteor;
    private List<string> meteorTags;
    private PhotonView PV;
    private Vector3 mouseLocation;

    public LayerMask ignoredLayers;
    private Camera cameraMain;

    public int meteorFallForce;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        poolOfObject = ObjectPooler.OP;
        meteorTags = new List<string>();
        cameraMain = Camera.main;
        
        SetMeteorTags();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            //StartCoroutine(SpawnMeteor());
            SpawnMeteor();
        }
    }
    
    [PunRPC]
    void GenerateMeteor(int randMet, Vector3 randPositionSpawn, Vector3 randPositionTarget)
    {
        if (PV.IsMine)
        {
            meteor = poolOfObject.SpawnFromPool(meteorTags[randMet], randPositionSpawn, Quaternion.identity);
            Vector3 pushFactor = ((randPositionTarget - randPositionSpawn).normalized) * meteorFallForce;
            meteor.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
            meteor.GetComponent<Rigidbody>().AddForce(pushFactor, ForceMode.Impulse);
        }
    }

    void SpawnMeteor()
    {
        Ray mouseRay = cameraMain.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mouseRay, out RaycastHit hit, 1000f, ~ignoredLayers))
        {
            mouseLocation = hit.point;
            Debug.Log(mouseLocation);
        }

        Vector3 spawnLoc = mouseLocation;
        Vector3 newPos = RandomSpawnPosition();
        spawnLoc.y += newPos.y;
        spawnLoc.x += newPos.x;
        spawnLoc.z += newPos.z;
        
        Debug.Log(meteorTags.Count);
        PV.RPC("GenerateMeteor", RpcTarget.All, 0, spawnLoc, mouseLocation);

    }
    
    public void SetMeteorTags()
    {
        foreach (ObjectPooler.Pool pool in poolOfObject.pools)
        {
            if (pool.prefab.CompareTag("Meteor"))
                meteorTags.Add(pool.tag);
        }
    }

    public Vector3 RandomSpawnPosition()
    {
        Vector3 RanSpawn;
        Random ran = new Random();
        RanSpawn.x = ran.Next(-2, 3);
        RanSpawn.z = ran.Next(-2, 3);
        RanSpawn.y = 10;

        return RanSpawn;
    }
    
    /*IEnumerator SpawnMeteor()
    {
        Ray mouseRay = cameraMain.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mouseRay, out RaycastHit hit, 1000f, ~ignoredLayers))
        {
            if (hit.transform.CompareTag("Ground"))
            {
                Vector3 mouseLocation = hit.point;
                lookDirection = (mouseLocation - player.position).normalized;
                lookRotation = Quaternion.LookRotation(lookDirection);
                lookRotation.x = 0f;
                lookRotation.z = 0f;
            }
        }
    }*/
    
}
