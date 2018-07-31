using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkIdentity))]
public class Player : NetworkBehaviour {

    private Rigidbody2D rbody2d;
    private Vector3 mousePos;

    public float speed = 0.01f;
    public float fireRate;

    [SyncVar]
    private float lastShot = 0f;
    public GameObject projectile;

    [SyncVar(hook ="SetColor")]
    public Color currentColor;
    
    public int numCaptured = 0; //how many other players they've captured

    [SyncVar]
    public int maxHealth = 3;

    [SyncVar]
    public int health = 3;
    
    private Collider2D cCollider;
    private GameManager gameManager;
    
    public string playerName;

    public static Player localPlayer;
    private Vector3 originalScale = Vector3.zero; //original size to revert to when captured

    [SyncVar]
    public int currentSize = 1;
    [SyncVar]
    public float width; //width of player
    [SyncVar]
    public int pWidth; // width of paint trail

    public GameObject PlayerCameraPrefab;
    public GameObject PlayerCameraObject;

    public TextMesh healthText;

    //for speed up powerup
    [SyncVar]
    private bool speedPowerUpActive = false;
    [SyncVar]
    private float speedPowerUpTimeLeft = 10;

    [SyncVar]
    private float startSpeed; // holds original speed to return to when speed powerup ends

    //for trail powerup
    [SyncVar]
    private bool trailPowerUpActive = false;
    [SyncVar]
    private float trailPowerUpTimeLeft = 10;
    [SyncVar] 
    private int startPWidth; // holds original trail size to return to when trail powerup ends

    static int which = 0;
    // Use this for initialization
    void Start() {
        rbody2d = GetComponent<Rigidbody2D>();
        //currentColor = GetComponent<SpriteRenderer>().color;

        originalScale = transform.lossyScale;
        cCollider = GetComponent<Collider2D>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        //syncvars can only change on server. color must be set by server.
        if (isServer)
        {
            pWidth = 3;
            var colors = TextureDrawing.instance.allColors;
             

            //int which = (int)Random.Range(0, colors.Length - 1);
            currentColor = colors[which];

            which = (which + 1) % colors.Count;

            healthText.GetComponent<Renderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;
            startSpeed = speed;
            startPWidth = pWidth;
        }

        if(isLocalPlayer)
        {
            localPlayer = this;
            Debug.Log("IS LOCAL PLAYER");
            PlayerCameraObject = Instantiate(PlayerCameraPrefab);

            var campos = this.transform.position;
            campos.z = PlayerCameraObject.transform.position.z;
            PlayerCameraObject.transform.position = campos;
        }
    }

    //I am the server running this code. Only the server can change things for everyone
    public override void OnStartServer()
    {
        base.OnStartServer();


    }

    void SetColor(Color color)
    {
        currentColor = color;
        GetComponent<SpriteRenderer>().color = color;

    }

    // Update is called once per frame
    void Update() {

        if (isLocalPlayer && PlayerCameraObject != null)
        {
            //var campos = Vector3.MoveTowards(PlayerCameraObject.transform.position, this.transform.position, speed * 0.9f);

            var campos = Vector3.Lerp(PlayerCameraObject.transform.position, this.transform.position, 0.5f * Time.deltaTime);
            campos.z = PlayerCameraObject.transform.position.z;
            PlayerCameraObject.transform.position = campos;
        }
        //if game is not over
        if(gameManager.gameOver == false)
        {
            healthText.text = "" + health;

            //only allow controls for the player you're controlling
            if (isServer)
            {
                

                // if speed power up is active, count down
                if (speedPowerUpActive)
                {
                    if (speedPowerUpTimeLeft > 0)
                    {
                        speedPowerUpTimeLeft -= Time.deltaTime;
                        speed = (float)(startSpeed * 1.5);
                    }
                    if (speedPowerUpTimeLeft <= 0)
                    {
                        speedPowerUpActive = false;
                        speed = startSpeed;
                    }
                }

                // if trail power up is active, count down
                if (trailPowerUpActive)
                {
                    if (trailPowerUpTimeLeft > 0)
                    {
                        trailPowerUpTimeLeft -= Time.deltaTime;
                        pWidth = (int)(startPWidth * 2);
                    }
                    if (trailPowerUpTimeLeft <= 0)
                    {
                        trailPowerUpActive = false;
                        pWidth = startPWidth;
                    }
                }
            }

            if (isLocalPlayer && localPlayerAuthority)
            {
                // Move character towards mouse if right click is held down
                if (Input.GetMouseButton(0))
                {
                    //get position to set ball to
                    var target = ScreenToWorld(Input.mousePosition);

                    transform.position = Vector3.MoveTowards(transform.position, target, speed);

                                      
                    Vector3 pos = transform.position;
                    pos.z = 0;
                    transform.position = pos;

                    //get rotation to set ball to
                    Vector3 difference = ScreenToWorld(Input.mousePosition) - transform.position;
                    difference.Normalize();
                    float z_rotation = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0f, 0f, z_rotation);
                }
                //on left click, fire a projectile
                if (Input.GetMouseButton(1) || Input.GetKeyUp(KeyCode.Space))
                {
                    if (Time.time > fireRate + lastShot)
                    {
                        if (isServer)
                        {
                            FireProjectile();
                        } else
                        {
                            CmdFireProjectile();
                        }
                    }
                }
                


            }
            else
            //temp behavior for non-human players, they just shoot repeatedly
            if ( playerControllerId == -1)
            {
                if (Time.time > fireRate + lastShot)
                {
                    FireProjectile();
                    lastShot = Time.time;
                }
            }
            
        }
    }
    

    // Get world position of mouse click
    Vector3 ScreenToWorld(Vector2 screenPos)
    {
        //Create ray
        Ray ray = PlayerCameraObject.GetComponent<Camera>().ScreenPointToRay(screenPos);
        RaycastHit hit;

        // ray hit an object, return hit position
        if (Physics.Raycast(ray, out hit))
        {
                return hit.point;
        }
        
        //if ray hits nothing, player stays in place
        return transform.position;
        
        
    }

    //When firing, instantiates a new projectile and sets its color to the player's color
    //This can only be run by the server!
    private void FireProjectile()
    {
        if (isServer)
        {
            GameObject clone = Instantiate(projectile) as GameObject;
            NetworkServer.Spawn(clone);

            var proj = clone.GetComponent<Projectile>();
            proj.color = currentColor;

            proj.parentPlayer = this.gameObject.GetComponent<Player>();

            clone.transform.position = transform.position + 0.5f * transform.right;
            clone.transform.rotation = transform.rotation;
            lastShot = Time.time;

        }
    }

    //When firing, instantiates a new projectile and sets its color to the player's color
    //This can be called by clients
    [Command]
    private void CmdFireProjectile()
    {
        FireProjectile();
    }


    [ClientRpc]
    private void RpcResetCamera()
    {
        if(PlayerCameraObject != null)
        {
            var pos = this.transform.position;
            pos.z = PlayerCameraObject.transform.position.z;

            PlayerCameraObject.transform.position = pos;
        }
    }

    public void ScramblePosition()
    {
        transform.position = NetworkManager.singleton.GetStartPosition().position;
        RpcResetCamera();

    }

    //hit by another player's projectile
    public void TakeHit(Color c, Player attackingPlayer)
    {
        if (isServer)
        {
            if (currentColor != c)
            {
                health -= 1;
                if (health <= 0)
                {
                    //when player is killed
                    currentColor = c;

                    attackingPlayer.GetComponent<Player>().Capture(this);

                    health = 3;
                    maxHealth = 3;
                    transform.localScale = originalScale;
                    currentSize = 1;
                    width = cCollider.bounds.size.x;
                    pWidth = 3;
                    startPWidth = pWidth;

                    ScramblePosition();


                    //if speed powerup is active, end it
                    speedPowerUpActive = false;
                    speed = startSpeed;

                    //if trail powerup is active, end it. trail size reset above
                    trailPowerUpActive = false;
                }
            }
        }
    }

    //start ball off moving
    void MoveBall()
    {
        rbody2d.AddForce(new Vector2(100, -100));
    }

    //Capture another player
    public void Capture(Player capturedPlayer)
    {
        
        if (isServer)
        {
            //increase size by 1/2 of captured player's size, ints are rounded
            int toIncrease = ((capturedPlayer.currentSize + 1) / 2);

            //grow after capturing
            this.gameObject.transform.localScale += ((new Vector3(0.5F, 0.5F, 0)) * toIncrease);

            currentSize += toIncrease;
            numCaptured += 1;
            width = cCollider.bounds.size.x;

            //calculate new health with size scaling
            int newHealth = (2 + currentSize) - (maxHealth - health);
            health = newHealth;
            maxHealth = 2 + currentSize;

            //increase trail width with every 3 captured
            //scaling will likely need to be adjusted later
            pWidth = (3 + Mathf.FloorToInt((currentSize - 1) / 3));
            startPWidth = pWidth;

        }

        if (isLocalPlayer)
        {
            PlayerObjectReferences.singleton.capturedText.text = "Captured: " + numCaptured;
        }
    }

    public void SpeedPowerUp()
    {
        speedPowerUpActive = true;
        speedPowerUpTimeLeft = 10;
    }

    public void TrailPowerUp()
    {
        trailPowerUpActive = true;
        trailPowerUpTimeLeft = 10;
    }

}
