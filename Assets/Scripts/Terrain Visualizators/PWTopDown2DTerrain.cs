using UnityEngine;
using PW;
using PW.Core;

public class PWTopDown2DTerrain : PWTerrainBase {

	static Gradient			rainbow = null;

	static Mesh				topDownTerrainMesh = null;
	static int				topDownTerrainMeshSize = 0;

	void	GenerateTopDownTerrainMesh()
	{
		int					size = chunkSize * chunkSize;
		int					nFaces = (chunkSize - 1) * (chunkSize - 1);
		Vector3[]			vertices = new Vector3[size];
		Vector2[]			uvs = new Vector2[size];
		Vector3[]			normals = new Vector3[size];
		int[]				triangles = new int[nFaces * 6];

		float				terrainWidth = 1;

		topDownTerrainMesh = new Mesh();
		topDownTerrainMesh.Clear();

		for (int x = 0; x < chunkSize; x++)
		{
			float xPos = ((float)x / (chunkSize - 1) - .5f) * terrainWidth;
			for (int z = 0; z < chunkSize; z++)
			{
				float zPos = ((float)z / (chunkSize - 1) - .5f) * terrainWidth;
				vertices[z + x * chunkSize] = new Vector3(xPos, 0, zPos);
				uvs[z + x * chunkSize] = new Vector2((float)x / (chunkSize - 1), (float)z / (chunkSize - 1));
			}
		}

		for (int i = 0; i < chunkSize * chunkSize; i++)
			normals[i] = Vector3.up;

        int t = 0;
        for (int face = 0; face < nFaces; face++)
        {
            int i = face % (chunkSize - 1) + (face / (chunkSize - 1) * chunkSize);

            triangles[t++] = i + 1;
            triangles[t++] = i + chunkSize + 1;
            triangles[t++] = i + chunkSize;

            triangles[t++] = i;
            triangles[t++] = i + 1;
            triangles[t++] = i + chunkSize;
        }

        topDownTerrainMesh.vertices = vertices;
		topDownTerrainMesh.uv = uvs;
		topDownTerrainMesh.normals = normals;
		topDownTerrainMesh.triangles = triangles;
		topDownTerrainMesh.RecalculateBounds();
	}
	
	public override object OnChunkCreate(ChunkData cd, Vector3 pos)
	{
		if (cd == null)
			return null;
		
		if (rainbow == null)
			rainbow = PWUtils.CreateRainbowGradient();
		
		TopDown2DData	chunk = (TopDown2DData)cd;
		
		//create the terrain texture:
		//TODO: bind the blendMap with biome maps to the terrain shader
		//TODO: bind all vertex datas from the mesh

		GameObject g = CreateChunkObject(pos * terrainScale);
		g.transform.rotation = Quaternion.identity;
		Debug.Log("chunkSize: " + chunkSize);
		g.transform.localScale = Vector3.one * chunkSize * terrainScale;
		
		MeshRenderer mr = g.AddComponent< MeshRenderer >();
		MeshFilter mf = g.AddComponent< MeshFilter >();
	
		if (topDownTerrainMesh == null || topDownTerrainMeshSize != chunkSize)
			GenerateTopDownTerrainMesh();

		mf.sharedMesh = topDownTerrainMesh;

		Material mat = new Material(Shader.Find("ProceduralWorlds/Basic terrain"));
		mat.SetTexture("_AlbedoMaps", chunk.albedoMaps);
		if (chunk.blendMaps != null)
		{
			mat.SetTexture("_BlendMaps", chunk.blendMaps);
			mat.SetInt("_BlendMapsCount", chunk.blendMaps.depth);
		}
		mr.sharedMaterial = mat;
		//TODO: vertex painting
		return g;
	}

	public override void OnChunkDestroy(ChunkData terrainData, object userStoredObject, Vector3 pos)
	{
		GameObject g = userStoredObject as GameObject;

		if (g != null)
			DestroyImmediate(g);
	}

	public override void OnChunkRender(ChunkData cd, object chunkGameObject, Vector3 pos)
	{
		if (cd == null)
			return ;
		GameObject		g = chunkGameObject as GameObject;
		TopDown2DData	chunk = (TopDown2DData)cd;

		if (g == null) //if gameobject have been destroyed by user and reference was lost.
		{
			chunkGameObject = RequestCreate(cd, pos);
			g = chunkGameObject as GameObject;
		}
		g.GetComponent< MeshRenderer >().sharedMaterial.SetTexture("_MainTex", chunk.texture);
	}
}
