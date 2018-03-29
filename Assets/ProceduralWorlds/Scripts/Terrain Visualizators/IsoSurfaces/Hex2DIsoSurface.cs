using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.IsoSurfaces
{
    public class Hex2DIsoSurface : IsoSurface
    {
        float       heightScale;
        Sampler2D   heightMap;

		float		oldHexSize;

		Vector3[]	hexPositions;
		Vector2[]	hexNearCoords;

		public Hex2DIsoSurface()
		{
			UpdateHexNearCoords();
		}

        public override Mesh Generate(int chunkSize)
        {
            int vertexCount = chunkSize * chunkSize * (6 + 1);
			int faceCount = chunkSize * chunkSize * 6;

			if (heightMap != null)
			{
				//add hex cell borders
				vertexCount += chunkSize * chunkSize * 6;
				faceCount += chunkSize * chunkSize * 6 * 2;
			}

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

					Vector3 pos = new Vector3(xPos, 0, zPos);

					for (int j = 0; j < 7; j++)
					{
						vertices[i + j] = hexPositions[j] + pos;
						if (heightMap != null)
							vertices[i + j].y = heightMap[x, z] * heightScale;
					}
					for (int j = 1; j < 6 + 1; j++)
						AddTriangle(i, (j == 6) ? i + 1 : i + j + 1, i + j);

					if (heightMap != null)
						GenerateHexBorders(x, z, chunkSize, pos);
				}
			}

            return GenerateMesh(true);
        }

		void GenerateHexBorders(int x, int z, int chunkSize, Vector3 pos)
		{
			int hexVertexIndex = (x + z * chunkSize) * (6 + 1) + 1;
			int borderVertexIndex = chunkSize * chunkSize * (6 + 1) + (x + z * chunkSize) * 6;
			if (x != 0 && z != 0 && z != chunkSize - 1 && x != chunkSize - 1)
			{

				//This vertex generation is totally wrong, you're assigning an hex vertex to a neighbour height which is not possible cauz each vertex have two neighbour height.
				for (int i = 0; i < 6; i++)
				{
					var nearCoord = hexNearCoords[i];
					Vector3 hexPos = pos + hexPositions[i + 1];
					float nearHeight = heightMap[x + (int)nearCoord.x, z + (int)nearCoord.y];
					float height = heightMap[x, z];

					// if (nearHeight < height)
						hexPos.y = nearHeight * heightScale;
					// else
						// hexPos.y = height * heightScale;
					
					vertices[borderVertexIndex + i] = hexPos;
				}

				for (int i = 0; i < 6; i++)
				{
					int nbv = (i == 5) ? 0 : i + 1;
					AddTriangle(hexVertexIndex + i, borderVertexIndex + i, hexVertexIndex + nbv);
					// AddTriangle(hexVertexIndex + i, borderVertexIndex + i, borderVertexIndex + nbv);
				}
			}
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

		void UpdateHexNearCoords()
		{
			//register neighbours coords
			hexNearCoords = new Vector2[6];

			//    / \ / \
			//   | 2 | 1 |
			//  / \ / \ / \
			// | 3 |   | 0 |
			//  \ / \ / \ /
			//   | 4 | 5 |
			//    \ / \ /

			hexNearCoords[0] = new Vector2(1, 0);
			hexNearCoords[1] = new Vector2(1, -1);
			hexNearCoords[2] = new Vector2(0, -1);
			hexNearCoords[3] = new Vector2(-1, 0);
			hexNearCoords[4] = new Vector2(0, 1);
			hexNearCoords[5] = new Vector2(1, 1);
		}

        public void SetHeightDisplacement(Sampler2D heightMap, float heightScale)
        {
            this.heightMap = heightMap;
            this.heightScale = heightScale;
        }
    }
}