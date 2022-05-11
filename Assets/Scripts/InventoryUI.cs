using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    struct Item
    {
        public TextMeshProUGUI background_Image;
        public TextMeshProUGUI icon;
        public bool selected;
    }
}
