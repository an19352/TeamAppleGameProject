using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gen_Tutorial : MonoBehaviour
{
    
    public float health = 50f;
    public float healthRemain = 50f;
    public GameObject explosionEffect;
    public Transform healthBar;
    private Image healthBarImage;
    
    // Start is called before the first frame update
    void Start()
    {
        healthBarImage = healthBar.gameObject.GetComponent<Image>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent<OfflineMovement>(out OfflineMovement mov))
        {
            ContactPoint cp = other.GetContact(0);
            Vector3 collisionVelocity = other.relativeVelocity;
            Vector3 collisionNormal = cp.normal;
            float mass = other.collider.attachedRigidbody.mass;
            float force = Mathf.Abs(Vector3.Dot(cp.normal, collisionVelocity)) * mass;
            
            applyForce(force);
        }
    }

    public void applyForce(float force)
    {
        healthRemain -= force;
        float fraction = healthRemain / health;
        healthBarImage.fillAmount = fraction;

        if (healthRemain <= 0)
        {
            gameObject.SetActive(false);
            Instantiate(explosionEffect, transform.position, transform.rotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
