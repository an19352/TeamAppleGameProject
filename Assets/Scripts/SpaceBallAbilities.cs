using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace SpaceBallAbilities
{
    public interface IAbility
    {
        public void SetUp();

        public void LeftClick();
        public void RightClick();
        public InventoryElement GetIE();
    }

    public class GravityGun : MonoBehaviour, IAbility
    {
        PhotonView PV;
        InventoryElement IE;
        Inventory inventory;
        string IEtag = "Gravity Gun";

        float maxGrabDistance, throwForce;
        Transform objectHolder;
        int grabbedID;
        Rigidbody grabbedRB = null;

        public void SetUp()
        {
            PV = GetComponent<PhotonView>();
            IE = InventoryUIManager.inventory.GetIE(IEtag);
            inventory = GetComponent<Inventory>();
            InventoryUIManager.inventory.AddUIElement(IEtag, inventory);

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

        public InventoryElement GetIE() { return IE; }
        
        private void OnDestroy()
        {
            RightClick();   
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

        InventoryElement IE;
        Inventory inventory;
        string IEtag = "Grapple Gun";

        float stopPullDistance;
        GameObject hookPrefab;
        Transform shootTransform;
        float antigravity;

        Hook hook;
        Rigidbody rigid;
        private Camera cameraMain;
        private Vector3 mouseLocation;
        private Vector3 lookDirection;
        private Quaternion lookRotation;
        private int lm;

        // Start is called before the first frame update
        public void SetUp()
        {
            IE = InventoryUIManager.inventory.GetIE(IEtag);
            rigid = GetComponent<Rigidbody>();
            inventory = GetComponent<Inventory>();
            InventoryUIManager.inventory.AddUIElement(IEtag, inventory);
            cameraMain = Camera.main;
            antigravity = inventory.antigravity;
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
                hook.PhotonInitialise(IPPV, shootTransform.forward, maxShootDistance, stopPullDistance, pullSpeed, antigravity, hookLifetime);
            }
        }

        public void RightClick()
        {
            if (hook != null)
                PhotonNetwork.Destroy(hook.gameObject);
        }

        public InventoryElement GetIE() { return IE; }

        private void OnDestroy()
        {
            RightClick();
        }
    }

    public class ImpulseCannon : MonoBehaviour, IAbility
    {
        PhotonView PV;
        InventoryElement IE;
        Inventory inventory;
        string IEtag = "Impulse Gun";
        GameObject particleSystem;
        float timeToShoot = 0;

        float pushForce;
        float distance;

        List<int> toBePushed;

        // Start is called before the first frame update
        public void SetUp()
        {
            PV = GetComponent<PhotonView>();
            IE = InventoryUIManager.inventory.GetIE(IEtag);
            toBePushed = new List<int>();
            inventory = GetComponentInParent<Inventory>();
            InventoryUIManager.inventory.AddUIElement(IEtag, inventory);
            pushForce = inventory.pushForce;
            particleSystem = inventory.particleSystem;
        }

        public void LeftClick()
        {
            if (Time.time < timeToShoot) return;
            particleSystem.SetActive(false);
            particleSystem.SetActive(true);
            timeToShoot = Time.time + 1.2f;
            if (toBePushed.Count == 0) return;
            int[] pushNow = new int[toBePushed.Count];
            //pushed = GameObject.FindGameObjectsWithTag("Detected");
            for (int i = 0; i < toBePushed.Count; i++)
            {
                if (PhotonView.Find(toBePushed[i]) != null)
                {
                    GameObject _obj = PhotonView.Find(toBePushed[i]).gameObject;
                    _obj.GetComponent<Movement>().PushMe(transform.forward * pushForce, ForceMode.VelocityChange);
                }
            }
        }

        public void RightClick() { return; }

        public InventoryElement GetIE() { return IE; }

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

    public class Grenade : MonoBehaviour, IAbility
    {
        public float delay;
        public float radius;
        public float force;
        public GameObject explosionEffect;
        public float throwForce;
        
        InventoryElement IE;
        Inventory inventory;
        string IEtag = "Grenade";

        public GameObject grenadePrefab;
        Transform shootTransform;
        float countdown;
        bool hasExploded = false;
        
        private Camera cameraMain;
        private Vector3 mouseLocation;
        private Vector3 lookDirection;
        private Quaternion lookRotation;
        private int lm;
        
        public void SetUp()
        {
            countdown = delay;
            cameraMain = Camera.main;
            
            IE = InventoryUIManager.inventory.GetIE(IEtag);
            inventory = GetComponent<Inventory>();
            InventoryUIManager.inventory.AddUIElement(IEtag, inventory);
            cameraMain = Camera.main;
            
            shootTransform = inventory.shootTransform;
        }

        public void RightClick()
        {
            GameObject grenade = Instantiate(grenadePrefab, transform.position, transform.rotation);
            Rigidbody rb = grenade.GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * throwForce, ForceMode.VelocityChange);
            
            countdown -= Time.deltaTime;
            if (countdown <= 0f && !hasExploded)
            {
                Explode();
                hasExploded = true;
            }
            
            void Explode(){

                // show effect
                Instantiate(explosionEffect, transform.position, transform.rotation);

                //get nearbyb objects
                Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

                // Add force
                foreach (Collider nearbyObject in colliders)
                {
                    Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
                    if(rb != null)
                    {
                        rb.AddExplosionForce(force, transform.position, radius);
                    }
                }

                //  damage

                // remove grenade
                Destroy(gameObject);
            }
        }
        public void LeftClick() { return; }

        
        public InventoryElement GetIE() { return IE; }
        
    }

    public class Coin : MonoBehaviour, IAbility
    {
        InventoryElement IE;
        Inventory inventory;
        string IEtag = "Coin";
        public void SetUp()
        {
            IE = InventoryUIManager.inventory.GetIE(IEtag);
            transform.localScale.Scale(new Vector3(2, 2, 2));

            inventory = GetComponent<Inventory>();
            InventoryUIManager.inventory.AddUIElement(IEtag, inventory);
        }

        private void OnDestroy()
        {
            transform.localScale.Scale(new Vector3(0.5f, 0.5f, 0.5f));
            //inventory.removeItem(IEtag);
        }

        public void LeftClick()
        {
            return;
        }

        public void RightClick()
        {
            return;
        }

        public InventoryElement GetIE() { return IE; }
    }
}
