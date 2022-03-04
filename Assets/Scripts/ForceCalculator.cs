using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForceCalculator : MonoBehaviour
{

    private Image healthBarImage;

    [SerializeField] float health = 50f;
    [SerializeField] float healthRemain = 50f;

    // Start is called before the first frame update
    void Start()
    {
        Transform canvas = this.gameObject.transform.Find("Canvas");
        Transform healthBar = canvas.Find("HealthBar");
        healthBarImage = healthBar.gameObject.GetComponent<Image>();
        Debug.Log(healthBarImage);
    }


    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            this.gameObject.SetActive(false);
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
        // Vector3 force = other.impulse / Time.deltaTime;
    }
}
