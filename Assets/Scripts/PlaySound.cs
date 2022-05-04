using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
 
    public static PlaySound playSound;
    
    #region Singleton

    private void Awake()      //PlaySound is on the Player Avatar
    {   
        if (PlaySound.playSound == null) PlaySound.playSound = this; //if there is no playSound, this one becomes the main one
        else if (PlaySound.playSound != this) 
        { // if a PlaySound already existing (on a different player avatar) ...
            Destroy(PlaySound.playSound.gameObject);  // ... destroy the different player avatar
            PlaySound.playSound = this;  // and this PlaySound takes its place as the main one.
        }
    }

    #endregion

    public GameObject soundBoard;
    public AudioSource[] sounds;
    
    // Start is called before the first frame update
    void Start()
    {
        sounds = soundBoard.GetComponents<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            sounds[0].Play();
        }
    }
}
