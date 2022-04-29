using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    
    public static PlaySound playSound;

    #region Singleton

    private void Awake()
    {
        if (PlaySound.playSound == null) PlaySound.playSound = this;
        else if (PlaySound.playSound != this)
        {
            Destroy(PlaySound.playSound.gameObject);
            PlaySound.playSound = this;
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
        
    }
}
