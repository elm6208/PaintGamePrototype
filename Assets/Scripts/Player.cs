using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using Smooth;

[RequireComponent(typeof(NetworkIdentity))]

public class Player : NetworkBehaviour
{

    public SpriteRenderer PaintRenderer;

    private Rigidbody2D rbody2d;
    private Vector3 mousePos;

    public float speed = 0.01f;
    public float fireRate;

    [SyncVar]
    public bool isMoving = false;

    [SyncVar]
    public float shotTimeLeft = 0.0f;

    [SyncVar]
    private float lastShot = 0f;
    public GameObject projectile;

    [SyncVar(hook ="SetColor")]
    public Color currentColor;

    [SyncVar]
    public Color originalColor;

    [SyncVar(hook = "SetNumCaptured")]
    public int numCaptured = 0; //how many other players they've captured

    [SyncVar]
    public int maxHealth = 3;

    [SyncVar]
    public int health = 3;
    
 
    private Collider2D cCollider;
    private GameManager gameManager;
    
    [SyncVar]
    public string playerName;

    public static Player localPlayer;

    [SyncVar]
    private Vector3 originalScale = Vector3.zero; //original size to revert to when captured

    [SyncVar(hook = "SetCurrentSize")]
    public int currentSize = 0;

    [SyncVar]
    public float width; //width of player
 

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
    public int buttonFingerID = -1;

    public GameObject forwardIndicator;

    [SyncVar(hook = "SetTeamColor")]
    private Color teamColor;

    static int which = 0;

    // Use this for initialization
    void Start() {
        rbody2d = GetComponent<Rigidbody2D>();
        SetColor(originalColor);

        originalScale = transform.lossyScale;
        cCollider = GetComponent<Collider2D>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        forwardIndicator = transform.GetChild(0).gameObject;

        SetTeamColor(originalColor);
    }
    
    // add to player list on enable
    public void OnEnable()
    {
        GameManager.instance.allPlayers.Add(this.gameObject);
    }

    //remove from player list on disable
    public void OnDisable()
    {
        GameManager.instance.allPlayers.Remove(this.gameObject);
    }

    override public void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        localPlayer = this;  
        Debug.Log("IS LOCAL PLAYER");
        PlayerCameraObject = Instantiate(PlayerCameraPrefab);
        var campos = this.transform.position;
        campos.z = PlayerCameraObject.transform.position.z;
        PlayerCameraObject.transform.position = campos;
    }

    override public void OnStartServer()
    {
        base.OnStartServer();
        var colors = TextureDrawing.instance.allColors;

        originalColor = colors[which];
        SetColor(originalColor);
        which = (which + 1) % colors.Count;
        healthText.GetComponent<Renderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;
        startSpeed = speed;
        SetTeamColor(originalColor);
    }


    NetworkIdentity _identity;
    NetworkIdentity identity
    {
        get
        {
            if(_identity != null)
            {
                return _identity;
            }
            _identity = GetComponent<NetworkIdentity>();
            return _identity;
        }
    }

    // Set current color and paint trail color
    public void SetColor(Color color)
    {
        currentColor = color;
        GetComponent<SpriteRenderer>().color = color;
        PaintRenderer.color = color;
    }

    // set team color (forward indicator / "nose")
    public void SetTeamColor(Color color)
    {
        teamColor = color;
        forwardIndicator.GetComponent<SpriteRenderer>().color = color;
    }

    // set number of times the player has captured another player
    public void SetNumCaptured(int num)
    {
        numCaptured = num;
        // update captured text
        if (identity.isLocalPlayer)
        {
            PlayerObjectReferences.singleton.capturedText.text = "Captured: " + numCaptured;
        }
    }

    // scale player according to given size
    public void SetCurrentSize(int num)
    {
        currentSize = num;
        this.gameObject.transform.localScale = (originalScale + (new Vector3(0.5F, 0.5F, 0)) * currentSize);
    }

    // Update is called once per frame
    void Update() {
        bool shouldMove = false;
        
        // set player name text
        if (identity.isLocalPlayer)
        {
            if (playerName == null)
            {
                Debug.Log("SORRY NO NAME");
            }
            else
            {
                PlayerObjectReferences.singleton.playerNameText.text = playerName;
            }
            
        }

        if (identity.isLocalPlayer && PlayerCameraObject != null)
        {
            var campos = Vector3.Lerp(PlayerCameraObject.transform.position, this.transform.position,1);
            campos.z = PlayerCameraObject.transform.position.z;
            PlayerCameraObject.transform.position = campos;
        }

        //if game is not over
        if(gameManager.gameOver == false)
        {
            healthText.text = "" + health;
            
            if (identity.isServer)
            {
                shotTimeLeft = Time.time - (fireRate + lastShot);

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
                    }
                    if (trailPowerUpTimeLeft <= 0)
                    {
                        trailPowerUpActive = false;
                    }
                }
            }

            if (identity.isLocalPlayer )
            {
                //controls for windows
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                // Move character towards mouse if right click is held down
                if (Input.GetMouseButton(0))
                {
                    // only move if not clicking over the fire button
                    if((EventSystem.current.currentSelectedGameObject == null) || (EventSystem.current.currentSelectedGameObject.tag != "Button"))
                    {
                        
                            bool dontSkip = true;
                            //get position to set ball to
                            var target = ScreenToWorld(Input.mousePosition);

                            //if target is equal to the current position, skip the below portion
                            if (target == transform.position)
                            {
                                dontSkip = false;
                            }
                            if (dontSkip)
                            {

                            Vector3 difference = ScreenToWorld(Input.mousePosition) - transform.position;
                            difference.Normalize();
                            float z_rotation = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
                            Quaternion quat = Quaternion.Euler(0f, 0f, z_rotation);
                            CmdMoveToward(quat);
                            shouldMove = true;

                        }
                        
                    }
                    
                }
                //on left click, fire a projectile
                if (Input.GetMouseButton(1) || Input.GetKeyUp(KeyCode.Space))
                {
                    TryToShoot();
                }
#endif
                // android controls
#if UNITY_ANDROID
                if(Input.touchCount > 0)
                {
                    // keep track of touches & fingerId
                    foreach(Touch touch in Input.touches)
                    {
                        if(buttonFingerID == -1 && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                        {
                            buttonFingerID = touch.fingerId;
                            
                        }
                        
                            if((touch.fingerId != buttonFingerID) && (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled))
                            {
                            
                                //get position to set ball to
                                var target = ScreenToWorld(touch.position);
                            Vector3 pos = Vector3.MoveTowards(transform.position, target, speed);
                            pos.z = 0;

                            //get rotation to set ball to
                            Vector3 difference = ScreenToWorld(touch.position) - transform.position;
                            difference.Normalize();
                            float z_rotation = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
                            Quaternion quat = Quaternion.Euler(0f, 0f, z_rotation);
                            CmdMoveToward(quat);
                            shouldMove = true;
                                
                        }
                            
                        
                    }
                }
                else
                {
                    buttonFingerID = -1;
                }
                
#endif
                
            }
            else

            //temp behavior for non-human players, they just shoot repeatedly
            if (identity.playerControllerId == -1)
            {
                if (Time.time > fireRate + lastShot)
                {
                    FireProjectile();
                    lastShot = Time.time;
                }
            }
            
        }

        if (shouldMove != isMoving && isLocalPlayer)
        {
            if (isServer)
            {
                isMoving = shouldMove;
            }
            else
            {
                CmdShouldMove(shouldMove);
            }
        }

        if (isMoving && isServer)
        {
            this.transform.position = this.transform.position + this.transform.right * this.speed;

        }

    }

    [Command]
    public void CmdShouldMove(bool val)
    {
        isMoving = val;
    }

    [Command]
    public void CmdMoveToward(Quaternion rotation)
    {
        this.transform.rotation = rotation;

    }

    //tries to fire a projectile
    public void TryToShoot()
    {
        if (identity.isLocalPlayer && gameManager.gameOver == false)
        {
            if (Time.time > fireRate + lastShot)
            {
                if (identity.isServer)
                {
                    FireProjectile();
                }
                else
                {
                    CmdFireProjectile();
                }
            }
        }
            
    }

    // Get world position of mouse click, only pass in clicks/touches that are not over UI
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
        if (identity.isServer)
        {
            GameObject clone = Instantiate(projectile) as GameObject;
            
            var proj = clone.GetComponent<Projectile>();
            proj.color = teamColor;

            proj.parentPlayer = this.gameObject.GetComponent<Player>();
            clone.transform.position = transform.position + transform.right * 0.2f;
            clone.transform.rotation = transform.rotation;
            lastShot = Time.time;
            NetworkServer.Spawn(clone);

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

    // scramble start position and teleport player to it
    public void ScramblePosition()
    {
        transform.position = NetworkManager.singleton.GetStartPosition().position;

        var ns = GetComponent<SmoothSync>();
        ns.teleport();

        RpcResetCamera();

    }

    //hit by another player's projectile
    public void TakeHit(Color c, Player attackingPlayer)
    {
        if (identity.isServer)
        {
            if (teamColor != c)
            {
                health -= 1;
                if (health <= 0)
                {
                    //when player is killed
                    SetColor(c);

                    attackingPlayer.GetComponent<Player>().Capture(this);

                    health = 3;
                    maxHealth = 3;

                    SetCurrentSize(0);
                    width = cCollider.bounds.size.x;

                    ScramblePosition();

                    //if speed powerup is active, end it
                    speedPowerUpActive = false;
                    speed = startSpeed;

                    //if trail powerup is active, end it
                    trailPowerUpActive = false;

                    gameManager.CheckIfAllOneColor();
                    CheckIfTeamConverted(teamColor);
                    
                }
            }
        }
    }
    
    //check if the entire team has had their color converted
    private void CheckIfTeamConverted(Color playerTeamColor)
    {
        bool isTeamConverted = true;
        Debug.Log("ALL PLAYERS COUNT:"+GameManager.instance.allPlayers.Count);
        foreach(GameObject p in GameManager.instance.allPlayers)
        {
            Player play = p.GetComponent<Player>();

            //if player is on their team, check if they are converted
            if(play.teamColor == playerTeamColor)
            {
                if (play.currentColor == play.teamColor)
                {
                    isTeamConverted = false;
                }
            }
            
        }

        //if the team has been converted, change all of their team colors to their current color
        if(isTeamConverted)
        {
            foreach (GameObject p in GameManager.instance.allPlayers)
            {
                Player play = p.GetComponent<Player>();
                if(play.currentColor != play.teamColor)
                {
                    if (play.currentColor == playerTeamColor)
                    {
                        play.SetColor(play.teamColor);
                    }
                    if (play.teamColor == playerTeamColor)
                    {
                        play.SetTeamColor(play.currentColor);
                    }
                }
                
            }

            // find which color team was eliminated and send the color name
            string eliminatedTeam = "x";
            for(int i = 0; i < TextureDrawing.instance.allColors.Count; i++)
            {
                if (TextureDrawing.instance.allColors[i] == playerTeamColor)
                {
                    eliminatedTeam = TextureDrawing.instance.colorNames[i];
                }
            }
            gameManager.RpcDisplayEliminatedText(eliminatedTeam);
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

        if (identity.isServer)
        {
            //increase size by 1/2 of captured player's size, ints are rounded
            int toIncrease = ((capturedPlayer.currentSize + 4) / 2);

            SetCurrentSize(currentSize + toIncrease);
            SetNumCaptured(numCaptured + 1);
            
            width = cCollider.bounds.size.x;

            //calculate new health with size scaling
            int newHealth = (2 + currentSize) - (maxHealth - health);
            health = newHealth;
            maxHealth = 2 + currentSize;
            
        }
        
    }
    
    // set player scale on client
    [ClientRpc]
    private void RpcSetScale()
    {
        this.gameObject.transform.localScale = (originalScale + (new Vector3(0.5F, 0.5F, 0)) * currentSize);
    }

    // start speed power up
    public void SpeedPowerUp()
    {
        speedPowerUpActive = true;
        speedPowerUpTimeLeft = 10;
    }

    //start trail power up
    public void TrailPowerUp()
    {
        trailPowerUpActive = true;
        trailPowerUpTimeLeft = 10;
    }

}
