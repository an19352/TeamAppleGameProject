using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using SpaceBallAbilities;

public class Inventory : MonoBehaviour
{
    PhotonView PV;
    public static InventoryUIManager inventory;

    InventoryElement[] IEs;
    Dictionary<string, System.Type> typeLookUp = new Dictionary<string, System.Type>();
    //public GameObject tooltip;
    public Vector3 tooltipOffset;
    public string firstPowerupTag = "Impulse Gun";

    [Range(1, 5)]
    public int inventorySize;
    IAbility[] inventoryItems;
    int selectedAbility, inventoryMaxATM;
    List<string> notNewPowerups = new List<string>();

    [Header("Gravity Gun Settings")]
    public Transform objectHolder;
    public float maxGrabDistance = 3f, throwForce = 20f, lerpSpeed = 10f;

    [Header("Grapple Gun Settings")]
    public Transform shootTransform;
    public float pullSpeed = 0.5f;
    public float maxShootDistance = 20f;
    public float stopPullDistance = 0.2f;
    public float hookLifetime = 8f;
    public float antigravity = 5f;
    public GameObject hookPrefab;

    [Header("Impulse Gun Settings")]
    public BoxCollider impulseGunHolder;
    public new GameObject particleSystem;
    public float pushForce;
    public GameObject pushedEffect;

    [Header("Grenade Settings")]
    public GameObject grenadePrefab;
    
    [Header("Meteor Settings")]
    public GameObject MeteorPrefab;
    public LayerMask ignoredLayers;
    public float meteorFallForce;
    public int meteorsSpawned;
    public float meteorInterval;
    
    [Header("Jetpack Settings")]
    public float antiGravity;
    public GameObject boosterFlame;

    bool offline = false;
    // Start is called before the first frame update
    void Start()
    {
        if (TryGetComponent(out PhotonView _PV))
            PV = _PV;
        else offline = true;
        inventory = InventoryUIManager.inventory;

        if(!offline)
        if (!PV.IsMine) return;

        IEs = inventory.CloneIEs();
        foreach (InventoryElement IE in IEs)
        {
            typeLookUp.Add(IE.powerupName, System.Type.GetType("SpaceBallAbilities." + IE.associatedClass));
        }

        inventoryItems = new IAbility[inventorySize];

        for (int i = 1; i < inventorySize; i++)
            inventoryItems[i] = null;

        inventoryMaxATM = 0;
        impulseGunHolder.enabled = true;
        activateItem(firstPowerupTag);

        //inventoryItems[0] = impulseGunHolder.AddComponent(itemComponents[2]) as IAbility;
        //inventoryItems[0].SetUp();
    }

    // Update is called once per frame
    void Update()
    {
        if(!offline)
        if (!PV.IsMine) return;

        if (inventoryMaxATM < 0)
            return;

        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
            SelectAbility(selectedAbility + 1);

        if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
            SelectAbility(selectedAbility - 1);

        if (Input.GetButtonDown("Fire1"))
            inventoryItems[selectedAbility].LeftClick();

        if (Input.GetButtonDown("Fire2"))
            inventoryItems[selectedAbility].RightClick();
    }

    public void activateItem(string tag)
    {/*
        if (typeLookUp.ContainsKey(tag)) 
        {
            Debug.LogWarning(tag + " not found");
            return; 
        }*/
        if(!offline)
        if (!PV.IsMine) return;

        if (inventoryMaxATM == inventorySize) return;

        if (!notNewPowerups.Contains(tag))
        {
            int tagIndex = 0;
            notNewPowerups.Add(tag);
            for (int i = 0; i < IEs.Length; i++) if (IEs[i].powerupName == tag) tagIndex = i;
            //GameObject _tooltip = Instantiate(tooltip, worldCanvas.transform);
            //_tooltip.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = IEs[tagIndex].tooltipText;
            //_tooltip.transform.position = transform.position + tooltipOffset;
        }

        for (int i = 0; i < inventorySize; i++)
        {
            if (inventoryItems[i] == null)
            {
                inventoryItems[i] = gameObject.AddComponent(typeLookUp[tag]) as IAbility;
                inventoryItems[i].SetUp(tag);
                
                if(!offline)
                PV.RPC("RPC_AddComponent", RpcTarget.Others, typeLookUp[tag].FullName, tag);
                
                inventoryMaxATM = i;

                SelectAbility(i);

                return;
            }

            if (inventoryItems[i].GetIE().powerupName == tag)
            {
                // Reset timer or add to it or something
                inventory.AddSecondsToPowerup(tag, inventory.GetIE(tag).timeToDie / 2);
                return;
            }
        }
    }

    public void removeItem(string tag)
    {
        if(!offline)
        if (!PV.IsMine) return;

        int i;
        for (i = 0; i < inventoryMaxATM; i++) if (inventoryItems[i].GetIE().powerupName == tag)
            {
                inventoryItems[i].RightClick();

                if (offline)
                    RPC_DestroyComponent(inventoryItems[i].GetType().FullName);
                else
                PV.RPC("RPC_DestroyComponent", RpcTarget.All, inventoryItems[i].GetType().FullName);
                //Destroy(inventoryItems[i] as MonoBehaviour);

                inventoryItems[i] = null;
                inventory.RemoveUIElement(tag);
                if (selectedAbility >= i) { selectedAbility--; SelectAbility(selectedAbility); }
                break;
            }
        if (i == inventoryMaxATM)
        {
            if (inventoryItems[inventoryMaxATM].GetIE().powerupName == tag)
            {
                inventoryItems[inventoryMaxATM].RightClick();


                if (offline)
                    RPC_DestroyComponent(inventoryItems[inventoryMaxATM].GetType().FullName);
                else
                    PV.RPC("RPC_DestroyComponent", RpcTarget.All, inventoryItems[inventoryMaxATM].GetType().FullName);
                //Destroy(inventoryItems[inventoryMaxATM] as MonoBehaviour);
                inventoryItems[inventoryMaxATM] = null;
                inventory.RemoveUIElement(tag);
                if (selectedAbility == inventoryMaxATM) { selectedAbility--; SelectAbility(selectedAbility); }
                inventoryMaxATM--;
                return;
            }
            else
            {
                Debug.LogWarning(tag + " was not in inventory");
                return;
            }
        }

        for (; i < inventoryMaxATM; i++) inventoryItems[i] = inventoryItems[i + 1];

        inventoryItems[inventoryMaxATM] = null;
        inventoryMaxATM--;
    }

    public void ClearInventory()
    {
        for (int i = inventoryMaxATM; i > 0; i--)
        {
            inventory.RemoveUIElement(inventoryItems[i].GetIE().powerupName);
            inventoryItems[i].RightClick();
            if (offline)
                RPC_DestroyComponent(inventoryItems[i].GetType().FullName);
            else
            PV.RPC("RPC_DestroyComponent", RpcTarget.All, inventoryItems[i].GetType().FullName);
            //Destroy(inventoryItems[i] as MonoBehaviour);

            inventoryItems[i] = null;
        }
        SelectAbility(0);
        inventoryMaxATM = 0;
    }

    void SelectAbility(int abilityIndex)
    {
        if (abilityIndex > inventoryMaxATM || abilityIndex < 0)
        {
            //Debug.LogError(abilityIndex + "is not in inventory");
            return;
        }

        if (abilityIndex == selectedAbility) return;

        inventoryItems[selectedAbility].RightClick();
        selectedAbility = abilityIndex;
        inventory.Select(inventoryItems[selectedAbility].GetIE().powerupName);
    }

    [PunRPC]
    void RPC_AddComponent(string type, string tag)
    {
        (gameObject.AddComponent(System.Type.GetType(type)) as IAbility).SetUp(tag);
    }

    [PunRPC]
    void RPC_DestroyComponent(string componentType)
    {
        System.Type type = System.Type.GetType(componentType);
        if (gameObject.GetComponent(type) != null)
            Destroy(gameObject.GetComponent(type) as MonoBehaviour);
        else
        if (gameObject.GetComponentInChildren(type) != null)
            Destroy(gameObject.GetComponentInChildren(type) as MonoBehaviour);
    }
}
