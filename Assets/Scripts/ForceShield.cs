using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

public class ForceShield : MonoBehaviour
{
    // This sits on the force shields in each base
    //public GameObject explosion;
    public int generatorDestroyed = 0;
    // green is 0 and red is 1
    public int team;

    private void OnEnable()
    {
        generatorDestroyed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // The generatorDestroyed variable is changed in the EnergyGenerator script
        if (generatorDestroyed >= 3)
        {
            StartCoroutine(GeneratorsDown());
        }
    }

    IEnumerator GeneratorsDown()
    {
        //Instantiate(explosion, transform.position, transform.rotation);
        yield return new WaitForSeconds(PlaySound.playSound.sounds[19].clip.length);  // After the commentator commentates, disable this generator
        PlaySound.playSound.RPC_QueueVoice(21, PhotonNetwork.PlayerList);
        PlaySound.playSound.RPC_QueueVoice(GenerateCommentaryID(), PhotonNetwork.PlayerList);
        gameObject.SetActive(false);
        yield return null;
    }

    int GenerateCommentaryID()
    {
        int commID = 0;
        Random ran = new Random();

        if (gameObject.layer == 14)
        {
            commID = ran.Next(3, 5);
        }
        else
        {
            commID = ran.Next(1,3);
        }
        return commID;
    }
}
