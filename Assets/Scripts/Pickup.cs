using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {

    private TextureDrawing textureDrawing;
    private int explosionDiameter;

	// Use this for initialization
	void Start () {

        textureDrawing = GameObject.FindGameObjectWithTag("TextureDrawing").GetComponent<TextureDrawing>();

        explosionDiameter = 20;

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter2D(Collider2D col)
    {
        //picked up by a player
        if (col.tag == "Player" || col.tag == "NonPlayer")
        {
            Color color = col.GetComponent<Player>().currentColor;
            //trigger explosion on TextureDrawing
            textureDrawing.PaintExplosion(color, this.gameObject.transform.position, explosionDiameter);
            Destroy(this.gameObject);
        }
        

    }
}
