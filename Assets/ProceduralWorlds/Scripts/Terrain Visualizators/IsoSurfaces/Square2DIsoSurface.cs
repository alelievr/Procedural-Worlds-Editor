using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.IsoSurfaces
{
    public class Square2DIsoSurface : IsoSurface
    {
		Sampler2D	heightDisplacementMap = null;
		float		heightScale;

		Vector2[]	uvs;
		Vector3[]	vertices;
		Vector3[]	normals;
		int[]		triangles;

		[System.NonSerialized]
		bool		initialized = false;
		[System.NonSerialized]
		int			oldChunkSize;

		//TODO: implement shared vertices

        public override Mesh Generate(int chunkSize)
        {
			int		size = (chunkSize) * chunkSize;
			int		nFaces = (chunkSize - 1) * (chunkSize - 1);
			Mesh	mesh = new Mesh();

			//we're not going to need dynamic number of vertices here so allocate them once.
			if (!initialized || chunkSize != oldChunkSize)
			{
				vertices = new Vector3[size];
				normals = new Vector3[size];
				uvs = new Vector2[size];
				triangles = new int[nFaces * 6];

				initialized = true;
				oldChunkSize = chunkSize;
			}
	
			for (int x = 0; x < chunkSize; x++)
			{
				float xPos = ((float)x / (chunkSize - 1) - .5f);
				for (int z = 0; z < chunkSize; z++)
				{
					float height = (heightDisplacementMap != null) ? heightDisplacementMap[x, z] : 0;
					float zPos = ((float)z / (chunkSize - 1) - .5f);
					vertices[z + x * chunkSize] = new Vector3(xPos, height * heightScale, zPos);
					if (generateUvs)
						uvs[z + x * chunkSize] = new Vector2((float)x / (chunkSize - 1), (float)z / (chunkSize - 1));
				}
			}
	
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

			mesh.vertices = vertices;
			mesh.triangles = triangles;

			if (generateUvs)
				mesh.uv = uvs;
	
			if (heightDisplacementMap != null)
				for (int i = 0; i < chunkSize * chunkSize; i++)
					normals[i] = Vector3.up;
	
			mesh.normals = normals;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();

			return mesh;
        }

		public void SetHeightDisplacement(Sampler2D heightMap, float heigthScale)
		{
			heightDisplacementMap = heightMap;
			this.heightScale = heigthScale;
		}
    }
}