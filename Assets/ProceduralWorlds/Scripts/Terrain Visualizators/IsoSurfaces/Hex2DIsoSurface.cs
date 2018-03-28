using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.IsoSurfaces
{
    public class Hex2DIsoSurface : IsoSurface
    {
        float       heightScale;
        Sampler2D   heigthMap;

		float		oldHexSize;

		Vector3[]	hexPositions;

        public override Mesh Generate(int chunkSize)
        {
            int vertexCount = chunkSize * chunkSize * (6 + 1);
			int faceCount = chunkSize * chunkSize * 6;

			UpdateVerticesSize(vertexCount, faceCount);

			UpdateHexPositions(chunkSize);

			float hexMinRadius = Mathf.Cos(Mathf.Deg2Rad * 30);
			float hexDecal = hexMinRadius * hexMinRadius;
			float f = 1f / chunkSize * hexMinRadius;

			int t = 0;
			for (int x = 0; x < chunkSize; x++)
			{
				for (int z = 0; z < chunkSize; z++)
				{
					int i = (x + z * chunkSize) * (6 + 1);
					float zPos = ((float)z * hexDecal / chunkSize);
					float xPos = ((float)x * hexMinRadius / chunkSize) - ((z % 2 == 1) ? f / 2 : 0);

					for (int j = 0; j < 7; j++)
					{
						vertices[i + j] = hexPositions[j] + new Vector3(xPos, 0, zPos);
						if (heigthMap != null)
							vertices[i + j].y = heigthMap[x, z]  * heightScale;
					}
					for (int j = 1; j < 6 + 1; j++)
					{
						triangles[t++] = i; //hex center;
						triangles[t++] = (j == 6) ? i + 1 : i + j + 1;
						triangles[t++] = i + j;
					}
				}
			}

            return GenerateMesh(true);
        }

		void UpdateHexPositions(int chunkSize)
		{
			float hexSize = 1f / (float)chunkSize / 2f;

			if (oldHexSize == hexSize)
				return ;

			//     2
			//  3 / \ 1
			//   | 0 | 
			//  4 \ / 6
			//     5 

			hexPositions = new Vector3[8];
			hexPositions[0] = Vector3.zero;
			hexPositions[1] = new Vector3(Mathf.Cos(Mathf.Deg2Rad * 30) * hexSize, 0, Mathf.Sin(Mathf.Deg2Rad * 30) * hexSize);
			hexPositions[2] = new Vector3(Mathf.Cos(Mathf.Deg2Rad * 90) * hexSize, 0, Mathf.Sin(Mathf.Deg2Rad * 90) * hexSize);
			hexPositions[3] = new Vector3(Mathf.Cos(Mathf.Deg2Rad * 150) * hexSize, 0, Mathf.Sin(Mathf.Deg2Rad * 150) * hexSize);
			hexPositions[4] = -hexPositions[1];
			hexPositions[5] = -hexPositions[2];
			hexPositions[6] = -hexPositions[3];

			oldHexSize = hexSize;
		}

        public void SetHeightDisplacement(Sampler2D heigthMap, float heightScale)
        {
            this.heigthMap = heigthMap;
            this.heightScale = heightScale;
        }
    }
}