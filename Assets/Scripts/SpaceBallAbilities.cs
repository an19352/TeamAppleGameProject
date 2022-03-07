using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace SpaceBallAbilities
{
    public interface IAbility
    {
        public void LeftClick();
        public void RightClick();
    }

    public class GravityGun : MonoBehaviour, IAbility
    {
        PhotonView PV;

        float maxGrabDistance, throwForce;
        Transform objectHolder;
        int grabbedID;
        Rigidbody grabbedRB = null;

        void Awake()
        {
            PV = GetComponent<PhotonView>();
            Inventory inventory = GetComponent<Inventory>();

            maxGrabDistance = inventory.maxGrabDistance;
            throwForce = inventory.throwForce;
            objectHolder = inventory.objectHolder;
        }

        public void LeftClick() 
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
        }

        public void RightClick() 
        {
            if (grabbedRB)
            {
                grabbedID = grabbedRB.gameObject.GetComponent<PhotonView>().ViewID;

                PV.RPC("ChangeKinematic", RpcTarget.All, grabbedID, false);
                PV.RPC("ReleaseChildren", RpcTarget.All, grabbedID);
                grabbedRB = null;
            }
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

    public class Grapple : MonoBehaviour, IAbility
    {
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
        void Awake()
        {
            rigid = GetComponent<Rigidbody>();
            Inventory inventory = GetComponent<Inventory>();
            cameraMain = Camera.main;
            lm = LayerMask.GetMask("Hookable");


            pullSpeed = inventory.pullSpeed;
            maxShootDistance = inventory.maxShootDistance;
            stopPullDistance = inventory.stopPullDistance;
            hookLifetime = inventory.hookLifetime;

            hookPrefab = inventory.hookPrefab;
            shootTransform = inventory.shootTransform;
        }

        public void LeftClick()
        {
            if(hook == null)
            {
                Ray mouseRay = cameraMain.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(mouseRay, out RaycastHit hit, 1000f, lm))
                {
                    mouseLocation = hit.point;
                    lookDirection = (mouseLocation - shootTransform.position).normalized;
                    lookRotation = Quaternion.LookRotation(lookDirection);
                    shootTransform.rotation = lookRotation;
                }
                int IPPV = rigid.gameObject.GetComponent<PhotonView>().ViewID;
                hook = PhotonNetwork.Instantiate(hookPrefab.name, shootTransform.position, Quaternion.identity).GetComponent<Hook>();
                hook.PhotonInitialise(IPPV, shootTransform.forward, maxShootDistance, stopPullDistance, pullSpeed, hookLifetime);
            }
        }

        public void RightClick()
        {
            if (hook != null)
                PhotonNetwork.Destroy(hook.gameObject);
        }
    }

    public class ImpulseCannon : MonoBehaviour, IAbility
    {
        PhotonView PV;

        float pushForce;
        float distance;

        List<int> toBePushed;

        // Start is called before the first frame update
        void Awake()
        {
            PV = GetComponent<PhotonView>();
            toBePushed = new List<int>();

            pushForce = GetComponentInParent<Inventory>().pushForce;
        }

        public void LeftClick()
        {
            int[] pushNow = new int[toBePushed.Count];
            //pushed = GameObject.FindGameObjectsWithTag("Detected");
            for (int i = 0; i < toBePushed.Count; i++)
            {
                pushNow[i] = toBePushed[i];
                //                PV.RPC("RPC_Cannon", RpcTarget.All, i);
            }
            PV.RPC("RPC_Cannon", RpcTarget.All, pushNow, transform.forward * pushForce);
        }

        public void RightClick() { return; }

        [PunRPC]
        void RPC_Cannon(int[] pushNow, Vector3 pushFactor)
        {
            for (int i = 0; i < pushNow.Length; i++)
            {
                GameObject _obj = PhotonView.Find(pushNow[i]).gameObject;
                distance = Vector3.Distance(transform.parent.position, _obj.transform.position);
                _obj.GetComponent<Rigidbody>().AddForce(pushFactor * (1 / distance), ForceMode.Impulse);
            }

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Detector") && other.transform.parent != this.transform)
            {
                toBePushed.Add(other.gameObject.transform.parent.gameObject.GetComponent<PhotonView>().ViewID);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Detector") && other.transform.parent != this.transform)
            {
                toBePushed.Remove(other.gameObject.transform.parent.gameObject.GetComponent<PhotonView>().ViewID);
            }
        }
    }

    public class Coin : MonoBehaviour, IAbility
    {
        public void LeftClick()
        {
            return;
        }

        public void RightClick()
        {
            return;
        }
    }
}
