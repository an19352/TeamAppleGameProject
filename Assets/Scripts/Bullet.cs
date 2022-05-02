using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float travelSpeed;
    public float lifeTime;
    public float bulletForce;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
            other.attachedRigidbody.AddForce(rb.velocity * bulletForce, ForceMode.Force);
        }
    }
}

