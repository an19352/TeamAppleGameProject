using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float travelSpeed;
    public float lifeTime;
    public const float lifeTimeConst = 10;
    public float bulletForce;
    private Rigidbody rb;
    public float soundRadius;
    public LayerMask players;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        lifeTime = lifeTimeConst;
        NotifyNearbyPlayers();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (lifeTime <= 0) gameObject.SetActive(false);
        else
        {
            rb.MovePosition(transform.position + -transform.forward * travelSpeed);
            lifeTime -= Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Movement>().PushMe(rb.velocity * bulletForce, ForceMode.Force);
        }
    }
    
    void NotifyNearbyPlayers()
    {
        Collider[] playersInRadius = Physics.OverlapSphere(transform.position, soundRadius, ~0, 0);
        foreach (Collider col in playersInRadius)
        {
            if (col.CompareTag("Player"))
            {
                Player[] target = { col.GetComponent<PhotonView>().Owner };
                PlaySound.playSound.RPC_InstantSound(22, target);
            }
        }
    }
}

