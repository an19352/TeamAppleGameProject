using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Grenade : MonoBehaviour
{
    PhotonView PV;

    public float delay = 3f;
    public float radius = 5f;
    public float force = 700f;

    public GameObject explosionEffect;
    public Transform shootTransform;
    float countdown;
    bool hasExploded = false;

    private Camera cameraMain;
    private Vector3 mouseLocation;
    private Vector3 lookDirection;
    private Quaternion lookRotation;
    private int lm;

    // Start is called before the first frame update
    void Start()
    {
        //PV = GetComponent<PhotonView>();
        countdown = delay;
        cameraMain = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {
        //if (!PV.IsMine) return;

        countdown -= Time.deltaTime;
        if (countdown <= 0f && !hasExploded)
        {
            Explode();
            hasExploded = true;
        }
    }

    void Explode()
    {

        // show effect
        GameObject explosion = Instantiate(explosionEffect, transform.position, transform.rotation);

        //get nearbyb objects
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        // Add force
        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(force, transform.position, radius);
            }
        }

        //  damage

        // remove grenade
        Destroy(gameObject);
        Destroy(explosion);
    }
}
