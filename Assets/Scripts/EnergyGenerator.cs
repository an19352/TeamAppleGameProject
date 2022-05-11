using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Realtime;

public class EnergyGenerator : MonoBehaviour, IPunObservable
{
    // Finds out which team this generator should be on, registers the hits it takes and synchronises it over the network
    PhotonView PV;
    GameMechanics gameMechanics;
    public GameObject forceShield;

    [Header("Can mess with")]
    public int team;
    public float health = 50f;
    public float healthRemain = 50f;
    public GameObject explosionEffect;
    public float explosionRadius = 2;
    public float pushForce = 5;
    // times/per second
    public float fireCountDown = 0;

    [Header("Do not mess with")]
    public Transform healthBar;

    public LayerMask players;

    private ForceShield fsScript;
    private Image healthBarImage;
    private GameObject mapgen;
    public MeshRenderer glowPart;

    // Start is called before the first frame update
    void Start()
    {
        PV = this.GetComponent<PhotonView>();
        gameMechanics = GameMechanics.gameMechanics;
        mapgen = gameMechanics.MapGenerator;
        // Transform canvas = this.gameObject.transform.Find("Canvas");
        healthBarImage = healthBar.gameObject.GetComponent<Image>();
        if (GameMechanics.gameMechanics == null) this.enabled = false;

        Vector3 midpos = mapgen.GetComponent<MapGenerator>().FindMiddle(); // Finds the coordonates of the middle platform
        //float redDistance = Vector3.Distance(transform.position, gameMechanics.bases[0].transform.position);
        //float blueDistance = Vector3.Distance(transform.position, gameMechanics.bases[1].transform.position);

        if (midpos.x < transform.position.x) 
        {
            gameMechanics.redgens.Add(gameObject);
            fsScript = gameMechanics.bases[0].GetComponentInChildren<ForceShield>();
            team = 0; // Be red if you're on the left
        }
            
        else 
        {
            gameMechanics.greengens.Add(gameObject);
            fsScript = gameMechanics.bases[1].GetComponentInChildren<ForceShield>();
            team = 1; // or blue if you're on the right
        }

        forceShield = fsScript.gameObject;
        glowPart.material = glowPart.materials[team];
    }


    // Update is called once per frame
/*    void Update()
    {
        // if (!PV.IsMine) return;
        if (healthRemain <= 0)
        {
            //Debug.Log("some ");
            // Debug.Log(fsScript.generatorDestroyed);
            PV.RPC("RememberMe", RpcTarget.All);
            //PhotonNetwork.Destroy(this.gameObject);
            CreateExplosion();
        }
    }*/


    void CreateExplosion()
    {
        GameObject explosion = PhotonNetwork.Instantiate(explosionEffect.name, transform.position, transform.rotation);
        
        // // yield return new WaitForSeconds(2);
        // PhotonNetwork.Destroy(explosion);
        // Debug.Log("destroy explosion instantiated");
    }


    /*
    If you just want a measurement of how strong the hit was (like, for example for damage calculations),
     the dot product of collision normal and collision velocity (ie the velocity of the two bodies relative to each other),
     times the mass of the other collider should give you useful values.
     */
    void OnCollisionEnter(Collision other)
    {

        if (other.gameObject.TryGetComponent<Movement>(out Movement mov)) // Only players have the movement script
        {

            int id = mov.GetId();
            int teamid = GameMechanics.gameMechanics.checkTeam(id);

            if (teamid != team)  // No friendly fire
            {

                ContactPoint cp = other.GetContact(0);
                Vector3 collisionVelocity = other.relativeVelocity;
                Vector3 collisionNormal = cp.normal;
                float mass = other.collider.attachedRigidbody.mass;
                float force = Mathf.Abs(Vector3.Dot(cp.normal, collisionVelocity)) * mass; // Converting the velocity to damage

                applyForce(force);
            }
            else return;
        }
    }

    private void OnDisable()
    {
        if(PV == null) return;
        fsScript.generatorDestroyed++; // Notify your shield, one more generator fell
        //Debug.Log("disable");
        NotifyNearbyPlayers();
    }

    // Take Damage
    public void applyForce(float force)
    {
        healthRemain -= force;
        float fraction = healthRemain / health;
        healthBarImage.fillAmount = fraction;

        if (healthRemain <= 0)
        {
            RepelNearbyPlayers();
            PV.RPC("RememberMe", RpcTarget.All);
            CreateExplosion();
        }
    }

      private void OnEnable()
     {
        if (forceShield == null) return;

         forceShield.SetActive(true); 
         healthRemain = health;
         healthBarImage = healthBar.gameObject.GetComponent<Image>();
         float fraction = healthRemain / health;
         healthBarImage.fillAmount = fraction;
     }

    // Repel every player in the explosionRadius, this happens on every client
    void RepelNearbyPlayers()
    {
        Debug.Log("called");
        Debug.Log(transform.position);
        Collider[] playersInRadius = Physics.OverlapSphere(transform.position, explosionRadius, ~0, 0);
        foreach (Collider player in playersInRadius)
        {
            Debug.Log(player);
            if (player.CompareTag("Player"))
            {
                Debug.Log("found");
                Vector3 pushFactor = (player.transform.position - transform.position).normalized * pushForce;
                player.GetComponent<Movement>().PushMe(pushFactor, ForceMode.Impulse, false);
            }
        }
    }
    
    //Play a sound in the explosionRadius. This only happens on the Master client
    void NotifyNearbyPlayers()
    {
        Collider[] playersInRadius = Physics.OverlapSphere(transform.position, explosionRadius, ~0, 0);
        foreach (Collider col in playersInRadius)
        {
            if (col.CompareTag("Player"))
            {
                Player[] target = { col.GetComponent<PhotonView>().Owner };
                PlaySound.playSound.RPC_QueueVoice(19, target);
            }
        }
    }

    [PunRPC]
    void RememberMe()
    {
        gameObject.SetActive(false);
    }

    // Photon uses this to synchronise whatever you want. The Player who owns the object (Photon View) is the one who constantly writes
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            if (healthRemain <= 0) healthRemain = 1;
            stream.SendNext(healthRemain);
        }
        if (stream.IsReading)
        {
            healthRemain = (float)stream.ReceiveNext();
            float fraction = healthRemain / health;
            healthBarImage.fillAmount = fraction;
        }
    }
}
