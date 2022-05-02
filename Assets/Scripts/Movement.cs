using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Movement : MonoBehaviour, IPunObservable
{
    PhotonView PV;
    public static GameMechanics gameMechanics;

    public bool isNPC;
    public Material highlightedBlueMaterial;
    public Material highlightedRedMaterial;
    public Rigidbody playerBody;

    Transform player;
    private Camera cameraMain;

    public float speed = 5f;
    public float frictionCoef = 1.2f;
    public float gravityStrength = 9.8f;
    public float jumpForce = 13f;
    Vector3 currentVelocity;
    bool isGrounded;

    int ID;           // ID is private so it can't be changed from inspector

    public float rotationSpeed;
    Vector3 lookDirection;
    Quaternion lookRotation;
    Vector3 mouseLocation;

    public string horizontalAxis;
    public string verticalAxis;
    public LayerMask mousecastPlane;
    public GameObject shadow;
    GameObject shadowInsatance;
    public LayerMask shadowMask;
    Vector3 move;
    Vector3 networkPosition;
    Quaternion networkRotation;
    public float maxDiscDistance = 3f;

    bool jumpInput = false;
    float xAxisInput = 0f;
    float zAxisInput = 0f;

    [HideInInspector]
    public Text currentPowerup;
    bool cooldownActive;

    [HideInInspector]
    public bool hasPowerup;

    public GameObject plane;

    private void Start()
    {
        PV = GetComponent<PhotonView>();
        gameMechanics = GameMechanics.gameMechanics;
        shadowInsatance = Instantiate(shadow);

        //Debug.Log(gameObject.transform.GetChild(9).GetChild(0).GetChild(2).GetChild(0).GetChild(11).GetChild(7).GetComponent<Renderer>().material);
        
        player = transform;
        cameraMain = Camera.main;

        if (PV.IsMine && gameMechanics != null)
        {
            plane.SetActive(true);
            int team = gameMechanics.checkTeam(ID);
            if (team == 1)
            {
                //GetComponent<Renderer>().material = highlightedGreenMaterial;
                //gameObject.transform.GetChild(9).GetChild(0).GetChild(0).GetChild(0).GetComponent<Renderer>().material = 
                //gameObject.transform.GetComponent<robotRenderer>().teamcolor();
                gameObject.transform.GetChild(9).GetChild(0).GetChild(2).GetChild(0).GetChild(11).GetChild(7)
                    .GetComponent<Renderer>().material = highlightedBlueMaterial;
            }
            else
            {
                gameObject.transform.GetChild(9).GetChild(0).GetChild(2).GetChild(0).GetChild(11).GetChild(7).GetComponent<Renderer>().material = highlightedRedMaterial;
            }
            cameraMain.GetComponent<FollowPlayer>().player = transform;
        }
    }

    void Update()
    {
        if (!PV.IsMine) return;
        
        jumpInput = Input.GetButton("Jump");

        zAxisInput = -Input.GetAxis(horizontalAxis);
        xAxisInput = Input.GetAxis(verticalAxis);
    }

    void FixedUpdate()
    {
        if (isNPC) return;
        if (!PV.IsMine)
        {   // Move other players according to the data they sent over the network... teleport them if they are too far away
            if (Vector3.Distance(networkPosition, playerBody.position) > maxDiscDistance)
            {
                playerBody.position = networkPosition;
                playerBody.rotation = Quaternion.RotateTowards(playerBody.rotation, networkRotation, Time.fixedDeltaTime * 100f);
                return;
            }

            playerBody.position = Vector3.MoveTowards(playerBody.position, networkPosition, Time.fixedDeltaTime);
            playerBody.rotation = Quaternion.RotateTowards(playerBody.rotation, networkRotation, Time.fixedDeltaTime * 100f);
            return;
        }
        //Debug.Log(playerBody.velocity);
        currentVelocity = new Vector3(playerBody.velocity.x, 0, playerBody.velocity.z);   // #LeaveGravityAlone

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
        if (isGrounded)
            if (jumpInput)
                playerBody.AddForce(transform.up * jumpForce, ForceMode.Acceleration);

        playerBody.AddForce(Vector3.down * gravityStrength, ForceMode.Acceleration);

        Fire();
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
    /*
    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }*/

    void Fire()
    {
        //Starts cooldown coroutine if key is pressed, powerup is possessed and cooldown is not active
        if (Input.GetKeyDown(KeyCode.V) && !cooldownActive && hasPowerup)
        {
            StartCoroutine(ActivateCooldown(5));
        }
    }

    //Coroutine to activate a cooldown for a set period of time
    IEnumerator ActivateCooldown(int time)
    {
        cooldownActive = true;
        hasPowerup = false;
        currentPowerup.text = "No Powerup";
        yield return new WaitForSeconds(time);
        cooldownActive = false;
    }

    // ReSpawn mechanic
    public void Spawn(int spawnPointID = -1)
    {

        float greenX = Random.Range(-145, -145);
        float greenZ = Random.Range(-16, 16);
        float redX = Random.Range(145, 145);
        float redZ = Random.Range(-16, 16);
        int team = 0;

        if (gameMechanics == null)
        {
            player.position = new Vector3(0, 4, 0);
        }
        else
        {
            team = gameMechanics.checkTeam(ID);

            /*if (spawnPointID < 0)
                player.position = new Vector3(0f, 4f, 0f);
            else
                player.position = gameMechanics.spawnPpoints[spawnPointID].position;*/

            if (team == 1)
            {
                player.position = new Vector3(greenX, 6, greenZ);
            }
            else
            {
                player.position = new Vector3(redX, 6, redZ);
            }
        }

        if (gameMechanics != null)
            GetComponent<Inventory>().ClearInventory();
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

    public void PushMe(Vector3 force, ForceMode mode)
    {
        PV.RPC("RPC_PushMe", PhotonView.Find(PV.ViewID).Owner, force, mode);
    }

    [PunRPC]
    public void RPC_PushMe(Vector3 force, ForceMode mode)
    {
        GetComponent<Rigidbody>().AddForce(force, mode);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(playerBody.position);
            stream.SendNext(playerBody.rotation);
            stream.SendNext(playerBody.velocity);
        }
        else if (stream.IsReading)
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
            playerBody.velocity = (Vector3)stream.ReceiveNext();

            if (playerBody.transform.parent != null)
                if (playerBody.transform.parent.gameObject.TryGetComponent(out AddConstantVelocity velocity))
                    playerBody.velocity += velocity.force;

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            networkPosition += playerBody.velocity * lag;
        }
    }

    public void Ground(bool value) { isGrounded = value; }

    private void OnDestroy()
    {
        Destroy(shadowInsatance);
    }

    public void SelfDestruct()
    {
        if (PV.IsMine)
            PhotonNetwork.Destroy(gameObject);
    }

    private void OnApplicationQuit()
    {
        if (gameMechanics != null)
            gameMechanics.RPC_RemovePlayer(ID);
        PhotonNetwork.Disconnect();
    }
}
