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
        arrowUp = gameObject.transform.GetChild(0).gameObject;

    }

    void OnCollisionEnter(Collision other)
    {
        if (arrowUp == null) return;
        arrowUp.SetActive(false);
        StartCoroutine(ActivateCooldown(coolDown));
        Rigidbody playerBody = other.collider.attachedRigidbody;
        playerBody.AddForce(transform.up * upwardForce, ForceMode.Acceleration);


    }

    //Coroutine to activate a cooldown for a set period of time
    IEnumerator ActivateCooldown(float time)
    {
        yield return new WaitForSeconds(time);
        arrowUp.SetActive(true);
    }
}


