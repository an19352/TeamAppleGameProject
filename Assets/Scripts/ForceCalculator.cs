using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;

public class ForceCalculator : MonoBehaviour, IPunObservable
{
    PhotonView PV;
    public GameObject forceShield;

    public float health = 50f;
    public float healthRemain = 50f;
    public GameObject destroyedVersion;
    public float range = 35f;
    public float turnSpeed = 10f;
    public int team;
    public Transform target;
    public Transform partToRotate;
    public Transform healthBar;
    private Image healthBarImage;

    private ForceShield fsScript;
    public LayerMask players;

    public float pushForce = 5;
    public float explosionRadius = 2;


    // Start is called before the first frame update
    void Start()
    {

        PV = this.GetComponent<PhotonView>();
        // Transform canvas = this.gameObject.transform.Find("Canvas");
        healthBarImage = healthBar.gameObject.GetComponent<Image>();
        fsScript = forceShield.GetComponent<ForceShield>();

        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }


    // Update is called once per frame
    void Update()
    {
        // if (!PV.IsMine) return;
        if (healthRemain <= 0)
        {
            PhotonNetwork.Instantiate(destroyedVersion.name, transform.position, transform.rotation);
            Debug.Log(fsScript.generatorDestroyed);
            PV.RPC("RememberMe", RpcTarget.AllBuffered);
            //PhotonNetwork.Destroy(this.gameObject);
            RepelNearbyPlayers();
        }

        if (target != null)
        {
            Vector3 direction = transform.position - target.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            Quaternion smoothRotation = Quaternion.Lerp(partToRotate.rotation, rotation, turnSpeed);
            partToRotate.rotation = Quaternion.Euler(0f, smoothRotation.eulerAngles.y, 0f);
        }
    }


    // updates a few times a second, used to locate the closest enemy player in range
    void UpdateTarget()
    {
        int enemyLayer = team == 0 ? 13 : 12;
        List<GameObject> players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        List<GameObject> enemys = players.FindAll(player => player.layer == enemyLayer);

        float closestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;
        foreach (var enemy in enemys)
        {
            float distanceToEnemy = Vector3.Distance(enemy.transform.position, transform.position);
            if (distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                closestEnemy = enemy;
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
        Debug.Log(other.collider.tag);
        ContactPoint cp = other.GetContact(0);
        Vector3 collisionVelocity = other.relativeVelocity;
        Vector3 collisionNormal = cp.normal;
        float mass = other.collider.attachedRigidbody.mass;
        float force = Mathf.Abs(Vector3.Dot(cp.normal, collisionVelocity)) * mass;

        healthRemain -= force;
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

    [PunRPC]
    void RememberMe()
    {
        fsScript.generatorDestroyed++;
        Destroy(gameObject);
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
