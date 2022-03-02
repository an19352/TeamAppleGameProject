using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBallAbilities;
using Photon.Pun;

public class Hook : MonoBehaviour
{
    float timeLife;
    public float hookForce = 25f;
    Rigidbody rigid, playerRB;
    LineRenderer lineRenderer;

    float pullSpeed;
    float maxShootDistance;
    float stopPullDistance;

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
            Destroy(this.gameObject);
        }

        if (Vector3.Distance(playerPosition, transform.position) <= stopPullDistance)
        {
            Destroy(this.gameObject);
        }
        else
        {

            playerRB.AddForce((transform.position - playerPosition).normalized * pullSpeed, ForceMode.VelocityChange);
        }
    }

    public void Initialise(int rigidId, Vector3 _shootTransformForward, float _maxShootDistance, float _stopPullDistance, float _pullSpeed, float _timeLife)
    {
        transform.forward = _shootTransformForward;
        rigid = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
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
