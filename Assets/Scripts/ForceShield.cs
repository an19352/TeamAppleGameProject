using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class ForceShield : MonoBehaviour
{
    // Start is called before the first frame update
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

        if (generatorDestroyed >= 3)
        {
            StartCoroutine(GeneratorsDown());
        }
    }

    IEnumerator GeneratorsDown()
    {
        //Instantiate(explosion, transform.position, transform.rotation);
        yield return new WaitForSeconds(PlaySound.playSound.sounds[19].clip.length);
        PlaySound.playSound.RPC_QueueVoice(21, PhotonNetwork.PlayerList);
        gameObject.SetActive(false);
        yield return null;
    }
}
