using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum PickupType
{
    PaintBomb,
    Speed,
    Trail,
    Health
}

public class Pickup : NetworkBehaviour
{
    private int explosionDiameter;
    public PickupType type; //type of pickup

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
            if(type == PickupType.PaintBomb)
            {
                Color color = col.GetComponent<Player>().currentColor;
                //trigger explosion on TextureDrawing
                    TextureDrawing.instance.RpcPaintExplosion(color, this.gameObject.transform.position, explosionDiameter);
                    NetworkServer.Destroy(this.gameObject);
                }
                //health refill
                if (type == PickupType.Health)
            {
                col.GetComponent<Player>().health = col.GetComponent<Player>().maxHealth;
                    NetworkServer.Destroy(this.gameObject);
                }
                //speed increase
                if (type == PickupType.Speed)
            {
                col.GetComponent<Player>().SpeedPowerUp();
                    NetworkServer.Destroy(this.gameObject);
                }
                //trail size increase
                if (type == PickupType.Trail)
            {
                col.GetComponent<Player>().TrailPowerUp();
                    NetworkServer.Destroy(this.gameObject);
                }
            }
        
        }
    }
}
