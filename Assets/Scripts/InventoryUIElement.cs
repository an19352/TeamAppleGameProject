using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIElement : MonoBehaviour
{
    Inventory inventory;

    InventoryElement powerup;
    float timeToDie;
    float totalTime;
    bool selected = false;

    [Header("Small Version")]
    public Image smallIcon;
    public Text smallTimerText;

    [Header("Large Version")]
    public Image largeIcon;
    public Text powerupName;
    public Text largeTimerText;

    public void SetPowerup(InventoryElement _powerup, Inventory _inv)
    {
        powerup = _powerup;
        inventory = _inv;

        smallIcon.sprite = powerup.icon;
        largeIcon.sprite = powerup.icon;

        powerupName.text = powerup.powerupName;

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

        if (totalTime < 0) inventory.removeItem(powerupName.text);

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
        selected = true;
    }

    public void Deselect()
    {
        if (!selected) return;

        largeIcon.transform.parent.gameObject.SetActive(false);
        smallIcon.transform.parent.gameObject.SetActive(true);
        selected = false;
    }
}
