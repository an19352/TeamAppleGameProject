using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class Nametag : MonoBehaviour
{
    PhotonView PV;
    public static PhotonLobby photonLobby;
    [SerializeField] private TextMeshProUGUI nameTag;
    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
       // if (!PV.IsMine) return;
        SetName();
    }

    private void SetName() => nameTag.text = PV.Owner.NickName;
    // Update is called once per frame
    
}
