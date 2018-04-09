using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.IsoSurfaces
{
	[System.Serializable]
	public class Naive2DIsoSurfaceSettings : IsoSurfaceSettings
	{
		public bool			heightDisplacement;
		public Sampler2D	heightMap;
		public float		heightScale;

		public void Update(int chunkSize, Sampler2D heightMap = null)
		{
			this.heightMap = (heightDisplacement) ? heightMap : null;
			base.chunkSize = chunkSize;
		}
	}
	
    public class Naive2DIsoSurface : IsoSurface
    {

        public override Mesh Generate(IsoSurfaceSettings settings)
        {
			int		cs = settings.chunkSize;
			int		vertexCount = cs * cs;
			int		faceCount = (cs - 1) * (cs - 1);
			var		ns = settings as Naive2DIsoSurfaceSettings;

			UpdateVerticesSize(vertexCount, faceCount * 2);
			
			for (int x = 0; x < cs; x++)
			{
				float xPos = ((float)x / (cs - 1) - .5f);
				for (int z = 0; z < cs; z++)
				{
					float height = (ns.heightMap != null) ? ns.heightMap[x, z] : 0;
					float zPos = ((float)z / (cs - 1) - .5f);
					vertices[z + x * cs] = new Vector3(xPos, height * ns.heightScale, zPos);
					if (ns.generateUvs)
						uvs[z + x * cs] = new Vector2((float)x / (cs - 1), (float)z / (cs - 1));
				}
			}
	
			int t = 0;
			for (int face = 0; face < faceCount; face++)
			{
				int i = face % (cs - 1) + (face / (cs - 1) * cs);
	
				triangles[t++] = i + 1;
				triangles[t++] = i + cs + 1;
				triangles[t++] = i + cs;
	
				triangles[t++] = i;
				triangles[t++] = i + 1;
				triangles[t++] = i + cs;
			}
	
			if (ns.heightMap == null)
			{
				for (int i = 0; i < cs * cs; i++)
					normals[i] = Vector3.up;

				return GenerateMesh(false);
			}

			return GenerateMesh(true);
        }
    }
}