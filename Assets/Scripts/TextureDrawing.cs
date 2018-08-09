using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
// Goes on Plane object, controls paint trails
public class TextureDrawing : NetworkBehaviour {

    
    public RenderTexture colorTexture;
    private MeshRenderer rend;
    
    public Text colorText;

    public List<Color> allColors;
    public List<string> colorNames;
    protected List<int> ColorPercentages = new List<int>();

    [SyncVar]
    string displayText = "";

    public int leadingColor;

    //edges of plane
    private float planeMinX;
    private float planeMaxX;
    private float planeMinY;
    private float planeMaxY;

    public static TextureDrawing instance;

    protected int scale = 3;

    protected Dictionary<Player, Vector3> previousPositions = new Dictionary<Player, Vector3>();
    protected float threshhold = 0.1f;

    public Color emptyColor = Color.red;

    protected bool isCounting = false;
    private void Awake()
    {
        instance = this;
    }

    //public Camera UpdateCam;

    // Use this for initialization
    void Start () {

       // texture = new Texture2D(192*2, 108*2);
        rend = GetComponent<MeshRenderer>();
        // texture.filterMode = FilterMode.Point;

        //set plane values
        /*
        float planeWidth = texture.width;
        float planeHeight = texture.height;

        planeMinX = 0;
        planeMaxX = planeWidth;
        planeMinY = 0;
        planeMaxY = planeHeight;

        ResetBoard();
        rend.material.mainTexture = texture;
        */

        ResetBoard();
        rend.material.mainTexture = colorTexture;

    }


    // Update is called once per frame
    void Update()
    {

        while (ColorPercentages.Count < allColors.Count)
        {
            ColorPercentages.Add(0);
        }
        

        if (isServer)
        {
            if( !isCounting)
            {
                StartCoroutine("CountColors");

                displayText = "";

                for (int i = 0; i < ColorPercentages.Count; i++)
                {
                    displayText = displayText + colorNames[i] + ": " + ColorPercentages[i] + "%, ";
                }
            }
        }
        UpdateUI();
    }

    // calculate how much of each color is on the canvas
    private IEnumerator CountColors()
    {
        isCounting = true;
        Texture2D texture = new Texture2D(colorTexture.width, colorTexture.height);

        RenderTexture.active = colorTexture;
        texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        texture.Apply();

        Color[] colors = texture.GetPixels();
        
        int[] colorNums = new int[allColors.Count];

        int maxPerFrame = 5000;
        int currentCount = 0;
        //count pixels of each individual color
        for (int i = 0; i < colors.Length; i++)
        {
            currentCount += 1;
            Color colorC = colors[i];


            //rounding due to GetPixels returning inaccurate color values, there may be some colors that are not detected correctly so this may need to be adjusted
            int r = (int)(colorC.r * 100);
            int g = (int)(colorC.g * 100);
            int b = (int)(colorC.b * 100);

            for(int j = 0; j < allColors.Count; j++)
            {
                if ((r == (int)(allColors[j].r * 100)) && (g == (int)(allColors[j].g * 100)) && (b == (int)(allColors[j].b * 100)))
                {
                    colorNums[j] = colorNums[j] + 1;
                }
            }

            if(currentCount >= maxPerFrame)
            {
                currentCount = 0;
                yield return null;
            }
            
        }
        
        // calculate percentages and display them

        double highestPercent = 0; //highest % of color

        //Find leading color and display it
        for (int k = 0; k < allColors.Count; k++)
        {
            
            double percentage = (((double)colorNums[k])/((double)colors.Length));
            ColorPercentages[k] = (int)(percentage * 100);

            if(percentage > highestPercent)
            {
                highestPercent = percentage;
                leadingColor = k;
            }
        }


        isCounting = false;
    }

    public void UpdateUI()
    {
        
        colorText.text = displayText;
    }

    //im the server, tell clients to reset
    public void ResetOnServer()
    {
        if(isServer)
        {
            RpcReset();

        }
    }


    [ClientRpc]
    public void RpcReset()
    {
        Debug.Log("ClientRpc RpcReset Called!");
        ResetBoard();
    }

    public void ResetBoard()
    {
        /*
        Color[] pixels = new Color[texture.width * texture.height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = emptyColor;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        */
        Debug.Log("reset board called");
        RenderTexture rt = UnityEngine.RenderTexture.active;
        UnityEngine.RenderTexture.active = colorTexture;
        GL.Clear(true, true, Color.clear);
       
        UnityEngine.RenderTexture.active = rt;
    }

    //cover area based on given color and position
    [ClientRpc]
    public void RpcPaintExplosion(Color c, Vector3 pos, int explosionDiameter)
    {
        /*
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


            int xPos = (int)(texture.width - (uv.x * texture.width) - (explosionDiameter / 2));
            int yPos = (int)(texture.height - (-uv.y * texture.height) - (explosionDiameter / 2));
            
            //check that it is not out of bounds and relocate accordingly
            if (xPos - (explosionDiameter / 2) - 1 < planeMinX)
            {
                xPos = (int)(planeMinX);
            }
            if (xPos + (explosionDiameter) + 1 > planeMaxX)
            {
                xPos = (int)(planeMaxX - (explosionDiameter));
            }
            if (yPos - (explosionDiameter / 2) - 1 < planeMinY)
            {
                yPos = (int)(planeMinY); 
            }
            if (yPos + (explosionDiameter) + 1 > planeMaxY)
            {
                yPos = (int)(planeMaxY - (explosionDiameter));
            }
            
            //SetPixels, Apply will be called in Update
            texture.SetPixels((int)xPos, (int)yPos, explosionDiameter, explosionDiameter, colors);
            
        }
        */
        
    }


}
