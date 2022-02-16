 using UnityEngine;
 
 public class SetTargetFrameRate : MonoBehaviour 
 {
     public int targetFrameRate = 60;
 
     private void Awake()
     {
         QualitySettings.vSyncCount = 0;
         Application.targetFrameRate = targetFrameRate;
     }
 }