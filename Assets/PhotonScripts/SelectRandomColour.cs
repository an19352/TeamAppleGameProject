using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectRandomColour : MonoBehaviour
{
    public List<Material> AllowedColours;

    // Start is called before the first frame update
    void Start()
    {
        int randdomIndex = Random.Range(0, AllowedColours.Count);
        Renderer render = GetComponent<Renderer>();
        render.material = AllowedColours[randdomIndex];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
