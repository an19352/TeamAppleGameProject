using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ForceCalculator : MonoBehaviour, IPunObservable
{
    PhotonView PV;
    public GameObject forceShield;

    public float health = 50f;
    public float healthRemain = 50f;
    public GameObject destroyedVersion;

    private Image healthBarImage;

    private ForceShield fsScript;
    public LayerMask players;

    public float pushForce = 5;
    public float explosionRadius = 2;


    // Start is called before the first frame update
    void Start()
    {

        PV = this.GetComponent<PhotonView>();
        Transform canvas = this.gameObject.transform.Find("Canvas");
        Transform healthBar = canvas.Find("HealthBar");
        healthBarImage = healthBar.gameObject.GetComponent<Image>();
        fsScript = forceShield.GetComponent<ForceShield>();
    }


    // Update is called once per frame
    void Update()
    {
        // if (!PV.IsMine) return;
        if (healthRemain <= 0)
        {
            PhotonNetwork.Instantiate(destroyedVersion.name, transform.position, transform.rotation);
            fsScript.generatorDestroyed++;
            Debug.Log(fsScript.generatorDestroyed);
            PV.RPC("RememberMe", RpcTarget.AllBuffered);
            //PhotonNetwork.Destroy(this.gameObject);
            RepelNearbyPlayers();
        }

    }

    /*
    If you just want a measurement of how strong the hit was (like, for example for damage calculations),
     the dot product of collision normal and collision velocity (ie the velocity of the two bodies relative to each other),
     times the mass of the other collider should give you useful values.
     */
    void OnCollisionEnter(Collision other)
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
