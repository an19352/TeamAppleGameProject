using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    // Start is called before the first frame update
    public float coolDown;
    public float upwardForce;
    private GameObject arrowUp;

    void Start()
    {
        coolDown = 4.0f;
        upwardForce = 1000.0f;
        arrowUp = gameObject.transform.GetChild(0).gameObject;

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision other)
    {

        arrowUp.SetActive(false);
        StartCoroutine(ActivateCooldown(coolDown));
        Rigidbody playerBody = other.collider.attachedRigidbody;
        playerBody.AddForce(transform.up * upwardForce);


    }

    //Coroutine to activate a cooldown for a set period of time
    IEnumerator ActivateCooldown(float time)
    {
        yield return new WaitForSeconds(time);
        arrowUp.SetActive(true);
    }
}


