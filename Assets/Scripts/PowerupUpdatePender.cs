using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupUpdatePender : MonoBehaviour
{
    // This script used to synchronise the powerup generator once Game Mechanics was synchronised
    // It is now redundant
    ObjectPooler objectPooler;
    PowerupGenerator powerupGenerator;

    void Start()
    {
        objectPooler = ObjectPooler.OP;
        powerupGenerator = GetComponentInParent<PowerupGenerator>();
    }

    void Update()
    {
        bool trigger = objectPooler.isSynced();
        if (!trigger) return;

        powerupGenerator.SetPowerupTags();
        powerupGenerator.ParentPowerups();
        GameMechanics.gameMechanics.SyncPowerupsNow();

        if (powerupGenerator.GetPowerupTagsCount() > 0) gameObject.SetActive(false);
    }
}
