using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TimerToDisable : MonoBehaviour
{
    // Cutscene timer
    public CinemachineBrain cameraCM;
    public GameObject introText;
    public float shutDownTime;


    void Start()
    {
        cameraCM = gameObject.GetComponent<CinemachineBrain>();
        StartCoroutine(ShutDownTimer(shutDownTime, cameraCM));
    }
    IEnumerator ShutDownTimer(float shutDownTime, CinemachineBrain cameraCM)
    {
        yield return new WaitForSeconds(shutDownTime);
        cameraCM.enabled = false;
    }

}
