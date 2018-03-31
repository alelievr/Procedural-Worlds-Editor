using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.IsoSurfaces
{
    public class Naive2DIsoSurface : IsoSurface
    {
		Sampler2D	heightDisplacementMap = null;
		float		heightScale;

		//TODO: implement flat shaded normals

        public override Mesh Generate(int chunkSize)
        {
			int		vertexCount = chunkSize * chunkSize;
			int		faceCount = (chunkSize - 1) * (chunkSize - 1);

			UpdateVerticesSize(vertexCount, faceCount * 2);

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
			for (int face = 0; face < faceCount; face++)
			{
				int i = face % (chunkSize - 1) + (face / (chunkSize - 1) * chunkSize);
	
				triangles[t++] = i + 1;
				triangles[t++] = i + chunkSize + 1;
				triangles[t++] = i + chunkSize;
	
				triangles[t++] = i;
				triangles[t++] = i + 1;
				triangles[t++] = i + chunkSize;
			}
	
			if (heightDisplacementMap == null)
				for (int i = 0; i < chunkSize * chunkSize; i++)
					normals[i] = Vector3.up;

			return GenerateMesh(true);
        }

		public void SetHeightDisplacement(Sampler2D heightMap, float heigthScale)
		{
			heightDisplacementMap = heightMap;
			this.heightScale = heigthScale;
		}
    }
}