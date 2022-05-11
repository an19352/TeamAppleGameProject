using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    // The global timer untill the round ends
    // The offline mode is used in the tutorial

    public float totalTime = 100f;
    public float offlineTime = 30f;

    bool offline = false;

    public static GameMechanics gameMechanics;
    TextMeshProUGUI timer;
    float finishTime;

    void Start()
    {
        if (GameMechanics.gameMechanics == null)
            offline = true;
        else
            gameMechanics = GameMechanics.gameMechanics;

        totalTime++;
        offlineTime++;
        if (offline) totalTime = offlineTime;
        finishTime = Time.time + totalTime;

        timer = GetComponent<TextMeshProUGUI>();
        timer.text = Convert_seconds(totalTime);
    }

    void FixedUpdate()
    {
        if (totalTime <= 0)
        {
            if (offline)
                PhotonNetwork.LoadLevel(PhotonRoom.room.multiplayerSceneIndex);

            else
                gameMechanics.End_Game();

            this.gameObject.SetActive(false);
        }

        totalTime = finishTime - Time.time;
        timer.text = Convert_seconds(totalTime);
    }

    // Convert the time in seconds to a readeble form
    private string Convert_seconds(float _totalTime)
    {
        string _timer = "";
        string minutes = ((int)_totalTime / 60).ToString();
        string seconds = ((int)_totalTime % 60).ToString();
        
        if (_totalTime / 60 < 10) minutes = "0" + minutes;
        if (_totalTime % 60 < 10) seconds = "0" + seconds;

        _timer = minutes + ":" + seconds;
        return _timer;
    }
    
    // This is what Game Mechanics uses to sync
    public void UpdateTimer (float newTotalTime)
    {
        totalTime = newTotalTime;
        finishTime = Time.time + totalTime;
    }
    
    public float GetTimer()
    {
        return totalTime;
    }
}
