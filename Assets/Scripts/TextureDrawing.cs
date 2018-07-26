﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Goes on Plane object, controls paint trails
public class TextureDrawing : MonoBehaviour {

    private Texture2D texture;
    private Renderer rend;
    public List<Player> players;
    public Text colorText;

    public Color[] allColors;
    public int leadingColor;

    //edges of plane
    private float planeMinX;
    private float planeMaxX;
    private float planeMinY;
    private float planeMaxY;

    public static TextureDrawing instance;

    private void Awake()
    {
        instance = this;
    }
    // Use this for initialization
    void Start () {
        //set up texture
        texture = new Texture2D(100, 50);
        rend = GetComponent<Renderer>();
        rend.material.mainTexture = texture;
        texture.filterMode = FilterMode.Point;
        
        //set plane values
        float planeWidth = texture.width;
        float planeHeight = texture.height;

        planeMinX = 0;
        planeMaxX = planeWidth;
        planeMinY = 0;
        planeMaxY = planeHeight;
        
        
    }
	
	// Update is called once per frame
	void Update () {
        
        if (texture != null)
        {
            
            // leave a paint trail behind each player
            foreach(Player p in players)
            {
                if(p == null)
                {
                    continue;
                }
                    //raycast down to find the spot below the player
                    Vector3 direction = new Vector3(0f, 0f, 1f);
                    Ray ray = new Ray(new Vector3(p.transform.position.x, p.transform.position.y, p.transform.position.z), direction);
                    RaycastHit hit;

                    //if there is a hit, draw player's paint trail
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        
                        Vector2 uv;
                        uv.x = (hit.point.x - hit.collider.bounds.min.x) / hit.collider.bounds.size.x;
                        uv.y = (hit.collider.bounds.min.y - hit.point.y) / hit.collider.bounds.size.y;

                        Color pColor = p.currentColor;
                        
                        /*
                        // Other method to paint it with the player's color, SetPixel is slower than SetPixels, this also doesn't adjust for scaling
                        // However the other approach may be a performance issue as well, still calling SetPixels multiple times
                        texture.SetPixel((int)(-uv.x * texture.width), (int)(uv.y * texture.height), pColor);
                        texture.SetPixel((int)(-uv.x * texture.width), (int)(uv.y * texture.height) + 1, pColor);
                        texture.SetPixel((int)(-uv.x * texture.width) + 1, (int)(uv.y * texture.height), pColor);
                        texture.SetPixel((int)(-uv.x * texture.width), (int)(uv.y * texture.height) - 1, pColor);
                        texture.SetPixel((int)(-uv.x * texture.width) - 1, (int)(uv.y * texture.height), pColor);
                        texture.SetPixel((int)(-uv.x * texture.width) + 1, (int)(uv.y * texture.height) + 1, pColor);
                        texture.SetPixel((int)(-uv.x * texture.width) - 1, (int)(uv.y * texture.height) - 1, pColor);
                        texture.SetPixel((int)(-uv.x * texture.width) - 1, (int)(uv.y * texture.height) + 1, pColor);
                        texture.SetPixel((int)(-uv.x * texture.width) + 1, (int)(uv.y * texture.height) - 1, pColor);
                        */
                        
                        //position to draw set of pixels from
                        float xPos = texture.width - (uv.x * texture.width);
                        float yPos = texture.height - (-uv.y * texture.height);
                        

                        //corners of square being drawn
                        float minX = xPos - p.pWidth;
                        float maxX = xPos;
                        float minY = yPos;
                        float maxY = yPos + p.pWidth;

                        //find difference between corner and plane edge, move square corner accordingly to avoid going off the edge
                        //this currently causes a snapping effect with the trail when approaching some of the walls
                        if(minX < planeMinX)
                        {
                            xPos = planeMinX;
                        }
                        if ((maxX + p.pWidth) > planeMaxX)
                        {
                            xPos = planeMaxX - p.pWidth;
                        }
                        if (minY - p.pWidth < planeMinY)
                        {
                            yPos = planeMinY;
                        }
                        if (maxY > planeMaxY)
                        {
                            yPos = planeMaxY - p.pWidth;
                        }

                        //make color array to draw
                        Color[] colors = new Color[p.pWidth * p.pWidth];

                        for (int i = 0; i < colors.Length; i++)
                        {
                            colors[i] = pColor;
                        }

                        texture.SetPixels((int)xPos, (int)yPos, p.pWidth, p.pWidth, colors);



                    }
                
                

                    
            }
            //applies SetPixels
            texture.Apply();


        }

        CountColors();
	}

    // calculate how much of each color is on the canvas
    private void CountColors()
    {
        Color[] colors = texture.GetPixels();
        
        int[] colorNums = new int[allColors.Length];

        //count pixels of each individual color
        for (int i = 0; i < colors.Length; i++)
        {
            Color colorC = colors[i];


            //rounding due to GetPixels returning inaccurate color values, there may be some colors that are not detected correctly so this may need to be adjusted
            int r = (int)(colorC.r * 100);
            int g = (int)(colorC.g * 100);
            int b = (int)(colorC.b * 100);

            for(int j = 0; j < allColors.Length; j++)
            {
                if ((r == (int)(allColors[j].r * 100)) && (g == (int)(allColors[j].g * 100)) && (b == (int)(allColors[j].b * 100)))
                {
                    colorNums[j] = colorNums[j] + 1;
                }
            }
            
        }
        
        // calculate percentages and display them
        string displayText = "";

        double highestPercent = 0; //highest % of color

        //Find leading color and display it
        for (int k = 0; k < allColors.Length; k++)
        {
            
            double percentage = (((double)colorNums[k])/((double)colors.Length));
            displayText = displayText + "Color " + (k + 1) + ": " + (int)(percentage * 100) + "%, ";
            if(percentage > highestPercent)
            {
                highestPercent = percentage;
                leadingColor = k + 1;
            }
        }

        colorText.text = displayText;

    
    }

    //cover area based on given color and position
    public void PaintExplosion(Color c, Vector3 pos, int explosionDiameter)
    {
        //array of color to fill
        Color[] colors = new Color[explosionDiameter * explosionDiameter];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = c;
        }

        //raycast down to find the spot below the pickup
        Vector3 direction = new Vector3(0f, 0f, 1f);
        Ray ray = new Ray(new Vector3(pos.x, pos.y, pos.z), direction);
        RaycastHit hit;

        //if there is a hit, draw paint area
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {

            Vector2 uv;
            uv.x = (hit.point.x - hit.collider.bounds.min.x) / hit.collider.bounds.size.x;
            uv.y = (hit.collider.bounds.min.y - hit.point.y) / hit.collider.bounds.size.y;


            //explosion location centered around pickup
            float xPos = texture.width - (uv.x * texture.width) - (explosionDiameter / 2);
            float yPos = texture.height - (-  uv.y * texture.height) - (explosionDiameter / 2);

            Debug.Log("xPos: " + xPos + ", yPos: " + yPos);

            //check that it is not out of bounds and relocate accordingly
            if (xPos - (explosionDiameter / 2) < planeMinX)
            {
                xPos = planeMinX + (explosionDiameter / 2);
            }
            if (xPos + (explosionDiameter / 2) > planeMaxX)
            {
                xPos = planeMaxX - explosionDiameter;
            }
            if (yPos - (explosionDiameter / 2) < planeMinY)
            {
                yPos = planeMinY + (explosionDiameter / 2);
            }
            if (yPos + (explosionDiameter / 2) > planeMaxY)
            {
                yPos = planeMaxY - explosionDiameter;
            }

            //SetPixels, Apply will be called in Update
            texture.SetPixels((int)xPos, (int)yPos, explosionDiameter, explosionDiameter, colors);
            
        }

        
    }


}
