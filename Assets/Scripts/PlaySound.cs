using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Unity.Mathematics;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    // Knows all lines and songs and plays them
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

    /*public struct SoundCommand
    {
        public int voiceNo;
        public Player[] _target;
    }*/

    public GameObject soundBoard;
    public AudioSource[] sounds;
    private PhotonView PV;
    private Queue<int> voiceQueue = new Queue<int>();
    private int lastPlayed;


    // Start is called before the first frame update
    void Start()
    {
        PV = this.GetComponent<PhotonView>();
        sounds = soundBoard.GetComponents<AudioSource>();
        RPC_QueueVoice(0, PhotonNetwork.PlayerList);
        RPC_QueueVoice(27, PhotonNetwork.PlayerList);
        RPC_QueueVoice(24, PhotonNetwork.PlayerList);
        RPC_QueueVoice(25, PhotonNetwork.PlayerList);
    }

    // Update is called once per frame
    void Update()
    {
        if (sounds[lastPlayed].isPlaying == false)
        {
            if (voiceQueue.Count > 0)
            {
                lastPlayed = voiceQueue.Dequeue();
                sounds[lastPlayed].Play();
            }

            if (sounds[17].volume != 1)
            {
                sounds[17].volume = 1;
            }
        }

        else
        {
            if (sounds[17].volume == 1)
            {
                sounds[17].volume = 0.3f;
            }   
        }
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
     17 - Background Music
     18 - Capture Sound
     19 - Generator Explosion
     20 - Flag Pickup
     21 - Shields Down
     22 - Laser
     23 - Turret Explosion
     24 - 321
     25 - Go
     26 - 3
     27 - Start
     */

    [PunRPC]
    void PlayVoice(int voiceID)
    {
        sounds[voiceID].Play();
    }

    public void RPC_InstantSound(int voiceID, Player[] target)
    {
        foreach (Player p in target)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PV.RPC("PlayVoice", p, voiceID);
            }
        }
    }

    public void RPC_QueueVoice(int voiceID, Player[] target)
    {
        foreach (Player p in target)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PV.RPC("QueueVoice", p, voiceID);
            }
        }
    }

    [PunRPC]
    public void QueueVoice(int voiceID)
    {
        voiceQueue.Enqueue(voiceID);
    }
}