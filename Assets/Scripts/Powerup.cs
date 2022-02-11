using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Powerup : MonoBehaviour
{

    public Text currentPowerup;
    private bool cooldownActive;
    public bool hasPowerup;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    

    // Update is called once per frame
    void Update()
    {

        
        //Starts cooldown coroutine if key is pressed, powerup is possessed and cooldown is not active
        if (Input.GetKeyDown(KeyCode.V) && !cooldownActive && hasPowerup)
        {
            StartCoroutine(ActivateCooldown(5));
        }
 
    }

    //Coroutine to activate a cooldown for a set period of time
    IEnumerator ActivateCooldown(int time)
    {
        cooldownActive = true;
        hasPowerup = false;
        currentPowerup.text = "No Powerup";
        yield return new WaitForSeconds(time);
        cooldownActive = false;
    }

    //Makes powerup disappear when touched and writes to powerup text in UI
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Powerup"))
        {
            other.gameObject.SetActive(false);
            currentPowerup.text = LayerMask.LayerToName(other.gameObject.layer);
            hasPowerup = true;
        }
    }
}
