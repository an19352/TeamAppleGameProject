using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBallAbilities;
using Photon.Pun;

public class Hook : MonoBehaviour
{
    PhotonView PV;

    float timeLife;
    public float hookForce = 25f;
    Rigidbody rigid, playerRB;
    LineRenderer lineRenderer;

    float pullSpeed;
    float maxShootDistance;
    float stopPullDistance;

    void Awake()
    { 
        PV = GetComponent<PhotonView>();

        rigid = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerPosition = playerRB.transform.position;

        lineRenderer.SetPositions(new Vector3[] {
            transform.position,
            playerPosition
        });

        if (rigid.useGravity) return;

        if (Time.time >= timeLife) Destroy(this.gameObject);


        if (Vector3.Distance(playerPosition, transform.position) >= maxShootDistance)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }

        if (Vector3.Distance(playerPosition, transform.position) <= stopPullDistance)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
        else
        {

            playerRB.AddForce((transform.position - playerPosition).normalized * pullSpeed, ForceMode.VelocityChange);
        }
    }

    public void PhotonInitialise(int rigidId, Vector3 _shootTransformForward, float _maxShootDistance, float _stopPullDistance, float _pullSpeed, float _timeLife)
    {
        

        PV.RPC("Initialise", RpcTarget.All, rigidId, _shootTransformForward, _maxShootDistance, _stopPullDistance, _pullSpeed, _timeLife);
    }

    [PunRPC]
    void Initialise(int rigidId, Vector3 _shootTransformForward, float _maxShootDistance, float _stopPullDistance, float _pullSpeed, float _timeLife)
    {
        transform.forward = _shootTransformForward;
        rigid.AddForce(transform.forward * hookForce, ForceMode.Impulse);

        playerRB = PhotonView.Find(rigidId).gameObject.GetComponent<Rigidbody>();
        maxShootDistance = _maxShootDistance;
        stopPullDistance = _stopPullDistance;
        pullSpeed = _pullSpeed;

        timeLife = Time.time + _timeLife;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if ((LayerMask.GetMask("Hookable") & 1 << other.gameObject.layer) > 0)
        {
            rigid.useGravity = false;
            rigid.isKinematic = true;
        }
    }
}
