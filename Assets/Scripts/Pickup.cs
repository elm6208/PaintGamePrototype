using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Pickup : NetworkBehaviour
{
    private int explosionDiameter;

	// Use this for initialization
	void Start () {
        explosionDiameter = 20;

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
                Color color = col.GetComponent<Player>().currentColor;


                Debug.Log("player:" + col.GetComponent<Player>()+" color:"+color+" diameter:"+explosionDiameter);
                //trigger explosion on TextureDrawing

                TextureDrawing.instance.RpcPaintExplosion(color, this.gameObject.transform.position, explosionDiameter);
                NetworkServer.Destroy(this.gameObject);
            }


        }
    }
}
