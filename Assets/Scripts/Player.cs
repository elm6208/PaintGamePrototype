using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {

    private Rigidbody2D rbody2d;
    private Vector3 mousePos;

    public float speed = 0.01f;
    public float fireRate;
    private float lastShot = 0f;
    public GameObject projectile;

    public Color currentColor;
    
    public Text capturedText;
    public int numCaptured = 0; //how many other players they've captured

    public int maxHealth = 3;
    public int health = 3;

    public bool isMainPlayer;
    
    private Collider2D cCollider;
    private GameManager gameManager;
    private Vector2 startPosition;
    public string playerName;

    private Vector3 originalScale = Vector3.zero; //original size to revert to when captured
    public int currentSize = 1;
    public float width; //width of player
    public int pWidth; // width of paint trail

    private TextMesh healthText;

    //for speed up powerup
    private bool speedPowerUpActive = false;
    private float speedPowerUpTimeLeft = 10;
    private float startSpeed; // holds original speed to return to when speed powerup ends

    //for trail powerup
    private bool trailPowerUpActive = false;
    private float trailPowerUpTimeLeft = 10;
    private int startPWidth; // holds original trail size to return to when trail powerup ends

    // Use this for initialization
    void Start() {
        rbody2d = GetComponent<Rigidbody2D>();
        currentColor = GetComponent<SpriteRenderer>().color;
        originalScale = transform.lossyScale;
        cCollider = GetComponent<Collider2D>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        startPosition = transform.position;
        pWidth = 3;
        healthText = GetComponentInChildren<TextMesh>();
        healthText.GetComponent<Renderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;
        startSpeed = speed;
        startPWidth = pWidth;
    }

    // Update is called once per frame
    void Update() {
        //if game is not over
        if(gameManager.gameOver == false)
        {
            //only allow controls for the player you're controlling
            if (isMainPlayer)
            {

                // Move character towards mouse if right click is held down
                if (Input.GetMouseButton(1))
                {
                    //get position to set ball to
                    transform.position = Vector3.MoveTowards(transform.position, ScreenToWorld(Input.mousePosition), speed);
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
                if (Input.GetMouseButton(0))
                {

                    if (Time.time > fireRate + lastShot)
                    {
                        FireProjectile();
                        lastShot = Time.time;
                    }

                }

                // if speed power up is active, count down
                if(speedPowerUpActive)
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
            //temp behavior for other players, they just shoot repeatedly
            if (!isMainPlayer)
            {
                if (Time.time > fireRate + lastShot)
                {
                    FireProjectile();
                    lastShot = Time.time;
                }
            }
            
        }
        healthText.text = "" + health;
    }
    

    // Get world position of mouse click
    Vector3 ScreenToWorld(Vector2 screenPos)
    {
        //Create ray
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
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
    private void FireProjectile()
    {
        GameObject clone;
        clone = Instantiate(projectile, transform.position + 0.5f * transform.right, transform.rotation) as GameObject;
        clone.GetComponent<SpriteRenderer>().color = currentColor;
        clone.GetComponent<Projectile>().parentPlayer = this.gameObject.GetComponent<Player>();
        
    }

    //hit by another player's projectile
    public void TakeHit(Color c, Player attackingPlayer)
    {
        if(currentColor != c)
        {
            health -= 1;
            if (health <= 0)
            {
                //when player is killed
                currentColor = c;
                GetComponent<SpriteRenderer>().color = currentColor;
                attackingPlayer.GetComponent<Player>().Capture(this);
                health = 3;
                maxHealth = 3;
                transform.localScale = originalScale;
                currentSize = 1;
                width = cCollider.bounds.size.x;
                pWidth = 3;
                startPWidth = pWidth;
                transform.position = startPosition;

                //if speed powerup is active, end it
                speedPowerUpActive = false;
                speed = startSpeed;

                //if trail powerup is active, end it. trail size reset above
                trailPowerUpActive = false;

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

        if(isMainPlayer)
        {
            capturedText.text = "Captured: " + numCaptured;
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
