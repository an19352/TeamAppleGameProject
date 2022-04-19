using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Inventory Element", menuName = "Inventory Element")]
public class InventoryElement : ScriptableObject
{
    public string powerupName;
    public Sprite icon;

    public bool Infinite;
    public float timeToDie;
    public string associatedClass;
    [TextArea(10, 100)]
    public string tooltipText;
}
