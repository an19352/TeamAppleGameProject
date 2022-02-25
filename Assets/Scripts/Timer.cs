using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public float totalTime = 100f;

    public static GameMechanics gameMechanics;
    TextMeshProUGUI timer;
    float finishTime;

    // Start is called before the first frame update
    void Start()
    {
        gameMechanics = GameMechanics.gameMechanics;
        totalTime++;
        finishTime = Time.time + totalTime;

        timer = GetComponent<TextMeshProUGUI>();
        timer.text = Convert_seconds(totalTime);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (totalTime <= 0)
        { 
            gameMechanics.End_Game();
            this.gameObject.SetActive(false);
        }

        totalTime = finishTime - Time.time;
        timer.text = Convert_seconds(totalTime);
    }

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
