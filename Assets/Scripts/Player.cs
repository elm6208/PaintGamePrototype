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
    private Color currentColor;
    public GameObject projectile;
    public Text capturedText;
    private int numCaptured = 0;
    private int health = 3;
    public bool isMainPlayer;
    private Vector3 originalScale = Vector3.zero;
    public int currentSize = 1;
    
	// Use this for initialization
	void Start () {
        rbody2d = GetComponent<Rigidbody2D>();
        currentColor = GetComponent<SpriteRenderer>().color;
        originalScale = transform.lossyScale;
    }
	
	// Update is called once per frame
	void Update () {

        //only allow controls for the player you're controlling
        if(isMainPlayer)
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
        if(!isMainPlayer)
        {
            if (Time.time > fireRate + lastShot)
            {
                FireProjectile();
                lastShot = Time.time;
            }
        }

        
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
                currentColor = c;
                GetComponent<SpriteRenderer>().color = currentColor;
                attackingPlayer.GetComponent<Player>().Capture(this);
                health = 3;
                transform.localScale = originalScale;
                currentSize = 1;
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

        if(isMainPlayer)
        {
            capturedText.text = "Captured: " + numCaptured;
        }
        
    }
}
