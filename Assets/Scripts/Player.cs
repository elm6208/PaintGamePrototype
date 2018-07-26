﻿using System.Collections;
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
    private float lastShot = 0f;
    public GameObject projectile;

    [SyncVar(hook ="SetColor")]
    public Color currentColor;
    
    public Text capturedText;
    public int numCaptured = 0; //how many other players they've captured

    [SyncVar]
    public int maxHealth = 3;

    [SyncVar]
    public int health = 3;
    
    private Collider2D cCollider;
    private GameManager gameManager;
    private Vector2 startPosition;
    public string playerName;

    private Vector3 originalScale = Vector3.zero; //original size to revert to when captured

    [SyncVar]
    public int currentSize = 1;
    [SyncVar]
    public float width; //width of player
    [SyncVar]
    public int pWidth; // width of paint trail

    private TextMesh healthText;
    

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
        //  healthText.GetComponent<Renderer>().sortingOrder = GetComponent<SpriteRenderer>().sortingOrder + 1;
        TextureDrawing.instance.players.Add(this);

    }

    //I am the server running this code. Only the server can change things for everyone
    public override void OnStartServer()
    {
        base.OnStartServer();

        if (isServer)
        {
            currentColor = Random.ColorHSV();
        }
    }

    void SetColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
        currentColor = color;
    }

    // Update is called once per frame
    void Update() {
        //if game is not over
        if(gameManager.gameOver == false)
        {
            //only allow controls for the player you're controlling
            if (isLocalPlayer)
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

            }
            //temp behavior for other players, they just shoot repeatedly
            if (isServer)
            {
                if (Time.time > fireRate + lastShot)
                {
                    FireProjectile();
                    lastShot = Time.time;
                }
            }
            
        }
        //healthText.text = "" + health;
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
        
        //if ray hits nothing, return intersection between ray and Y=0 plane
        float t = -ray.origin.y / ray.direction.y;
        return ray.GetPoint(t);
        
    }

    //When firing, instantiates a new projectile and sets its color to the player's color
    //This can only be run by the server!
    private void FireProjectile()
    {
        GameObject clone = Instantiate(projectile) as GameObject;
        NetworkServer.Spawn(clone);

        clone.GetComponent<SpriteRenderer>().color = currentColor;
        clone.GetComponent<Projectile>().parentPlayer = this.gameObject.GetComponent<Player>();

        clone.transform.position = transform.position + 0.5f * transform.right;
        clone.transform.rotation = transform.rotation;   
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
                    GetComponent<SpriteRenderer>().color = currentColor;
                    attackingPlayer.GetComponent<Player>().Capture(this);
                    health = 3;
                    maxHealth = 3;
                    transform.localScale = originalScale;
                    currentSize = 1;
                    width = cCollider.bounds.size.x;
                    pWidth = 3;
                    transform.position = startPosition;
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

        if(isLocalPlayer)
        {
            capturedText.text = "Captured: " + numCaptured;
        }
        
    }



}
