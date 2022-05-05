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

    public MeshRenderer glowPart;

    // Start is called before the first frame update
    void Start()
    {
        PV = this.GetComponent<PhotonView>();
        gameMechanics = GameMechanics.gameMechanics;
        // Transform canvas = this.gameObject.transform.Find("Canvas");
        healthBarImage = healthBar.gameObject.GetComponent<Image>();
        if (GameMechanics.gameMechanics == null) this.enabled = false;

        float redDistance = Vector3.Distance(transform.position, gameMechanics.bases[0].transform.position);
        float blueDistance = Vector3.Distance(transform.position, gameMechanics.bases[1].transform.position);

        if (redDistance < blueDistance)
        {
            //Debug.Log("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< red");
            gameMechanics.redgens.Add(gameObject);
            fsScript = gameMechanics.bases[0].GetComponentInChildren<ForceShield>();
            team = 0;
        }
        else 
        {
            //Debug.Log("<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< blue");
            gameMechanics.greengens.Add(gameObject);
            fsScript = gameMechanics.bases[1].GetComponentInChildren<ForceShield>();
            team = 1; 
        }

        forceShield = fsScript.gameObject;
        glowPart.material = glowPart.materials[team];
    }


    // Update is called once per frame
    void Update()
    {
        // if (!PV.IsMine) return;
        if (healthRemain <= 0)
        {
            //Debug.Log("some ");
            // Debug.Log(fsScript.generatorDestroyed);
            PV.RPC("RememberMe", RpcTarget.AllBuffered);
            //PhotonNetwork.Destroy(this.gameObject);
            CreateExplosion();
        }
    }


    void CreateExplosion()
    {
        GameObject explosion = PhotonNetwork.Instantiate(explosionEffect.name, transform.position, transform.rotation);
        NotifyNearbyPlayers();
        Debug.Log("exploded");
        // Debug.Log("explosion instantiated");

        // RepelNearbyPlayers();
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
        int f = 0;
        foreach (GameObject gen in GameMechanics.gameMechanics.greengens)
        {
            if (gen.transform == gameObject.transform)
            {
                f = 1;
                break;
            }
        }

        if (other.gameObject.TryGetComponent<Movement>(out Movement mov))
        {

            int id = mov.GetId();
            int teamid = GameMechanics.gameMechanics.checkTeam(id);

            if (teamid != f)
            {

                ContactPoint cp = other.GetContact(0);
                Vector3 collisionVelocity = other.relativeVelocity;
                Vector3 collisionNormal = cp.normal;
                float mass = other.collider.attachedRigidbody.mass;
                float force = Mathf.Abs(Vector3.Dot(cp.normal, collisionVelocity)) * mass;

                healthRemain -= force;
                float fraction = healthRemain / health;
                healthBarImage.fillAmount = fraction;
            }
            else return;
        }
    }

    public void applyForce(float force)
    {
        healthRemain -= force;
        float fraction = healthRemain / health;
        healthBarImage.fillAmount = fraction;
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

    void RepelNearbyPlayers()
    {
        Collider[] playersInRadius = Physics.OverlapSphere(transform.position, explosionRadius, players, 0);
        foreach (Collider player in playersInRadius)
        {
            Debug.Log(player);
            Vector3 pushFactor = (player.transform.position - transform.position).normalized * pushForce;
            Debug.Log(pushFactor);
            player.GetComponent<Movement>().RPC_PushMe(pushFactor, ForceMode.Impulse);
        }
    }
    
    void NotifyNearbyPlayers()
    {
        Collider[] playersInRadius = Physics.OverlapSphere(transform.position, explosionRadius, players, 0);
        foreach (Collider player in playersInRadius)
        {
            Player[] target = {player.GetComponent<PhotonView>().Owner};
            PlaySound.playSound.RPC_QueueVoice(19, target);
            Debug.Log(player);
        }
    }

    [PunRPC]
    void RememberMe()
    {
        fsScript.generatorDestroyed++;
        gameObject.SetActive(false);
    }

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
