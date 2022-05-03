using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
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

    public GameObject soundBoard;
    public AudioSource[] sounds;
    private PhotonView PV;
    private Queue<int> voiceQueue = new Queue<int>();
    private int lastPlayed = 0;


    // Start is called before the first frame update
    void Start()
    {
        PV = this.GetComponent<PhotonView>();
        sounds = soundBoard.GetComponents<AudioSource>();
        QueueVoice(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (sounds[lastPlayed].isPlaying == false && voiceQueue.Count > 0)
        {
            lastPlayed = voiceQueue.Dequeue();
            RPC_PlayVoice(lastPlayed);
            StartCoroutine(FadeBackground());
        }
    }

    IEnumerator FadeBackground()
    {
        sounds[17].volume = 0.3f;
        yield return new WaitForSeconds(sounds[lastPlayed].clip.length);
        sounds[17].volume = 1f;
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
     */

    [PunRPC]
    void PlayVoice(int voiceID)
    {
        Debug.Log(voiceID);
        sounds[voiceID].Play();
    }

    public void RPC_PlayVoice(int voiceID)
    {
        Debug.Log(voiceID);
        PV.RPC("PlayVoice", RpcTarget.All, voiceID);
    }

    public void QueueVoice(int voiceID)
    {
        voiceQueue.Enqueue(voiceID);
    }
}
