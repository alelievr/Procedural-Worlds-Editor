// #define DEBUG

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
		Vector2[]	evenHexNeighbourCoords;
		Vector2[]	oddHexNeighbourCoords;

		float[]		neighbourHeights = new float[6];

		TerrainStorage	terrainStorage;
		Vector3			currentChunkPosition;

		public Hex2DIsoSurface()
		{
			useDynamicTriangleCount = true;
			UpdateHexNearCoords();
		}

		public void UpdateTerrainStorage(TerrainStorage terrainStorage)
		{
			this.terrainStorage = terrainStorage;
		}

        public override Mesh Generate(int chunkSize, Vector3 chunkPosition = default(Vector3))
        {
            int vertexCount = chunkSize * chunkSize * (6 + 1);
			int faceCount = chunkSize * chunkSize * 6;

			currentChunkPosition = chunkPosition;

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

			#if DEBUG
				isoDebug.Initialize();
				for (int x = 0; x < chunkSize; x++)
					for (int z = 0; z < chunkSize; z++)
					{
						float zPos = ((float)z * hexDecal / chunkSize);
						float xPos = ((float)x * hexMinRadius / chunkSize) - ((z % 2 == 1) ? f / 2 : 0);
						float height = (heightMap != null) ? heightMap[x, z] * heightScale : 0;
						isoDebug.DrawLabel(new Vector3(xPos * chunkSize, height * chunkSize + .1f, zPos * chunkSize), x + " / " + z);
					}
			#endif

			for (int x = 0; x < chunkSize; x++)
			{
				for (int z = 0; z < chunkSize; z++)
				{
					int i = (x + z * chunkSize) * (6 + 1);
					float zPos = ((float)z * hexDecal / chunkSize);
					float xPos = ((float)x * hexMinRadius / chunkSize) - ((z % 2 == 1) ? f / 2 : 0);

					Vector3 pos = new Vector3(xPos, 0, zPos);
					
					#if DEBUG
						isoDebug.BeginFrame("Hex " + x + " / " + z);
					#endif

					for (int j = 0; j < 7; j++)
					{
						vertices[i + j] = hexPositions[j] + pos;
						if (heightMap != null)
							vertices[i + j].y = heightMap[x, z] * heightScale;
						
						#if DEBUG
							isoDebug.DrawVertex(vertices[i + j], i + j, chunkSize);
						#endif
					}
					for (int j = 1; j < 6 + 1; j++)
					{
						int i2 = (j == 6) ? i + 1 : i + j + 1;
						AddTriangle(i, i2, i + j);

						#if DEBUG
							isoDebug.DrawTriangle(i, i2, i + j);
						#endif
					}

					if (heightMap != null)
						GenerateHexBorders(x, z, chunkSize, pos);
				}
			}
			
			for (int i = 0; i < vertexCount; i++)
				normals[i] = Vector3.up;

            return GenerateMesh(false);
        }

		float SafeGetHeight(int x, int z, int chunkSize, int defaultX = 0, int defaultZ = 0)
		{
			if (x < 0 && z < 0)
			{
				var chunkData = terrainStorage.GetChunkDatas(currentChunkPosition);
				//TODO: get the neighbour chunk datas and access them to generate polys
			}
			else if (x < 0 || x >= chunkSize || z < 0 || z >= chunkSize)
				return heightMap[defaultX, defaultZ];
			return heightMap[x, z];
		}

		float GetNeighbourHeight(int x, int z, int index, int chunkSize)
		{
			//Yeah i know, it seems to be black magic, but trust me it works !
			int i1 = (-index + 6) % 6;
			int i2 = (-index + 11) % 6;
			var neighbourCoord1 = (z % 2 == 0) ? evenHexNeighbourCoords[i1] : oddHexNeighbourCoords[i1];
			var neighbourCoord2 = (z % 2 == 0) ? evenHexNeighbourCoords[i2] : oddHexNeighbourCoords[i2];
			float neighbourHeight1 = SafeGetHeight(x + (int)neighbourCoord1.x, z + (int)neighbourCoord1.y, chunkSize, x, z);
			float neighbourHeight2 = SafeGetHeight(x + (int)neighbourCoord2.x, z + (int)neighbourCoord2.y, chunkSize, x, z);

			return Mathf.Min(neighbourHeight1, neighbourHeight2);
		}

		void GenerateHexBorders(int x, int z, int chunkSize, Vector3 pos)
		{
			int hexVertexIndex = (x + z * chunkSize) * (6 + 1) + 1;
			int borderVertexIndex = chunkSize * chunkSize * (6 + 1) + (x + z * chunkSize) * 6;
			float height = heightMap[x, z];

			#if DEBUG
				isoDebug.BeginFrame("Hex border of " + x + " / " + z);
			#endif

			for (int i = 0; i < 6; i++)
			{
				float neighbourHeight = GetNeighbourHeight(x, z, i, chunkSize);
				neighbourHeights[i] = neighbourHeight;
				
				Vector3 hexPos = pos + hexPositions[i + 1];

				if (neighbourHeight < height)
					hexPos.y = neighbourHeight * heightScale;
				else
					hexPos.y = height * heightScale;
				
				vertices[borderVertexIndex + i] = hexPos;

				#if DEBUG
					isoDebug.DrawVertex(vertices[borderVertexIndex + i], borderVertexIndex + i, chunkSize);
				#endif
			}

			for (int i = 0; i < 6; i++)
			{
				int nbv = (i + 1) % 6;

				//we remove useless flat triangles
				if (height == neighbourHeights[i])
					continue ;
				
				AddTriangle(hexVertexIndex + i, hexVertexIndex + nbv, borderVertexIndex + i);
				AddTriangle(hexVertexIndex + nbv, borderVertexIndex + nbv, borderVertexIndex + i);

				#if DEBUG
					isoDebug.DrawTriangle(hexVertexIndex + i, hexVertexIndex + nbv, borderVertexIndex + i, Color.red);
					isoDebug.DrawTriangle(hexVertexIndex + nbv, borderVertexIndex + nbv, borderVertexIndex + i, Color.blue);
				#endif
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
			evenHexNeighbourCoords = new Vector2[6];
			oddHexNeighbourCoords = new Vector2[6];

			//    / \ / \
			//   | 2 | 1 |
			//  / \ / \ / \
			// | 3 |   | 0 |
			//  \ / \ / \ /
			//   | 4 | 5 |
			//    \ / \ /

			evenHexNeighbourCoords[0] = new Vector2(1, 0);
			evenHexNeighbourCoords[1] = new Vector2(1, -1);
			evenHexNeighbourCoords[2] = new Vector2(0, -1);
			evenHexNeighbourCoords[3] = new Vector2(-1, 0);
			evenHexNeighbourCoords[4] = new Vector2(0, 1);
			evenHexNeighbourCoords[5] = new Vector2(1, 1);
			
			oddHexNeighbourCoords[0] = new Vector2(1, 0);
			oddHexNeighbourCoords[1] = new Vector2(0, -1);
			oddHexNeighbourCoords[2] = new Vector2(-1, -1);
			oddHexNeighbourCoords[3] = new Vector2(-1, 0);
			oddHexNeighbourCoords[4] = new Vector2(-1, 1);
			oddHexNeighbourCoords[5] = new Vector2(0, 1);
		}

        public void SetHeightDisplacement(Sampler2D heightMap, float heightScale)
        {
            this.heightMap = heightMap;
            this.heightScale = heightScale;
        }
    }
}