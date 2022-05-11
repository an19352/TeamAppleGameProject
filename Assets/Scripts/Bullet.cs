using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // The turrets shoot bullets. This is each bullet's logic
    public float travelSpeed;
    public float lifeTime;
    public const float lifeTimeConst = 10;
    public float bulletForce;
    private Rigidbody rb;
    public float soundRadius;
    public LayerMask players;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        lifeTime = lifeTimeConst;
        NotifyNearbyPlayers();
    }

    // This is the physics loop update, called first
    void FixedUpdate()
    {
        if (lifeTime <= 0) gameObject.SetActive(false);
        else
        {
            rb.MovePosition(transform.position + -transform.forward * travelSpeed);
            lifeTime -= Time.deltaTime;
        }
    }

    // I touched a player, yuck! Push it away!
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Movement>().PushMe(rb.velocity * bulletForce, ForceMode.Force, true);
        }
    }
    
    // Play a sounf for players within the soundRadius
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

