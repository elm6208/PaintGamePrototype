using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controls health bar in corner of screen that displays player's health
public class HealthBar : MonoBehaviour {

    private float barDisplay;
    RectTransform rectT;
    private float width;
    private float height;
    private float xPos;
    private float yPos;

	// Use this for initialization
	void Start () {

        //get RectTransform and starting position offset
        rectT = GetComponent<RectTransform>();
        xPos = rectT.offsetMin.x;
        yPos = rectT.offsetMin.y;
        width = rectT.rect.width;
        height = rectT.rect.height;
	}
	
	// Update is called once per frame
	void Update () {
        var p = Player.localPlayer;
        //currently updates every frame, could be changed to only update when taking hits/regenerating health
        if(rectT != null && p != null)
        {
            //update health bar width and position
            barDisplay = (float)p.health / (float)p.maxHealth;
            rectT.sizeDelta = new Vector2(width * barDisplay, height);
            rectT.offsetMin = new Vector2(xPos, yPos);
        }
        
	}
}
