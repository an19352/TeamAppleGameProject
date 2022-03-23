using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class GoBack : MonoBehaviour
{
   public void OnClick()
    {
        PhotonNetwork.Disconnect();
        PhotonNetwork.Destroy(PhotonRoom.room.gameObject);
        SceneManager.LoadScene(0);
    }
}
