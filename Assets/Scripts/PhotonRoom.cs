using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class PhotonRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    public static PhotonRoom room;
    private PhotonView PV;

    public GameObject prefab;

    public int multiplayerSceneIndex = 1;
    private int currentSceneIndex;

    [HideInInspector]
    public int greenScore;
    [HideInInspector]
    public int redScore;

    #region Singleton

    private void Awake()
    {
        if (PhotonRoom.room == null)
        {
            PhotonRoom.room = this;
        }
        else
        {
            if (PhotonRoom.room != this)
            {
                Destroy(PhotonRoom.room.gameObject);
                PhotonRoom.room = this;
            }
        }
        DontDestroyOnLoad(this.gameObject);
        PV = GetComponent<PhotonView>();
    }

    #endregion

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        if (PhotonNetwork.IsMasterClient) return;
        StartGame();
    }

    void StartGame()
    {
        PhotonNetwork.LoadLevel(multiplayerSceneIndex);
    }

    void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        currentSceneIndex = scene.buildIndex;
        if (currentSceneIndex == multiplayerSceneIndex) CreatePlayer();
    }

    void CreatePlayer()
    {
        PhotonNetwork.Instantiate(prefab.name, transform.position, Quaternion.identity, 0);
    }

    public void Reload()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
    }
}
