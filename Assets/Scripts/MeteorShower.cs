using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

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
        meteor = poolOfObject.SpawnFromPool(meteorTags[randMet], randPositionSpawn, Quaternion.identity);
        Vector3 pushFactor = ((randPositionTarget - randPositionSpawn).normalized) * meteorFallForce;
        meteor.GetComponent<Rigidbody>().AddForce(pushFactor, ForceMode.Impulse);
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
        spawnLoc.y += 10;
        spawnLoc.x += 2;
        spawnLoc.z += 2;
        
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
