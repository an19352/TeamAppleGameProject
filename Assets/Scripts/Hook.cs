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
    Rigidbody rigid;
    Movement player;
    LineRenderer lineRenderer;
    Vector3 pullingForce = new Vector3(0f, 0f, 0f);

    float pullSpeed;
    float maxShootDistance;
    float stopPullDistance;
    float antigravity;
    float sin = 0f;

    void Awake()
    { 
        PV = GetComponent<PhotonView>();

        rigid = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerPosition = player.transform.position;

        lineRenderer.SetPositions(new Vector3[] {
            transform.position,
            playerPosition
        });

        if (rigid.velocity.magnitude > 0.1f) return;

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
            //pullingForce = (transform.position - playerPosition).normalized * pullSpeed;
            //sin = (new Vector2(pullingForce.x, pullingForce.z).magnitude) / Vector3.Distance(transform.position, playerPosition);
            player.PushMe((-playerPosition + transform.position).normalized * pullSpeed - player.gameObject.GetComponent<Rigidbody>().velocity, ForceMode.Acceleration);
        }
    }

    public void PhotonInitialise(int rigidId, Vector3 _shootTransformForward, float _maxShootDistance, float _stopPullDistance, float _pullSpeed, float _antigravity, float _timeLife)
    {
        PV.RPC("Initialise", RpcTarget.All, rigidId, _shootTransformForward, _maxShootDistance, _stopPullDistance, _pullSpeed, _antigravity, _timeLife);
    }

    [PunRPC]
    void Initialise(int rigidId, Vector3 targetPosition, float _maxShootDistance, float _stopPullDistance, float _pullSpeed, float _antigravity, float _timeLife)
    {
        transform.LookAt(targetPosition);

        player = PhotonView.Find(rigidId).gameObject.GetComponent<Movement>();
        rigid.velocity = Vector3.zero;
        rigid.AddForce(hookForce * transform.forward, ForceMode.Impulse);
        
        maxShootDistance = _maxShootDistance;
        stopPullDistance = _stopPullDistance;
        pullSpeed = _pullSpeed;
        antigravity = _antigravity;

        timeLife = Time.time + _timeLife;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if ((LayerMask.GetMask("Hookable") & 1 << other.gameObject.layer) > 0)
        {
            rigid.isKinematic = true;
        }
    }
}
