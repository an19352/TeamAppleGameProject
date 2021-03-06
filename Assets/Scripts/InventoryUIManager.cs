using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    // This singleton is responssible for the communication between the UIElements and the actual Inventory
    public static InventoryUIManager inventory;

    public Sprite prefabspr;
    public float verticalOffset = 0f;
    List<InventoryElement> IEs;

    public GameObject prefab;
    List<InventoryUIElement> UIelements;
    int selectedIndex = 0;

    #region Singleton

    private void Awake()
    {
        if (InventoryUIManager.inventory == null)
        {
            InventoryUIManager.inventory = this;
        }
        else
        {
            if (InventoryUIManager.inventory != this)
            {
                Destroy(InventoryUIManager.inventory.gameObject);
                InventoryUIManager.inventory = this;
            }
        }
    }

    #endregion

    void Start()
    {
        IEs = new List<InventoryElement>();
        Object[] _objs = Resources.LoadAll("Powerups Settings", typeof(InventoryElement));  // Retrievs the settings for any powerup in the game files
        for (int i = 0; i < _objs.Length; i++) IEs.Add((InventoryElement)_objs[i]);
        UIelements = new List<InventoryUIElement>(); 
    }

    // A powerup script was added to your player. Awesome. Now let's display it and by proxy set a timer do remove it
    public void AddUIElement(string tag, Inventory _inv)
    {
        InventoryElement powerup = GetIE(tag);
        InventoryUIElement _element;

        for (int i = 0; i < UIelements.Count; i++)
            if (UIelements[i].powerupName.text == powerup.powerupName)
            {
                return;
            }

        _element = Instantiate(prefab, transform).GetComponent<InventoryUIElement>();
        _element.SetPowerup(powerup, _inv);

        //if (UIelements.Count == 0) Select(tag);

        UIelements.Add(_element);
    }

    public void AddSecondsToPowerup(string tag, float seconds)
    {
        foreach(InventoryUIElement element in UIelements)
        {
            if(element.powerupName.text == tag)
            {
                element.AddSeconds(seconds);
                return;
            }
        }
    }

    public void RemoveUIElement(string tag)
    {
        for (int i = 0; i < UIelements.Count; i++)
        {
            if(UIelements[i].powerupName.text == tag)
            {
                if (UIelements[i].GetSelected()) Select(UIelements[i-1].powerupName.text);
                
                InventoryUIElement _element = UIelements[i];
                UIelements.RemoveAt(i);
                Destroy(_element.gameObject);
                
                return;
            }
        }
    }

    // Get one of the IEs this script copied from the files
    public InventoryElement GetIE(string tag) 
    {
        foreach(InventoryElement IE in IEs)
            if (IE.powerupName == tag) return IE;
        
        return IEs[0];
    }
    
    // Deselcts the currently selected powerup and Selects the new one
    public void Select(string tag) 
    {
        UIelements[selectedIndex].Deselect();
        for(int i = 0; i < UIelements.Count; i++)
            if(UIelements[i].powerupName.text == tag)
            {
                UIelements[i].Select();
                selectedIndex = i;
                return;
            }

    }

    // Clone the IEs this script loaded from the files
    public InventoryElement[] CloneIEs()
    {
        return IEs.ToArray();
    }
}
