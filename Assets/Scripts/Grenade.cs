using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Grenade : MonoBehaviour
{
    PhotonView PV;
    public GameObject grenadePrefab;

    public float delay = 3f;
    public float radius = 5f;
    public float force = 700f;
    //[SerializeField] float maxShootDistance = 20f;
    public GameObject explosionEffect;
    public Transform shootTransform;
    public float throwForce;
    float countdown;
    bool hasExploded = false;
    private Rigidbody rb;
    private Camera cameraMain;
    private Vector3 mouseLocation;
    private Vector3 lookDirection;
    private Quaternion lookRotation;
    private int lm;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        countdown = delay;
        cameraMain = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine) return;

        
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
            rb = nearbyObject.GetComponent<Rigidbody>();
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
