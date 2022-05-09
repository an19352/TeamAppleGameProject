using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineMovement : MonoBehaviour
{
    Rigidbody playerBody;
    Transform player;
    Camera cameraMain;

    public GameObject plane;
    public bool isNPC;

    public GameObject shadow;
    GameObject shadowInsatance;
    public LayerMask shadowMask;
    public LayerMask mousecastPlane;

    public Material highlightedMaterial;

    bool jumpInput;
    float xAxisInput, zAxisInput;
    bool isGrounded;
    bool isFracture;
    Vector3 SpawningPosition;

    public float speed;
    public float frictionCoef = 1.2f;
    public float gravityStrength = 9.8f;
    public float jumpForce = 13f;

    public float rotationSpeed;
    Vector3 lookDirection;
    Quaternion lookRotation;
    Vector3 mouseLocation;

    public GameObject pushedEffect;


    // Start is called before the first frame update
    private void Start()
    {
        playerBody = GetComponent<Rigidbody>();
        shadowInsatance = Instantiate(shadow);
        player = transform;
        cameraMain = Camera.main;
        transform.GetChild(0).gameObject.SetActive(true);
        plane.SetActive(true);
        gameObject.transform.GetChild(9).GetChild(0).GetChild(2).GetChild(0).GetChild(11).GetChild(7)
                .GetComponent<Renderer>().material = highlightedMaterial;

        if(!isNPC)
        GetComponent<Inventory>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isNPC) return;
        jumpInput = Input.GetButton("Jump");

        zAxisInput = Input.GetAxis("Vertical");
        xAxisInput = Input.GetAxis("Horizontal");
        if (isGrounded) SpawningPosition = new Vector3(transform.position.x,(transform.position.y+3f),transform.position.z);
    }

    void FixedUpdate()
    {
        Vector3 currentVelocity = new Vector3(playerBody.velocity.x, 0, playerBody.velocity.z);   // #LeaveGravityAlone

        //Keyboard controls
        float x = xAxisInput;
        float z = zAxisInput;
        Vector3 move = new Vector3(x * speed, 0, z * speed);        // The direction the player wants to move in
        if (currentVelocity.magnitude > speed + 5f)
            playerBody.AddForce(move - playerBody.velocity, ForceMode.Acceleration);
        // If the force is greater then what the player can do, let it play out
        else
            playerBody.velocity = Vector3.Lerp(playerBody.velocity, new Vector3(0, playerBody.velocity.y, 0) + move, frictionCoef * Time.deltaTime);

        // This part ^ allows the player to beat his own input force and turn around quickly
        float step = rotationSpeed * Time.deltaTime;                // frictionCoef controls how easy it is for the player to fully change moving direction

        Ray mouseRay = cameraMain.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mouseRay, out RaycastHit hit, 1000f, mousecastPlane))
        {
            mouseLocation = hit.point;
            lookDirection = (mouseLocation - player.position).normalized;
            lookRotation = Quaternion.LookRotation(lookDirection);  // Calculte the angle needed to rotate on the y axis to look at the cursor
            lookRotation.x = 0f;
            lookRotation.z = 0f;
        }

        player.rotation = Quaternion.Slerp(player.rotation, lookRotation, step);
        if (isGrounded || isFracture)
            if (jumpInput)
                playerBody.AddForce(transform.up * jumpForce, ForceMode.Acceleration);

        playerBody.AddForce(Vector3.down * gravityStrength, ForceMode.Acceleration);

        //Fire();
    }

    void LateUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, shadowMask))
        {
            shadowInsatance.SetActive(true);
            shadowInsatance.transform.position = transform.position + Vector3.down * hit.distance;
        }
        else shadowInsatance.SetActive(false);
    }

    public void Spawn(int spawnPointID = -1)
    {
        player.position = SpawningPosition;
        player.rotation = new Quaternion(0f, 0f, 0f, 0f);

        playerBody.angularVelocity = new Vector3(0f, 0f, 0f);
        playerBody.velocity = new Vector3(0f, 0f, 0f);
    }

    public void RPC_PushMe(Vector3 force, ForceMode mode, bool isExplosion)
    {
        if(isExplosion)
        {
            Instantiate(pushedEffect, transform.position, transform.rotation);
        }
        GetComponent<Rigidbody>().AddForce(force, mode);
    }

    public void Ground(bool value) { isGrounded = value; }
    
    public void Fracture(bool value) { isFracture = value; }

    private void OnDestroy()
    {
        Destroy(shadowInsatance);
    }
}
