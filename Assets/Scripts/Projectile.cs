using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//controls bullet objects being fired by players
public class Projectile : MonoBehaviour {

    private Rigidbody2D rbody2d;
    public Player parentPlayer;

    // Use this for initialization
    void Start () {
        rbody2d = GetComponent<Rigidbody2D>();
        MoveBall();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    //move the ball forward in the direction it is facing
    private void MoveBall()
    {
        rbody2d.AddForce(transform.right * 500);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        //destroy if it hits a wall
        if(col.tag == "Wall")
        {
            Destroy(this.gameObject);
        }
        //hit an "enemy"
        if(col.tag == "NonPlayer")
        {
            
            Color c = GetComponent<SpriteRenderer>().color;
            col.GetComponent<Player>().TakeHit(c, parentPlayer);
        }
        //another player hits you
        if(col.tag == "Player")
        {
            Color c = GetComponent<SpriteRenderer>().color;
            col.GetComponent<Player>().TakeHit(c, parentPlayer);
        }
    }
}
