using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Movement : MonoBehaviour
{
    public Transform player;
    public Rigidbody playerBody;
    public Camera cameraMain;

    public float speed = 5f;

    private int ID;               // ID is private so it can't be changed from inspector

    public float rotationSpeed;
    private Vector3 lookDirection;
    private Quaternion lookRotation;
    private Vector3 mouseLocation;

    public string horizontalAxis;
    public string verticalAxis;
    public bool isUsingMouse;

    private Vector3 move;

    // Update is called once per frame
    void Update()
    {
       if (Input.GetKeyDown(KeyCode.F))
       {
            player.rotation = new Quaternion(0f, 0f, 0f, 0f);
            playerBody.angularVelocity = new Vector3(0f, 0f, 0f);
            playerBody.velocity = new Vector3(0f, 0f, 0f);
       }

            
        //Keyboard controls
            
        float x = Input.GetAxis(horizontalAxis);
        float z = Input.GetAxis(verticalAxis);
        Vector3 move = new Vector3(x, 0f, z);
        player.position = (player.position + move * speed * Time.deltaTime);
        
        float step = rotationSpeed * Time.deltaTime;

        Ray mouseRay = cameraMain.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mouseRay, out RaycastHit hit))
        {
            if (hit.transform.CompareTag("Ground"))
            {
                //move = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                mouseLocation = hit.point;
                lookDirection = (mouseLocation - player.position).normalized;
                lookRotation = Quaternion.LookRotation(lookDirection);
                lookRotation.x = 0f;
                lookRotation.z = 0f;
            }
        }

        
        //player.position = Vector3.MoveTowards(player.position, move, step);
        player.rotation = Quaternion.Slerp(player.rotation, lookRotation, step);
        
            
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
