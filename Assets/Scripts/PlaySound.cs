using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    
    public static PlaySound playSound;
    private PhotonView PV;

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
        PV = this.GetComponent<PhotonView>();
        sounds = soundBoard.GetComponents<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /*
     0 - intro
     1 - B-AGD1
     2 - B-AGD2
     3 - R-AGD1
     4 - R-AGD2
     5 - B-FC1
     6 - B-FC2
     7 - B-FC3
     8 - R-FC1
     9 - R-FC2
     10 - R-FC3
     11 - B-FD1
     12 - B-FD2
     13 - B-FD3
     14 - R-FD1
     15 - R-FD2
     16 - R-FD3
     */

    [PunRPC]
    void PlayVoice(int soundID)
    {
        Debug.Log(soundID);
        sounds[soundID].Play();
    }

    public void RPC_PlayVoice(int voiceID)
    {
        Debug.Log(voiceID);
        PV.RPC("PlayVoice", RpcTarget.All, voiceID);
    }
}
