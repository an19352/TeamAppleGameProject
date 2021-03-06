using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIElement : MonoBehaviour
{
    // The thing that gets displayed at the bottom right for each powerup you have. It also is responssible for removing the powerup
    Inventory inventory;

    InventoryElement powerup;
    float timeToDie;
    float totalTime;
    bool selected = false;

    [Header("Small Version")]  // Powerup NOT selected
    public Image smallIcon;
    public TextMeshProUGUI smallTimerText;

    [Header("Large Version")]  // Powerup selected
    public Image largeIcon;
    public TextMeshProUGUI powerupName;
    public TextMeshProUGUI largeTimerText;

    [Header("Tooltip")] 
    public TextMeshProUGUI tooltip;
    
    // __init__
    public void SetPowerup(InventoryElement _powerup, Inventory _inv)
    {
        powerup = _powerup;
        inventory = _inv;

        smallIcon.sprite = powerup.icon;
        largeIcon.sprite = powerup.icon;

        powerupName.text = powerup.powerupName;
        tooltip.text = powerup.tooltipText;

        timeToDie = powerup.timeToDie + Time.time;

        if (powerup.Infinite)
        {
            smallTimerText.text = Mathf.Infinity.ToString();
            largeTimerText.text = Mathf.Infinity.ToString();
        }
    }

    void Update()
    {
        if (powerup == null ) return;
        if (smallTimerText.text == Mathf.Infinity.ToString()) return;

        totalTime = timeToDie - Time.time;

        if (totalTime < 0) inventory.removeItem(powerupName.text); // Powerup expired

        string seconds = ((int)totalTime).ToString();
        smallTimerText.text = seconds;
        largeTimerText.text = seconds;
    }

    public bool GetSelected() { return selected; }

    public void Select()
    {
        if (selected) return;
        smallIcon.transform.parent.gameObject.SetActive(false);
        largeIcon.transform.parent.gameObject.SetActive(true);
        tooltip.transform.gameObject.SetActive(true);
        selected = true;
    }

    public void AddSeconds(float seconds)
    {
        timeToDie += Mathf.Abs(seconds);
    }

    public void Deselect()
    {
        if (!selected) return;
        largeIcon.transform.parent.gameObject.SetActive(false);
        smallIcon.transform.parent.gameObject.SetActive(true);
        tooltip.transform.gameObject.SetActive(false);
        selected = false;
    }
}
