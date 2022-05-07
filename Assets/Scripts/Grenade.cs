using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Grenade : MonoBehaviour
{
    public float delay;
    public float radius;
    public float force;
    public GameObject explosionEffect;
    public GameObject offlineExplosionEffect;
    float countdown;
    bool hasExploded = false;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        countdown = delay;
    }

    // Update is called once per frame
    void Update()
    {
        countdown -= Time.deltaTime;
        if (countdown <= 0f && !hasExploded)
        {
            hasExploded = true;
            Explode();
        }
    }
    void Explode()
    {
        bool offline = false;
        if (GameMechanics.gameMechanics == null) offline = true;

        // show effect
        GameObject explosion;
        if (offline)
            explosion = Instantiate(offlineExplosionEffect, transform.position, Quaternion.identity);
        else 
            explosion = PhotonNetwork.Instantiate(explosionEffect.name, transform.position, Quaternion.identity);
        //get nearbyb objects
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        
        // Add force
        foreach (Collider nearbyObject in colliders)
        {
            //Debug.Log(nearbyObject.tag);
            rb = nearbyObject.GetComponent<Rigidbody>();
            
            if (rb != null)
            {
                if (nearbyObject.tag == "Generator") nearbyObject.GetComponent<EnergyGenerator>().applyForce(force);
                if (nearbyObject.tag == "Turret") nearbyObject.GetComponent<Turret>().applyForce(force);
                if (nearbyObject.tag == "Player")
                {
                    Vector3 pforce = (rb.position - transform.position).normalized * force;
                    if (offline) nearbyObject.GetComponent<OfflineMovement>().RPC_PushMe(pforce, ForceMode.VelocityChange);
                    else
                    nearbyObject.GetComponent<Movement>().PushMe(pforce,ForceMode.VelocityChange);
                }
                if (nearbyObject.TryGetComponent(out Gen_Tutorial GT))
                    GT.applyForce(force);
                //rb.AddExplosionForce(force, transform.position, radius);
            }
        }
        // remove grenade
        Destroy(gameObject);
        Destroy(explosion);
    }
}
