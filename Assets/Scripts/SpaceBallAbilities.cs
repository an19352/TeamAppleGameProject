using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace SpaceBallAbilities
{
    public class GravityGun : MonoBehaviour
    {
        PhotonView PV;

        float maxGrabDistance, throwForce;
        Transform objectHolder;
        int grabbedID;

        public void SetVariables(PhotonView _PV, float _maxGrabDistance, float _throwForce, Transform _objectHolder)
        {
            PV = _PV;

            maxGrabDistance = _maxGrabDistance;
            throwForce = _throwForce;

            objectHolder = _objectHolder;
        }

        public Rigidbody LeftClick(Rigidbody grabbedRB) 
        {
            if (grabbedRB)
            {
                grabbedID = grabbedRB.gameObject.GetComponent<PhotonView>().ViewID;

                PV.RPC("ChangeKinematic", RpcTarget.All, grabbedID, false);
                PV.RPC("ReleaseChildren", RpcTarget.All, grabbedID);
                PV.RPC("AddSomeForce", RpcTarget.All, grabbedID, objectHolder.transform.forward * throwForce, ForceMode.VelocityChange);
                grabbedRB = null;
            }
            else {
                RaycastHit hit;
                Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                int lm = LayerMask.GetMask("Grabbable");

                if (Physics.Raycast(mouseRay, out hit, 1000f, lm))
                {
                    GameObject objectHit = hit.collider.gameObject;
                    float dist = Vector3.Distance(objectHolder.position, objectHit.transform.position);
                    // make sure to only grab objects that are within a certain distance to the player 
                    if (dist <= maxGrabDistance)
                    {
                        grabbedRB = objectHit.GetComponent<Rigidbody>();
                        grabbedID = grabbedRB.gameObject.GetComponent<PhotonView>().ViewID;
                        PV.RPC("ChangeKinematic", RpcTarget.All, grabbedID, true);
                        PV.RPC("ParentObject", RpcTarget.All, grabbedID);
                    }
                }
            }
            return grabbedRB;
        }

        public Rigidbody RightClick(Rigidbody grabbedRB) 
        {
            if (grabbedRB)
            {
                grabbedID = grabbedRB.gameObject.GetComponent<PhotonView>().ViewID;

                PV.RPC("ChangeKinematic", RpcTarget.All, grabbedID, false);
                PV.RPC("ReleaseChildren", RpcTarget.All, grabbedID);
                grabbedRB = null;
            }
            return grabbedRB;
        }

        [PunRPC]
        void ChangeKinematic(int PVID, bool value)
        {
            Rigidbody rb = PhotonView.Find(PVID).gameObject.GetComponent<Rigidbody>();
            rb.isKinematic = value;
        }

        [PunRPC]
        void ParentObject(int PVID)
        {
            GameObject _obj = PhotonView.Find(PVID).gameObject;
            _obj.transform.SetParent(transform);
        }

        [PunRPC]
        void ReleaseChildren(int PVID)
        {
            GameObject _obj = PhotonView.Find(PVID).gameObject;
            _obj.transform.parent = null;
        }

        [PunRPC]
        void AddSomeForce(int PVID, Vector3 force, ForceMode mode)
        {
            Rigidbody _rb = PhotonView.Find(PVID).gameObject.GetComponent<Rigidbody>();
            _rb.AddForce(force, mode);
        }
    }

    public class Grapple : MonoBehaviour
    {
        PhotonView PV;

        float pullSpeed;
        float maxShootDistance;
        float hookLifetime;

        float stopPullDistance;
        GameObject hookPrefab;
        Transform shootTransform;

        Hook hook;
        Rigidbody rigid;
        private Camera cameraMain;
        private Vector3 mouseLocation;
        private Vector3 lookDirection;
        private Quaternion lookRotation;
        private int lm;

        // Start is called before the first frame update
        public void SetVariables(PhotonView _PV, Rigidbody _rigid, float _pullSpeed, float _maxShootDistance, float _stopPullingDistance, float _hookLifetime, GameObject _hookPrefab, Transform _shootTransform)
        {
            PV = _PV;
            rigid = _rigid;
            cameraMain = Camera.main;
            lm = LayerMask.GetMask("Hookable");

            pullSpeed = _pullSpeed;
            maxShootDistance = _maxShootDistance;
            stopPullDistance = _stopPullingDistance;
            hookLifetime = _hookLifetime;

            hookPrefab = _hookPrefab;
            shootTransform = _shootTransform;
        }

        public void LeftClick()
        {
            if(hook == null)
            {
                //rigid.gameObject.GetComponent<Inventory>().StopAllCoroutines();  //PROBLEMATIC
                Ray mouseRay = cameraMain.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(mouseRay, out RaycastHit hit, 1000f, lm))
                {
                    mouseLocation = hit.point;
                    lookDirection = (mouseLocation - shootTransform.position).normalized;
                    lookRotation = Quaternion.LookRotation(lookDirection);
                    shootTransform.rotation = lookRotation;
                }
                int IPPV = rigid.gameObject.GetComponent<PhotonView>().ViewID;
                PV.RPC("InitializeHook", RpcTarget.All, shootTransform.position, IPPV, shootTransform.forward, maxShootDistance, stopPullDistance, pullSpeed, hookLifetime);
                //rigid.gameObject.GetComponent<Inventory>().StartCoroutine(DestroyHookAfterLifetime());
                //FollowHook();
            }
        }

        public void RightClick()
        {
            if (hook != null)
                Destroy(hook.gameObject);
        }

        [PunRPC]
        public void InitializeHook(Vector3 shootPos, int rigidbodyID, Vector3 shootFor, float maxShoot, float stopDist, float pullS, float hookL)
        {
            hook = Instantiate(hookPrefab, shootPos, Quaternion.identity).GetComponent<Hook>();
            hook.Initialise(rigidbodyID, shootFor, maxShoot, stopDist, pullS, hookL);
        }
    }

}
