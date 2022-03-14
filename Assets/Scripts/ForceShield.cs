using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceShield : MonoBehaviour
{
    // Start is called before the first frame update
    public int generatorDestroyed = 0;
    // green is 0 and red is 1
    public int team;

    // Update is called once per frame
    void Update()
    {
        if (generatorDestroyed >= 3)
        {
            Destroy(this.gameObject);
        }
    }
}
