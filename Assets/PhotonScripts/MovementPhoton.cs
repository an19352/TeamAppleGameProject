using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class MovementPhoton : MonoBehaviour
{
    public Transform player;
    public Rigidbody playerBody;
    public Camera cameraMain;

    public float speed = 5f;

    private int ID;               // ID is private so it can't be changed from inspector

    public string horizontalAxis;
    public string verticalAxis;
    public bool isUsingMouse;

    private Vector3 move;

    public PhotonView view;

    // Update is called once per frame
    void Update()
    {
        if (view.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                player.rotation = new Quaternion(0f, 0f, 0f, 0f);
                playerBody.angularVelocity = new Vector3(0f, 0f, 0f);
                playerBody.velocity = new Vector3(0f, 0f, 0f);
            }


            //Keyboard controls
            if (!isUsingMouse)
            {
                float x = Input.GetAxis(horizontalAxis);
                float z = Input.GetAxis(verticalAxis);
                Vector3 move = new Vector3(x, 0f, z);
                player.Translate(move * speed * Time.deltaTime);
            }

            if (isUsingMouse)
            {
                float step = speed * Time.deltaTime;

                Ray mouseRay = cameraMain.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(mouseRay, out RaycastHit hit))
                {
                    if (hit.transform.CompareTag("Ground"))
                    {
                        move = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                    }
                }
                else
                {
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cameraMain.transform.position.y));
                    move = new Vector3(mousePos.x, player.position.y, mousePos.z);
                }

                if (Input.GetMouseButton(0))
                {
                    player.position = Vector3.MoveTowards(player.position, move, step);
                }
            }
        }
    }

    // ReSpawn mechanic
    public void Spawn()
    {
        player.position = new Vector3(0f, 4f, 0f);
        player.rotation = new Quaternion(0f, 0f, 0f, 0f);
        playerBody.angularVelocity = new Vector3(0f, 0f, 0f);
        playerBody.velocity = new Vector3(0f, 0f, 0f);
    }

    // Easy method to sync with GameMechanics 
    public void SetId(int newID)
    {
        ID = newID;
    }

    // For other classes to read this player's ID
    public int GetId()
    {
        return ID;
    }
}
