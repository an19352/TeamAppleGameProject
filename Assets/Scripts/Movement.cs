using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Movement : MonoBehaviour
{
    PhotonView PV;
    public static GameMechanics gameMechanics;

    public bool isNPC;
    public Material highlightedGreenMaterial;
    public Material highlightedRedMaterial;

    Transform player;
    Rigidbody playerBody;
    private Camera cameraMain;

    public float speed = 5f;

    private int ID;               // ID is private so it can't be changed from inspector

    public float rotationSpeed;
    private Vector3 lookDirection;
    private Quaternion lookRotation;
    private Vector3 mouseLocation;

    public string horizontalAxis;
    public string verticalAxis;
    public LayerMask ignoredLayers;
    public GameObject shadow;
    GameObject shadowInsatance;
    public LayerMask shadowMask;
    private Vector3 move;

    [HideInInspector]
    public Text currentPowerup;
    private bool cooldownActive;

    [HideInInspector]
    public bool hasPowerup;

    private void Start()
    {
        PV = GetComponent<PhotonView>();
        gameMechanics = GameMechanics.gameMechanics;
        shadowInsatance = Instantiate(shadow);

        player = transform;
        playerBody = GetComponent<Rigidbody>();
        cameraMain = Camera.main;

        if (PV.IsMine)
        {
            int team = gameMechanics.checkTeam(ID);
            if (team == 1)
            {
                GetComponent<Renderer>().material = highlightedGreenMaterial;
            }
            else
            {
                GetComponent<Renderer>().material = highlightedRedMaterial;
            }
            cameraMain.GetComponent<FollowPlayer>().player = transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isNPC) return;
        if (!PV.IsMine) return;

        //Keyboard controls

        float x = Input.GetAxis(horizontalAxis);
        float z = Input.GetAxis(verticalAxis);
        Vector3 move = new Vector3(x * speed * Time.fixedDeltaTime, playerBody.velocity.y, z * speed * Time.fixedDeltaTime);
        //player.position = (player.position + move * speed * Time.deltaTime);
        playerBody.velocity = (move);

        float step = rotationSpeed * Time.deltaTime;

        Ray mouseRay = cameraMain.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mouseRay, out RaycastHit hit, 1000f, ~ignoredLayers))
        {
            mouseLocation = hit.point;
            lookDirection = (mouseLocation - player.position).normalized;
            lookRotation = Quaternion.LookRotation(lookDirection);
            lookRotation.x = 0f;
            lookRotation.z = 0f;
        }

        //player.position = Vector3.MoveTowards(player.position, move, step);
        player.rotation = Quaternion.Slerp(player.rotation, lookRotation, step);

        Fire();
    }

    void LateUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, shadowMask))
        {
            shadowInsatance.SetActive(true);
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.green);
            shadowInsatance.transform.position = transform.position + Vector3.down * hit.distance;
        }
        else shadowInsatance.SetActive(false);
    }

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
        if (spawnPointID < 0)
            player.position = new Vector3(0f, 4f, 0f);
        else
            player.position = gameMechanics.spawnPpoints[spawnPointID].position;

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

    public void Call_Score()
    {
        //if (!PV.IsMine) return;

        int team = (1 + gameMechanics.checkTeam(ID)) % 2;    // Only works for 2 teams
        gameMechanics.RPC_Score(team);

    }
}
