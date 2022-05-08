using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Random = System.Random;

namespace SpaceBallAbilities
{
    // If you want to add a new Powerup:
    //          - in this file create a new monobehaviour that inherets from IAbility
    //          - in "Resources/Powerups Settings" create a new InventoryElement (Scriptable Object) for this powerup
    //            This IE will contain everything from how long this powerup should last to its name and icon
    //          - finally add, in Resources add the prefab that will spawn and be picked up.
    //                          Give it the Powerup Script (don't forget to assign your IE here) and a Photon View
    public interface IAbility
    {
        public void SetUp(string IEname); // Reads this player's settings for this weapon and creates UI element for it

        public void LeftClick();
        public void RightClick();
        public InventoryElement GetIE(); // Return the scriptable object associated with this component
    }

    public class GravityGun : MonoBehaviour, IAbility
    {
        PhotonView PV;
        InventoryElement IE;
        Inventory inventory;

        float maxGrabDistance, throwForce;
        Transform objectHolder;
        int grabbedID;
        //private Grenade _grenade;
        Rigidbody grabbedRB = null;

        public void SetUp(string IEtag)
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
            else
            {
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
        public void SetUp(string IEtag)
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
            if (hook == null)
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
                hook.PhotonInitialise(IPPV, mouseLocation, maxShootDistance, stopPullDistance, pullSpeed, antigravity, hookLifetime);
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
        new GameObject particleSystem;
        float timeToShoot = 0;

        float pushForce;
        float distance;

        List<int> toBePushed;

        GameObject pushedEffect;
        bool offline = false;
        OfflineMovement NPCfound = null;

        // Start is called before the first frame update
        public void SetUp(string IEtag)
        {
            if (TryGetComponent(out PhotonView _PV))
                PV = _PV;
            else
                offline = true;

            IE = InventoryUIManager.inventory.GetIE(IEtag);
            toBePushed = new List<int>();
            inventory = GetComponentInParent<Inventory>();
            pushForce = inventory.pushForce;
            particleSystem = inventory.particleSystem;
            pushedEffect = inventory.pushedEffect;

            if(offline)
                InventoryUIManager.inventory.AddUIElement(IEtag, inventory);
                else
            if (PV.IsMine)
            InventoryUIManager.inventory.AddUIElement(IEtag, inventory);
        }

        public void LeftClick()
        {
            if (Time.time < timeToShoot) return;
            if (offline) Showoff();
            else PV.RPC("Showoff", RpcTarget.All);
            timeToShoot = Time.time + 1.2f;

            if (NPCfound != null) NPCfound.RPC_PushMe(transform.forward * pushForce, ForceMode.VelocityChange, true);

            if (toBePushed.Count == 0) return;
            int[] pushNow = new int[toBePushed.Count];
            //pushed = GameObject.FindGameObjectsWithTag("Detected");
            for (int i = 0; i < toBePushed.Count; i++)
            {
                if (PhotonView.Find(toBePushed[i]) != null)
                {
                    GameObject _obj = PhotonView.Find(toBePushed[i]).gameObject;
                    Debug.Log(_obj);
                    if (_obj.TryGetComponent(out Movement movement))
                    {
                        Instantiate(pushedEffect, _obj.transform.position, _obj.transform.rotation);
                        movement.PushMe(transform.forward * pushForce, ForceMode.VelocityChange, true);
                    }
                    else if(_obj.TryGetComponent(out Rigidbody rb)) rb.AddForce(transform.forward * pushForce, ForceMode.VelocityChange);
                }
            }
        }

        [PunRPC]
        public void Showoff()
        {
            particleSystem.SetActive(false);
            particleSystem.SetActive(true);
        }

        public void RightClick() { return; }

        public InventoryElement GetIE() { return IE; }

        private void OnTriggerEnter(Collider other)
        {
            if (offline)
            {
                if (other.gameObject.TryGetComponent(out OfflineMovement OM)) NPCfound = OM;
                return;
            }

            if (other.CompareTag("Detector") && other.transform.parent != transform)
            {
                toBePushed.Add(other.transform.parent.GetComponent<PhotonView>().ViewID);
            }
            if (other.CompareTag("DetectorNonPlayer"))
            {
                toBePushed.Add(other.gameObject.GetComponent<PhotonView>().ViewID);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (offline)
            {
                if (other.gameObject.TryGetComponent(out OfflineMovement OM)) NPCfound = null;
                return;
            }

            if (other.CompareTag("Detector") && other.transform.parent != transform)
            {
                toBePushed.Remove(other.transform.parent.GetComponent<PhotonView>().ViewID);
            }
            if (other.CompareTag("DetectorNonPlayer"))
            {
                toBePushed.Remove(other.gameObject.GetComponent<PhotonView>().ViewID);
            }
        }
    }

    public class Grenade : MonoBehaviour, IAbility
    {
        InventoryElement IE;
        Inventory inventory;

        Transform objectHolder;
        public GameObject grenadePrefab;
        bool hasExploded = false;
        private Camera cameraMain;
        private Vector3 mouseLocation;
        bool offline = false;

        public void SetUp(string IEtag)
        {
            cameraMain = Camera.main;
            IE = InventoryUIManager.inventory.GetIE(IEtag);
            inventory = GetComponent<Inventory>();
            grenadePrefab = inventory.grenadePrefab;
            mouseLocation = Input.mousePosition;

            if (TryGetComponent(out PhotonView PV))
            {
                if (PV.IsMine)
                    InventoryUIManager.inventory.AddUIElement(IEtag, inventory);
            }
            else 
            { 
                InventoryUIManager.inventory.AddUIElement(IEtag, inventory);
                offline = true; 
            }
        }

        public void RightClick() { return; }

        public void LeftClick()
        {
            RaycastHit hit;
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            LayerMask lm = LayerMask.GetMask("Ground");
            if (Physics.Raycast(mouseRay, out hit, 1000f, lm))
            {
                Vector3 objectHit = hit.point;
                if (offline)
                    Instantiate(grenadePrefab, objectHit, Quaternion.identity);
                else
                    PhotonNetwork.Instantiate(grenadePrefab.name, objectHit, Quaternion.identity);
            }
        }
        public InventoryElement GetIE() { return IE; }
    }

    public class Meteor : MonoBehaviour, IAbility
    {
        ObjectPooler poolOfObject;
        InventoryElement IE;
        Inventory inventory;
        PhotonView PV;
        
        Transform objectHolder;
        public GameObject MeteorPrefab;
        private Camera cameraMain;
        private Vector3 mouseLocation;
        bool offline = false;
        public LayerMask ignoredLayers;
        private List<string> meteorTags;
        private GameObject meteor;
        public float meteorFallForce;
        public int meteorsSpawned;
        public float meteorInterval;
        
        public void SetUp(string IEtag)
        {
            PV = GetComponent<PhotonView>();
            poolOfObject = ObjectPooler.OP;
            meteorTags = new List<string>();
            cameraMain = Camera.main;
            IE = InventoryUIManager.inventory.GetIE(IEtag);
            inventory = GetComponent<Inventory>();
            MeteorPrefab = inventory.MeteorPrefab;
            meteorFallForce = inventory.meteorFallForce;
            ignoredLayers = inventory.ignoredLayers;
            mouseLocation = Input.mousePosition;
            meteorsSpawned = inventory.meteorsSpawned;
            meteorInterval = inventory.meteorInterval;
            SetMeteorTags();

            /*if (TryGetComponent(out PhotonView PV))
            {
                if (PV.IsMine)
                    InventoryUIManager.inventory.AddUIElement(IEtag, inventory);
            }
            else 
            { 
                InventoryUIManager.inventory.AddUIElement(IEtag, inventory);
                offline = true; 
            }*/
        }

        public void RightClick() { return; }

        public void LeftClick()
        {
            Debug.Log("meteor");
            SpawnMeteors();
        }

        void SpawnMeteors()
        {
            Ray mouseRay = cameraMain.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out RaycastHit hit, 1000f, ~ignoredLayers))
            {
                mouseLocation = hit.point;
            }

            StartCoroutine(CoSpawnMeteors(mouseLocation));
        }
        
        /*void SpawnMeteor()
        {
            Ray mouseRay = cameraMain.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mouseRay, out RaycastHit hit, 1000f, ~ignoredLayers))
            {
                mouseLocation = hit.point;
            }

            Vector3 spawnLoc = mouseLocation;
            Vector3 newPos = RandomSpawnPosition();
            spawnLoc.y += newPos.y;
            spawnLoc.x += newPos.x;
            spawnLoc.z += newPos.z;
            
            PV.RPC("GenerateMeteor", RpcTarget.All, 0, spawnLoc, mouseLocation);

        }*/

        IEnumerator CoSpawnMeteors(Vector3 mouseLocation)
        {
            for (int i = 0; i < meteorsSpawned; i++)
            {
                Vector3 spawnLoc = mouseLocation;
                Vector3 newPos = RandomSpawnPosition();
                spawnLoc.y += newPos.y;
                spawnLoc.x += newPos.x;
                spawnLoc.z += newPos.z;
            
                PV.RPC("GenerateMeteor", RpcTarget.All, 0, spawnLoc, mouseLocation);
                yield return new WaitForSeconds(meteorInterval);
            }
            yield return null;
        }
        
        [PunRPC]
        void GenerateMeteor(int randMet, Vector3 randPositionSpawn, Vector3 randPositionTarget)
        {
            if (PV.IsMine)
            {
                Debug.Log(meteorTags[randMet]);
                meteor = poolOfObject.SpawnFromPool(meteorTags[randMet], randPositionSpawn, Quaternion.identity);
                Vector3 pushFactor = ((randPositionTarget - randPositionSpawn).normalized) * meteorFallForce;
                meteor.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
                meteor.GetComponent<Rigidbody>().AddForce(pushFactor, ForceMode.Impulse);
            }
        }
        
        public void SetMeteorTags()
        {
            foreach (ObjectPooler.Pool pool in poolOfObject.pools)
            {
                if (pool.prefab.CompareTag("Meteor"))
                    meteorTags.Add(pool.tag);
            }
        }
        
        public Vector3 RandomSpawnPosition()
        {
            Vector3 RanSpawn;
            Random ran = new Random();
            RanSpawn.x = ran.Next(-2, 3);
            RanSpawn.z = ran.Next(-2, 3);
            RanSpawn.y = 10;

            return RanSpawn;
        }
        
        public InventoryElement GetIE() { return IE; }
    }
    
    public class Coin : MonoBehaviour, IAbility
    {
        InventoryElement IE;
        Inventory inventory;
        public void SetUp(string IEtag)
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

    public class Jetpack : MonoBehaviour, IAbility
    {
        InventoryElement IE;
        Inventory inventory;
        PhotonView PV;

        float gravity;
        float antiGravity;

        GameObject boosterFlamePrefab;
        GameObject boosterFlame;
        Movement movement;

        public void SetUp(string IEtag)
        {
            PV = GetComponent<PhotonView>();
            IE = InventoryUIManager.inventory.GetIE(IEtag);
            inventory = GetComponent<Inventory>();
            movement = GetComponent<Movement>();
            // fetch parameter settings about the power-up from the static reference to inventory
            // (I guess) only store those variables that you want to expose to the editor in inventory
            antiGravity = inventory.antiGravity;
            boosterFlamePrefab = inventory.boosterFlame;

            gravity = GetComponent<Movement>().gravityStrength;
            boosterFlame = Instantiate(boosterFlamePrefab, transform);
            boosterFlame.SetActive(false);

            if(PV.IsMine)
            InventoryUIManager.inventory.AddUIElement(IEtag, inventory);
        }



        public void LeftClick()
        {
            PV.RPC("Ues", RpcTarget.All, antiGravity);

        }

        public void RightClick()
        {
            PV.RPC("Ues", RpcTarget.All, gravity);

        }

        public void Update()
        {
            if (!PV.IsMine) return;

            if (Input.GetButtonUp("Fire1"))
            {
                PV.RPC("Ues", RpcTarget.All, gravity);
            }
        }

        [PunRPC]
        public void Ues(float _gravity)
        {
            movement.gravityStrength = _gravity;
            boosterFlame.SetActive(_gravity == antiGravity);
        }

        public InventoryElement GetIE() { return IE; }
    }
}
