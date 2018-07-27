using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Pickup : NetworkBehaviour
{
    private int explosionDiameter;
    public int type; //type of pickup

	// Use this for initialization
	void Start () {
        explosionDiameter = 20;

        //set z to zero to make raycast accurate
        transform.position = new Vector3(transform.position.x, transform.position.y, 0); 
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter2D(Collider2D col)
    {
        if (isServer)
        {
            //picked up by a player
            if (col.tag == "Player" || col.tag == "NonPlayer")
            {
  
            //paint bomb
            if(type == 0)
            {
                Color color = col.GetComponent<Player>().currentColor;
                //trigger explosion on TextureDrawing
                    TextureDrawing.instance.RpcPaintExplosion(color, this.gameObject.transform.position, explosionDiameter);
                    NetworkServer.Destroy(this.gameObject);
                }
                //health refill
                if (type == 1)
            {
                col.GetComponent<Player>().health = col.GetComponent<Player>().maxHealth;
                    NetworkServer.Destroy(this.gameObject);
                }
                //speed increase
                if (type == 2)
            {
                col.GetComponent<Player>().SpeedPowerUp();
                    NetworkServer.Destroy(this.gameObject);
                }
                //trail size increase
                if (type == 3)
            {
                col.GetComponent<Player>().TrailPowerUp();
                    NetworkServer.Destroy(this.gameObject);
                }
            }
        
        }
    }
}
