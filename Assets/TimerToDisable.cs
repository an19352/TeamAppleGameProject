using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TimerToDisable : MonoBehaviour
{
    public CinemachineBrain cameraCM;
    public float shutDownTime;

    // Start is called before the first frame update
    void Start()
    {
        cameraCM = gameObject.GetComponent<CinemachineBrain>();
        StartCoroutine(ShutDownTimer(shutDownTime, cameraCM));
    }
    IEnumerator ShutDownTimer(float shutDownTime, CinemachineBrain cameraCM)
    {
        yield return new WaitForSeconds(shutDownTime);
        Debug.Log("times up");
        cameraCM.enabled = false;
    }

}
