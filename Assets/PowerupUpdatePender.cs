using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupUpdatePender : MonoBehaviour
{
    ObjectPooler objectPooler;
    PowerupGenerator powerupGenerator;

    // Start is called before the first frame update
    void Start()
    {
        objectPooler = ObjectPooler.OP;
        powerupGenerator = GetComponentInParent<PowerupGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        bool trigger = objectPooler.isSynced();
        if (!trigger) return;

        powerupGenerator.SetPowerupTags();
        powerupGenerator.ParentPowerups();

        if (powerupGenerator.GetPowerupTagsCount() > 0) gameObject.SetActive(false);
    }
}
