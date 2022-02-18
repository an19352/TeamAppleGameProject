using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{

    private float hookForce = 25f;
    Grapple grapple;
    Rigidbody rigid;
    LineRenderer lineRenderer;
    private Camera cameraMain;
    private Vector3 mouseLocation;

    // Start is called before the first frame update
    void Start()
    {
        cameraMain = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer.SetPositions(new Vector3[] {
            transform.position,
            grapple.transform.position
        });
    }

    public void Initialise(Grapple grapple, Transform shootTransform)
    {
        transform.forward = shootTransform.forward;
        this.grapple = grapple;
        rigid = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
        rigid.AddForce(transform.forward * hookForce, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((LayerMask.GetMask("Hookable") & 1 << other.gameObject.layer) > 0)
        {
            rigid.useGravity = false;
            rigid.isKinematic = true;

            grapple.StartPull();
        }
    }

}
