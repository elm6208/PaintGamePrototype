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

        if (GridObject != null)
        {
            int xSize = GridObject.xSize;
            int ySize = GridObject.ySize;

            GenerateWithSize(xSize, ySize, GridObject.GetComponent<MeshRenderer>());

        } else
        {
            GenerateWithSize(50, 20, this.GetComponent<MeshRenderer>());
        }

    }

    void GenerateWithSize(int xSize, int ySize, Renderer r)
    {
        GeneratedTexture = new Texture2D(xSize, ySize);
        GeneratedTexture.filterMode = FilterMode.Point;
        GeneratedTexture.wrapMode = TextureWrapMode.Clamp;

        var randomColors = new Color[xSize * ySize];
        for (int i = 0; i < xSize; i++)
        {
            for (int j = 0; j < ySize; j++)
            {
                    int index = i * (ySize) + j;
                    if(index >= randomColors.Length)
                    {
                        Debug.Log("xSize:"+xSize+" ySize:"+ySize+" total:"+xSize*ySize+" index:"+index+" i:"+i+" j:"+j);
                    }
                    randomColors[index] = randomColor();
            }
        }
        GeneratedTexture.SetPixels(randomColors);
        GeneratedTexture.Apply();

        r.material.mainTexture = GeneratedTexture;
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