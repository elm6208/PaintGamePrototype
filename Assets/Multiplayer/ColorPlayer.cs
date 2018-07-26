using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ColorPlayer : NetworkBehaviour {
    public Renderer Renderer;
    public Camera cam;
    public LayerMask RaycastLayer;

    [SyncVar(hook = "SetColor")]
    public Color playerColor = Color.black;

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (isServer)
        {
            InitializeWithRandomColor();
        }
    }

    void InitializeWithRandomColor()
    {
        var color = Random.ColorHSV();
        playerColor = color;
    }

    void SetColor(Color c)
    {
       // Debug.Log("Setcolor called: c:"+c);
        Renderer.material.color = c;
    }

    // Update is called once per frame
    void Update () {
        var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, cam.nearClipPlane));
        RaycastHit info;

        if(Physics.Raycast(ray, out info, float.MaxValue, RaycastLayer))
        {
            /*
            var Updater = NetworkTextureUpdater.singleton;
          //  Debug.Log("Got a Hit : "+ info.textureCoord + Updater.Texture);
            var coord = info.textureCoord;

            int xPixel = Mathf.RoundToInt(coord.x * Updater.Texture.width);
            int yPixel = Mathf.RoundToInt(coord.y * Updater.Texture.height);

            Updater.UpdatePixel(xPixel, yPixel, playerColor);
            */
        } 
}
}
