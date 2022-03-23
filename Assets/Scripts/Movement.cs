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
    public Material highlightedGreenMaterial;
    public Material highlightedRedMaterial;
    public Rigidbody playerBody;

    Transform player;
    private Camera cameraMain;

    public float speed = 5f;
    public float frictionCoef = 1.2f;
    Vector3 lastFrameVelocity = new Vector3(0, 0, 0);
    Vector3 currentVelocity;
    Vector3 acceleration = new Vector3(0, 0, 0);

    int ID;               // ID is private so it can't be changed from inspector

    public float rotationSpeed;
    Vector3 lookDirection;
    Quaternion lookRotation;
    Vector3 mouseLocation;

    public string horizontalAxis;
    public string verticalAxis;
    public LayerMask ignoredLayers;
    public GameObject shadow;
    GameObject shadowInsatance;
    public LayerMask shadowMask;
    Vector3 move;
    Vector3 networkPosition;
    Quaternion networkRotation;
    public float maxDiscDistance = 3f;

    [HideInInspector]
    public Text currentPowerup;
    bool cooldownActive;

    [HideInInspector]
    public bool hasPowerup;

    private void Start()
    {
        PV = GetComponent<PhotonView>();
        gameMechanics = GameMechanics.gameMechanics;
        shadowInsatance = Instantiate(shadow);

        player = transform;
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

    void FixedUpdate()
    {
        if (isNPC) return;
        if (!PV.IsMine)
        {
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

        currentVelocity = new Vector3(playerBody.velocity.x, 0, playerBody.velocity.z);
        acceleration = (currentVelocity - lastFrameVelocity);
        //Keyboard controls

        float x = Input.GetAxis(horizontalAxis);
        float z = Input.GetAxis(verticalAxis);
        Vector3 move = new Vector3(x * speed, 0, z * speed);
        //player.position = (player.position + move * speed * Time.deltaTime);
        if (currentVelocity.magnitude > 23f)
            playerBody.AddForce(move - currentVelocity, ForceMode.VelocityChange);
        else
            playerBody.velocity = new Vector3(0, playerBody.velocity.y, 0) + move;

        //if (Time.time % 2 == 0) Debug.Log(Vector3.Dot(currentVelocity, lastFrameVelocity) / (currentVelocity.magnitude * lastFrameVelocity.magnitude));
        //DebugText.text = (Vector3.Dot(currentVelocity, lastFrameVelocity) / (currentVelocity.magnitude * lastFrameVelocity.magnitude)).ToString();

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

        lastFrameVelocity = currentVelocity;
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

    public void PushMe(Vector3 force, ForceMode mode, int PVID)
    {
        PV.RPC("RPC_PushMe", PhotonView.Find(PVID).Owner, force, mode);
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

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.timestamp));
            networkPosition += playerBody.velocity * lag;
        }
    }

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
        gameMechanics.RPC_RemovePlayer(ID);
        PhotonNetwork.Disconnect();
    }
}
