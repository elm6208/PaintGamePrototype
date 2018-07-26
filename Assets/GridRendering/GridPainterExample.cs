using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridPainterExample : MonoBehaviour {
    public Grid GridObject;
    public Texture2D GeneratedTexture;
    
    public List<Color> colors;

    public Vector2Int DebugTextureIndex;
    public Color DebugColor;

    // Use this for initialization
    void Start() {

        int xSize = GridObject.xSize ;
        int ySize = GridObject.ySize ;

        GeneratedTexture = new Texture2D(xSize, ySize);
        GeneratedTexture.filterMode = FilterMode.Point;
        GeneratedTexture.wrapMode = TextureWrapMode.Clamp;

        var randomColors = new Color[GeneratedTexture.width * GeneratedTexture.height];
        for (int i = 0; i < xSize; i++)
        {
            for(int j = 0; j < ySize; j++)
            {

             //   if (i % 2 == 0 && j % 2 == 0)
                {
                    randomColors[i * (xSize) + j] = randomColor();
                }
            }
        }
        GeneratedTexture.SetPixels(randomColors);
        GeneratedTexture.Apply();

        GridObject.GetComponent<MeshRenderer>().material.mainTexture = GeneratedTexture;

	}

    Color randomColor()
    {
        return colors[Random.Range(0, colors.Count )];
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(GridPainterExample))]
public class GridPainterExampleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GridPainterExample painter = (GridPainterExample)(target);
        if(GUILayout.Button("Apply New Color"))
        {
            painter.GeneratedTexture.SetPixel(painter.DebugTextureIndex.x, painter.DebugTextureIndex.y, painter.DebugColor);
            painter.GeneratedTexture.Apply();
        }
       
    }
}
#endif