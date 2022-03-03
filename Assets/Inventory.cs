using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using SpaceBallAbilities;

public class Inventory : MonoBehaviour
{
    PhotonView PV;
    List<string> leftClickFunctions = new List<string> { "GravityGunLeftClick", "GrappleGunLeftClick", "ImpulseGunLeftClick" };
    List<string> rightClickFunctions = new List<string> { "GravityGunRightClick", "GrappleGunRightClick", null };
    SpaceBallAbilities.GravityGun gravityGun;
    SpaceBallAbilities.Grapple grappleGun;
    
    [Range(1, 5)]
    public int inventorySize; 
    int[] activeAbilities;
    int selectedAbility, inventoryMaxATM;

    [Header("Gravity Gun Settings")]
    public Transform objectHolder;
    public float maxGrabDistance = 3f, throwForce = 20f, lerpSpeed = 10f;
    Rigidbody grabbedRB;

    [Header("Grapple Gun Settings")]
    public Transform shootTransform;
    public float pullSpeed = 0.5f;
    public float maxShootDistance = 20f;
    public float stopPullDistance = 0.2f;
    public float hookLifetime = 8f;
    public GameObject hookPrefab;
    Rigidbody rigid;

    [Header("Impulse Gun Settings")]
    public ImpulseCannon impulseGun;
    public float pushForce;

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();

        gravityGun = new SpaceBallAbilities.GravityGun();
        gravityGun.SetVariables(PV, maxGrabDistance, throwForce, objectHolder);

        rigid = GetComponent<Rigidbody>();
        grappleGun = new SpaceBallAbilities.Grapple();
        grappleGun.SetVariables(rigid, pullSpeed, maxShootDistance, stopPullDistance, hookLifetime, hookPrefab, shootTransform);

        if (!PV.IsMine) return;

        activeAbilities = new int[inventorySize];
        for (int i = 0; i < inventorySize; i++) activeAbilities[i] = -1;
        inventoryMaxATM = 0;
        activeAbilities[0] = 2;
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

        if (Input.GetButtonDown("Fire1"))
            Invoke(leftClickFunctions[activeAbilities[selectedAbility]], 0f);

        if (Input.GetButtonDown("Fire2"))
            if(rightClickFunctions[activeAbilities[selectedAbility]] != null)
                Invoke(rightClickFunctions[activeAbilities[selectedAbility]], 0f);
    }

    public void activateItem(int index)
    {
        if (index >= leftClickFunctions.Count || index < 0) 
        {
            Debug.LogWarning("Ability not found");
            return; 
        }
        if (inventoryMaxATM == inventorySize) return;

        for (int i = 0; i < inventorySize; i++)
        {
            if (activeAbilities[i] == index)
            {
                // Reset timer or add to it or something
                return;
            }

            if (activeAbilities[i] == -1)
            {
                inventoryMaxATM = i;
                activeAbilities[i] = index;
                return;
            }
        }
    }

    public void removeItem(int index)
    {
        if (index >= leftClickFunctions.Count || index < 0)
        {
            Debug.LogWarning("Ability not found");
            return;
        }

        bool ok = false;
        for (int i = 0; i < inventoryMaxATM; i++) if(activeAbilities[i] == index || ok)
            {
                activeAbilities[i] = activeAbilities[i + 1];
                ok = true;
            }

        if (ok || activeAbilities[inventoryMaxATM] == index)
        {
            activeAbilities[inventoryMaxATM] = -1;
            inventoryMaxATM--;
        }
    }

    void GravityGunLeftClick()
    {
        grabbedRB = gravityGun.LeftClick(grabbedRB);
    }

    void GravityGunRightClick()
    {
        grabbedRB = gravityGun.RightClick(grabbedRB);
    }

    void GrappleGunLeftClick()
    {
        grappleGun.LeftClick();
    }

    void GrappleGunRightClick()
    {
        grappleGun.RightClick();
    }

    void ImpulseGunLeftClick()
    {
        impulseGun.Fire();
    }
}
