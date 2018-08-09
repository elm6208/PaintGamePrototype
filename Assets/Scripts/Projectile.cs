using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//controls bullet objects being fired by players
public class Projectile : NetworkBehaviour {

    [SyncVar(hook = "SetColor")]
    public Color color;

    private Rigidbody2D rbody2d;
    public Player parentPlayer;

    // Use this for initialization
    void Start () {
        rbody2d = GetComponent<Rigidbody2D>();
        MoveBall();
    }

    void SetColor(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }

    //move the ball forward in the direction it is facing
    private void MoveBall()
    {
        if (isServer)
        {
            rbody2d.AddForce(transform.right * 500);
        }
    }



    void OnTriggerEnter2D(Collider2D col)
    {
        if (isServer)
        {
            //destroy if it hits a wall
            if (col.tag == "Wall")
            {
                NetworkServer.Destroy(this.gameObject);
            }

            //hit an "enemy", or another player hits you
            if ((col.tag == "NonPlayer" || col.tag == "Player") && col.gameObject != parentPlayer.gameObject)
            {

                Color c = GetComponent<SpriteRenderer>().color;
                col.GetComponent<Player>().TakeHit(c, parentPlayer);
                NetworkServer.Destroy(this.gameObject);
            }
        }
    }
}
