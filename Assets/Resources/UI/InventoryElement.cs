using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Inventory Element", menuName = "Inventory Element")]
public class InventoryElement : ScriptableObject
{
    // This is a scriptable object that holds all the standard information about each powerup like../
    public string powerupName;      // their name
    public Sprite icon;             // the icon displayed in the bottom right corner

    public bool Infinite;           // Is it infinite
    public float timeToDie;         // If not, how much does it last?
    public string associatedClass;  // --------------------------------IMPORTANT: Which Monobehaviour are they associated with-----------------------------------
    [TextArea(10, 100)]
    public string tooltipText;      // What does its tooltip say
}
