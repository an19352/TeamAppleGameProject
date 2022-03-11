using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager inventory;

    public Sprite prefabspr;
    public float verticalOffset = 0f;
    public List<InventoryElement> IEs;

    public GameObject prefab;
    Queue<InventoryUIElement> UIelements;

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
        DontDestroyOnLoad(this.gameObject);
    }

    #endregion

    void Start()
    {
        UIelements = new Queue<InventoryUIElement>(); 
    }

    void RenderUIelements()
    {
        InventoryUIElement _element;
        RectTransform rect;

        _element = UIelements.Dequeue();
        _element.Select();
        rect = _element.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-70, 16);

        int count = UIelements.Count;
        UIelements.Enqueue(_element);

        for (int i = 1; i <= count; i++)
        {
            _element = UIelements.Dequeue();
            _element.Deselect();
            rect = _element.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(-70, 16 + i * verticalOffset);

            UIelements.Enqueue(_element);
        }
    }

    public void AddUIElement(string tag)
    {
        InventoryElement powerup = GetIE(tag);
        int count = UIelements.Count;
        bool ok = false;
        InventoryUIElement _element;

        for (int i = 0; i < count; i++)
        {
            _element = UIelements.Dequeue();
            if (_element.powerupName.text == powerup.powerupName)
                ok = true;
            UIelements.Enqueue(_element);
        }

        if (ok)
        {
            Debug.LogWarning(powerup.powerupName + " was added twice");
            return;
        }

        _element = Instantiate(prefab, transform.parent).GetComponent<InventoryUIElement>();
        _element.SetPowerup(powerup);

        UIelements.Enqueue(_element);
        Debug.LogWarning("Added " + powerup.powerupName);
        RenderUIelements();
    }

    public void RemoveUIElement(string tag)
    {
        bool ok = false;

        int count = UIelements.Count;
        for(int i = 0; i < count; i++)
        {
            InventoryUIElement _element = UIelements.Dequeue();
            if(_element.powerupName.text == tag)
            {
                Destroy(_element.gameObject);
                ok = true;
                Debug.LogWarning("Removed " + tag);
            }
            else
                UIelements.Enqueue(_element);
        }
       
        if(!ok) Debug.LogWarning(tag + " does not exist");

        RenderUIelements();
    }

    public InventoryElement GetIE(string tag) 
    {
        foreach(InventoryElement IE in IEs)
            if (IE.powerupName == tag) return IE;

        Debug.LogWarning("Inventory element " + tag + " not found");
        return null;
    }

    public void UpdateQueue(string tag)
    {
        if (UIelements.Peek().powerupName.text == tag) return;
        int count = 1;
        InventoryUIElement[] _temp = new InventoryUIElement[UIelements.Count];

        for(_temp[0] = UIelements.Dequeue(); UIelements.Peek().powerupName.text != tag; count++)
             _temp[count] = UIelements.Dequeue();

        for (int i = 0; i < count; i++) UIelements.Enqueue(_temp[i]);

        RenderUIelements();
    }
}
