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
    
    [Range(1, 5)]
    public int inventorySize; 
    IAbility[] inventoryItems;
    int selectedAbility, inventoryMaxATM;

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
    public float pushForce;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        inventory = InventoryUIManager.inventory;
        if (!PV.IsMine) return;

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
            selectedAbility = Mathf.Min(selectedAbility + 1, inventoryMaxATM);

        if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
            selectedAbility = Mathf.Max(0, selectedAbility - 1);
            
        inventory.UpdateQueue(inventoryItems[selectedAbility].GetIE().powerupName);

        if (Input.GetButtonDown("Fire1"))
            inventoryItems[selectedAbility].LeftClick();

        if (Input.GetButtonDown("Fire2"))
            inventoryItems[selectedAbility].RightClick();
    }

    public void activateItem(int index)
    {
        if (index >= itemComponents.Count || index < 0) 
        {
            Debug.LogWarning("Ability not found");
            return; 
        }
        if (inventoryMaxATM == inventorySize) return;

        for (int i = 0; i < inventorySize; i++)
        {
            if (inventoryItems[i] == null)
            {
                inventoryItems[i] = gameObject.AddComponent(itemComponents[index]) as IAbility;
                inventoryItems[i].SetUp();
                inventoryMaxATM = i;
                return;
            }

            if (inventoryItems[i].GetType() == itemComponents[index])
            {
                // Reset timer or add to it or something
                return;
            }
        }
    }

    public void removeItem(int index)
    {
        if (index >= itemComponents.Count || index < 0)
        {
            Debug.LogWarning("Ability not found");
            return;
        }

        bool ok = false;
        for (int i = 0; i < inventoryMaxATM; i++) if (inventoryItems[i].GetType() == itemComponents[index])
            {
                Destroy(inventoryItems[i] as MonoBehaviour);
                inventoryItems[i] = inventoryItems[i + 1];
                ok = true;
            }
            else if (ok) inventoryItems[i] = inventoryItems[i + 1];

        if (ok || inventoryItems[inventoryMaxATM].GetType() == itemComponents[index])
        {
            Destroy(inventoryItems[inventoryMaxATM] as MonoBehaviour);
            inventoryItems[inventoryMaxATM] = null;
            inventoryMaxATM--;
        }
    }
}
