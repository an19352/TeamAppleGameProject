using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Grapple : MonoBehaviour
{
    PhotonView PV;

    [SerializeField] float pullSpeed = 0.5f;
    [SerializeField] float maxShootDistance = 20f;

    [SerializeField] float stopPullDistance = 4f;
    [SerializeField] GameObject hookPrefab;
    [SerializeField] Transform shootTransform;

    Hook hook;
    bool pulling;
    Rigidbody rigid;
    private Camera cameraMain;
    private Vector3 mouseLocation;
    private Vector3 lookDirection;
    private Quaternion lookRotation;
    private int lm;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        rigid = GetComponent<Rigidbody>();
        pulling = false;
        cameraMain = Camera.main;
        lm = LayerMask.GetMask("Hookable");
    }

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine) return;

        if (hook == null && Input.GetMouseButtonDown(0))
        {
            StopAllCoroutines();
            pulling = false;
            Ray mouseRay = cameraMain.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out RaycastHit hit, 1000f, lm))
            {
                mouseLocation = hit.point;
                lookDirection = (mouseLocation - shootTransform.position).normalized;
                lookRotation = Quaternion.LookRotation(lookDirection);
                shootTransform.rotation = lookRotation;
            }
            hook = Instantiate(hookPrefab, shootTransform.position, Quaternion.identity).GetComponent<Hook>();
            hook.Initialise(this, shootTransform);
            StartCoroutine(DestroyHookAfterLifetime());
        }
        else if (hook != null && Input.GetMouseButtonDown(1))
        {
            DestroyHook();
        }


        if (hook == null) return;

        if (Vector3.Distance(transform.position, hook.transform.position) >= maxShootDistance)
        {
            DestroyHook();
        }

        if (!pulling || hook == null) return;

        if (Vector3.Distance(transform.position, hook.transform.position) <= stopPullDistance)
        {
            DestroyHook();
        }
        else
        {
            rigid.AddForce((hook.transform.position - transform.position).normalized * pullSpeed, ForceMode.VelocityChange);
        }
    }

    public void StartPull()
    {
        pulling = true;
    }

    public void DestroyHook()
    {
        if (hook == null) return;

        pulling = false;
        Destroy(hook.gameObject);
        hook = null;
    }

    private IEnumerator DestroyHookAfterLifetime()
    {
        yield return new WaitForSeconds(8f);

        DestroyHook();
    }
}
