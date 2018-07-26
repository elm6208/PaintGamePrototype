using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Grid : MonoBehaviour {

	public int xSize, ySize;

	private Mesh mesh;
	private Vector3[] vertices;
    //  private Color[] colors;

	private void Awake () {
		Generate();
	}

	private void Generate () {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Grid";

        int Size = (xSize + 1) * (ySize + 1);

        vertices = new Vector3[Size];
        //  colors = new Color[Size];


       // Vector2[] uv2 = new Vector2[vertices.Length];
		Vector2[] uv = new Vector2[vertices.Length];
		Vector4[] tangents = new Vector4[vertices.Length];
		Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
		for (int i = 0, y = 0; y <= ySize; y++) {
			for (int x = 0; x <= xSize; x++, i++) {
				vertices[i] = new Vector3(x, y);
              //  colors[i] = Random.ColorHSV();
				uv[i] = new Vector2((float)x / xSize, (float)y / ySize);

            //    uv2[i] = new Vector2((x%2) , (y%2));

				tangents[i] = tangent;
			}
		}
        
		mesh.vertices = vertices;
		mesh.uv = uv;
      //  mesh.uv2 = uv2;
		mesh.tangents = tangents;
     //   mesh.colors = colors;

		int[] triangles = new int[xSize * ySize * 6];
		for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++) {
			for (int x = 0; x < xSize; x++, ti += 6, vi++) {
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
				triangles[ti + 5] = vi + xSize + 2;
			}
		}
		mesh.triangles = triangles;
		mesh.RecalculateNormals();

        var collider = GetComponent<MeshCollider>();
        if(collider != null)
        {
            collider.sharedMesh = mesh;
        }
	}
}