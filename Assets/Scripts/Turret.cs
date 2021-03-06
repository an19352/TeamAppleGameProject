using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;
using Photon.Realtime;

public class Turret : MonoBehaviour, IPunObservable
{
    // Handles turret logic and damage registration
    // Turrets used to be on the team in whose half they spawned but it ended up being more fun if they just shoot at everyone

    PhotonView PV;

    [Header("Can mess with")]
    public int team;
    public float health = 50f;
    public float healthRemain = 50f;
    public GameObject explosionEffect;
    public float explosionRadius = 20;
    public float range = 15f;
    public float turnSpeed = 10f;
    public float pushForce = 5;
    // times/per second
    public float fireRate = 0.2f;
    public float fireCountDown = 0;

    [Header("Do not mess with")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Transform target;
    public Transform partToRotate;
    public Transform healthBar;

    public LayerMask players;

    private ForceShield fsScript;
    public Image healthBarImage;
    ObjectPooler poolOfObject;

    // Start is called before the first frame update
    void Start()
    {

        PV = this.GetComponent<PhotonView>();
        // Transform canvas = this.gameObject.transform.Find("Canvas");
        //healthBarImage = healthBar.gameObject.GetComponent<Image>();
        poolOfObject = ObjectPooler.OP;
        //InvokeRepeating("UpdateTarget", 0f, 0.1f);

        // Determines which team its on____ No longer used
        float distanceRed = Vector3.Distance(transform.position, GameMechanics.gameMechanics.bases[0].transform.position);
        float distanceBlue = Vector3.Distance(transform.position, GameMechanics.gameMechanics.bases[1].transform.position);

        if (distanceBlue < distanceRed) team = 1;
    }

    // Update is called once per frame
    void Update()
    {
        //if (!PV.IsMine) return;
        if (healthRemain <= 0)
        {
            NotifyNearbyPlayers();
            PV.RPC("RememberMe", RpcTarget.AllBuffered);
            //PhotonNetwork.Destroy(this.gameObject);

        }
        
        UpdateTarget(); // Check for new players
        
        if (target != null)
        {
            Vector3 direction = transform.position - target.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            Quaternion smoothRotation = Quaternion.Lerp(partToRotate.rotation, rotation, turnSpeed);
            partToRotate.rotation = Quaternion.Euler(0f, smoothRotation.eulerAngles.y, 0f);
            if (fireCountDown <= 0)
            {
                if (PV.IsMine)
                    PV.RPC("Shoot", RpcTarget.All);
                fireCountDown = 1 / fireRate;
            }
            fireCountDown -= Time.deltaTime;
        }
    }

    [PunRPC]
    void Shoot()
    {
        // Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        poolOfObject.SpawnFromPool("Bullet", firePoint.position, firePoint.rotation);
    }

    IEnumerator CreateExplosion()
    {
        GameObject explosion = PhotonNetwork.Instantiate(explosionEffect.name, transform.position, transform.rotation);
        RepelNearbyPlayers();
        yield return new WaitForSeconds(2);
        PhotonNetwork.Destroy(explosion);
    }

    // updates a few times a second, used to locate the closest enemy player in range
    void UpdateTarget()
    {
        List<GameMechanics.Player> players = new List<GameMechanics.Player>();
        foreach (GameMechanics.Player pl in GameMechanics.gameMechanics.players)
        {
            if(pl.obj != null) players.Add(pl);
        }

        float closestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;
        foreach (GameMechanics.Player enemy in players)
        {
            GameObject obj = enemy.obj;
            float distanceToEnemy = Vector3.Distance(obj.transform.position, transform.position);
            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                closestEnemy = obj;
            }
        }

        if (closestEnemy != null && closestDistance <= range)
        {
            target = closestEnemy.transform;
        }
        else
        {
            target = null;
        }


    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }

    /*
    If you just want a measurement of how strong the hit was (like, for example for damage calculations),
     the dot product of collision normal and collision velocity (ie the velocity of the two bodies relative to each other),
     times the mass of the other collider should give you useful values.
     */
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent<Movement>(out Movement mov))
        {

            int id = mov.GetId();
            int teamid = GameMechanics.gameMechanics.checkTeam(id);

            if (teamid != team)
            {

                ContactPoint cp = other.GetContact(0);
                Vector3 collisionVelocity = other.relativeVelocity;
                Vector3 collisionNormal = cp.normal;
                float mass = other.collider.attachedRigidbody.mass;
                float force = Mathf.Abs(Vector3.Dot(cp.normal, collisionVelocity)) * mass;

                applyForce(force);
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

    void RepelNearbyPlayers()
    {
        Collider[] playersInRadius = Physics.OverlapSphere(transform.position, explosionRadius, ~0, 0);
        foreach (Collider player in playersInRadius)
        {
            if (player.CompareTag("Player"))
            {
                Vector3 pushFactor = (player.transform.position - transform.position).normalized * pushForce;
                player.GetComponent<Movement>().PushMe(pushFactor, ForceMode.Impulse, true);
            }
        }
    }

    void NotifyNearbyPlayers()
    {
        Collider[] playersInRadius = Physics.OverlapSphere(transform.position, explosionRadius, ~0, 0);
        foreach (Collider col in playersInRadius)
        {
            if (col.CompareTag("Player"))
            {
                Player[] target = { col.GetComponent<PhotonView>().Owner };
                PlaySound.playSound.RPC_QueueVoice(23, target);
            }
        }
    }

    [PunRPC]
    void RememberMe()
    {
        Destroy(gameObject);
        StartCoroutine(CreateExplosion());
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
