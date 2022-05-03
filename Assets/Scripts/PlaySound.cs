using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Unity.Mathematics;
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

    public struct SoundCommand
    {
        public int voiceNo;
        public Player[] _target;
    }

    public GameObject soundBoard;
    public AudioSource[] sounds;
    private PhotonView PV;
    private Queue<SoundCommand> voiceQueue = new Queue<SoundCommand>();
    private SoundCommand lastPlayed = new SoundCommand();


    // Start is called before the first frame update
    void Start()
    {
        PV = this.GetComponent<PhotonView>();
        sounds = soundBoard.GetComponents<AudioSource>();
        
        QueueVoice(0, PhotonNetwork.PlayerList);
    }

    // Update is called once per frame
    void Update()
    {
        if (sounds[lastPlayed.voiceNo].isPlaying == false)
        {
            if (voiceQueue.Count > 0)
            {
                Debug.Log("play queue");
                lastPlayed = voiceQueue.Dequeue();
                RPC_PlayVoice(lastPlayed.voiceNo, lastPlayed._target);
                Debug.Log("finished");
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

    void RPC_FadeBackground(int voiceID, Player[] target)
    {
        foreach (Player p in target)
        {
            PV.RPC("StartFadeBackground", p, voiceID);
        }
    }

    [PunRPC]
    void StartFadeBackgroud(float time)
    {
        StartCoroutine(FadeBackground(time));
    }
    
    [PunRPC]
    IEnumerator FadeBackground(float time)
    {
        Debug.Log("called");
        sounds[17].volume = 0.3f;
        yield return new WaitForSeconds(time);
        sounds[17].volume = 1f;
        Debug.Log("end");
        yield return null;
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
     */

    [PunRPC]
    void PlayVoice(int voiceID)
    {
        sounds[voiceID].Play();
    }

    public void RPC_PlayVoice(int voiceID, Player[] target)
    {
        foreach (Player p in target)
        {
            PV.RPC("PlayVoice", p, voiceID);
            //PV.RPC("StartFadeBackground", p, sounds[voiceID].clip.length);
        }
    }

    public void QueueVoice(int voiceID, Player[] target)
    {
        SoundCommand command = new SoundCommand();
        command.voiceNo = voiceID;
        command._target = target;
        voiceQueue.Enqueue(command);
    }
}