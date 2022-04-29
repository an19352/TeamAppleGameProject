using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class PlaySound2 : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log(PlaySound.playSound.sounds);
            PlaySound.playSound.sounds[0].Play();
        }
    }
}
