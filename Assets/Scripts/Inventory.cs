using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using SpaceBallAbilities;

public class Inventory : MonoBehaviour
{
    PhotonView PV;
    public static InventoryUIManager inventory;
    List<System.Type> itemComponents = new List<System.Type> { typeof(SpaceBallAbilities.GravityGun), typeof(SpaceBallAbilities.Grapple), 
                                                                typeof(SpaceBallAbilities.ImpulseCannon), typeof(Coin)};
    Dictionary<string, System.Type> typeLookUp = new Dictionary<string, System.Type>();
    
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
    public GameObject hookPrefab;

    [Header("Impulse Gun Settings")]
    public GameObject impulseGunHolder;
    public GameObject particleSystem;
    public float pushForce;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        inventory = InventoryUIManager.inventory;
        if (!PV.IsMine) return;

        typeLookUp.Add("Impulse Gun", typeof(SpaceBallAbilities.ImpulseCannon));
        typeLookUp.Add("Grapple Gun", typeof(SpaceBallAbilities.Grapple));
        typeLookUp.Add("Gravity Gun", typeof(SpaceBallAbilities.GravityGun));
        typeLookUp.Add("Coin", typeof(Coin));
        inventoryItems = new IAbility[inventorySize];

        for (int i = 1; i < inventorySize; i++)
            inventoryItems[i] = null;
        
        inventoryMaxATM = 0;
        inventoryItems[0] = impulseGunHolder.AddComponent(itemComponents[2]) as IAbility;
        inventoryItems[0].SetUp();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine) return;

        if (inventoryMaxATM < 0)
            return;

        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
        {
            selectedAbility = Mathf.Min(selectedAbility + 1, inventoryMaxATM);
            inventory.Select(inventoryItems[selectedAbility].GetIE().powerupName);
        }

        if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            selectedAbility = Mathf.Max(0, selectedAbility - 1);
            inventory.Select(inventoryItems[selectedAbility].GetIE().powerupName);
        }

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
        if (inventoryMaxATM == inventorySize) return;

        if(notNewPowerups.Contains(tag)) //spanw tool tip;

        for (int i = 0; i < inventorySize; i++)
        {
            if (inventoryItems[i] == null)
            {
                inventoryItems[i] = gameObject.AddComponent(typeLookUp[tag]) as IAbility;
                inventoryItems[i].SetUp();
                inventoryMaxATM = i;
                return;
            }

            if (inventoryItems[i].GetIE().powerupName == tag)
            {
                // Reset timer or add to it or something
                return;
            }
        }
    }

    public void removeItem(string tag)
    {
        int i;
        for (i = 0; i < inventoryMaxATM; i++) if (inventoryItems[i].GetIE().powerupName == tag)
            {
                Destroy(inventoryItems[i] as MonoBehaviour);

                inventoryItems[i] = null;
                inventory.RemoveUIElement(tag);
                break;
            }
        if (i == inventoryMaxATM)
        {
            if (inventoryItems[inventoryMaxATM].GetIE().powerupName == tag)
            {
                Destroy(inventoryItems[inventoryMaxATM] as MonoBehaviour);
                inventoryItems[inventoryMaxATM] = null;
                inventory.RemoveUIElement(tag);
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
}
