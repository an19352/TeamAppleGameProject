using UnityEngine;
using UnityEngine.UI;

public class WeaponWheelController : MonoBehaviour
{

    public Animator anim;
    private bool weaponWheelSelected = false;
    public Image selectedItem;
    public Sprite noImage;
    public static int weaponID;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            weaponWheelSelected = !weaponWheelSelected;
        }
        if (weaponWheelSelected)
        {
            anim.SetBool("OpenWeaponWheel", true);
        }
        else
        {
            anim.SetBool("OpenWeaponWheel", false);
        }
        switch (weaponID)
        {
            case 0:
                selectedItem.sprite = noImage;
                break;
            case 1:

                break;
            case 2:

                break;
            case 3:

                break;
            case 4:

                break;
            case 5:

                break;
        }
    }
}
