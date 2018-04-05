// #define DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using System.Linq;

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

		int			currentChunkSize;

		readonly float[]	neighbourHeights = new float[6];

		public Hex2DIsoSurface()
		{
			useDynamicTriangleCount = true;
			UpdateHexNearCoords();
		}

        public override Mesh Generate(int chunkSize, Vector3 chunkPosition = default(Vector3))
        {
            int vertexCount = chunkSize * chunkSize * (6 + 1);
			int faceCount = chunkSize * chunkSize * 6;

			currentChunkSize = chunkSize;

			if (heightMap != null)
			{
				//add hex cell borders
				vertexCount += chunkSize * chunkSize * 6;
				faceCount += chunkSize * chunkSize * 6 * 2;
			}

			UpdateVerticesSize(vertexCount, faceCount);

			UpdateHexPositions(chunkSize);

			#if DEBUG
				isoDebug.Initialize();
				for (int x = 0; x < chunkSize; x++)
					for (int z = 0; z < chunkSize; z++)
					{
						Vector3 pos = GetPositionFromCoords(x, z, (heightMap != null) ? heightMap[x, z] * heightScale : 0);
						isoDebug.DrawLabel(pos * chunkSize, x + " / " + z + " | " + (pos.y / heightScale));
					}
			#endif

			for (int x = 0; x < chunkSize; x++)
			{
				for (int z = 0; z < chunkSize; z++)
				{
					int i = (x + z * chunkSize) * (6 + 1);
					
					Vector3 pos = GetPositionFromCoords(x, z);
					
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

		#region Hexagon borders Borders

		float SafeGetHeight(int x, int z, float defaultValue, Sampler2D terrain = null)
		{
			if (terrain == null)
				terrain = heightMap;
			
			if (x < 0 || x >= currentChunkSize || z < 0 || z >= currentChunkSize)
				return defaultValue;
			return terrain[x, z];
		}

		float GetNeighbourHeight(int x, int z, int index)
		{
			//Yeah i know, it seems to be black magic, but trust me it works !
			int i1 = (-index + 6) % 6;
			int i2 = (-index + 11) % 6;
			var neighbourCoord1 = (z % 2 == 0) ? evenHexNeighbourCoords[i1] : oddHexNeighbourCoords[i1];
			var neighbourCoord2 = (z % 2 == 0) ? evenHexNeighbourCoords[i2] : oddHexNeighbourCoords[i2];
			float neighbourHeight1 = SafeGetHeight(x + (int)neighbourCoord1.x, z + (int)neighbourCoord1.y, heightMap[x, z]);
			float neighbourHeight2 = SafeGetHeight(x + (int)neighbourCoord2.x, z + (int)neighbourCoord2.y, heightMap[x, z]);

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
				float neighbourHeight = GetNeighbourHeight(x, z, i);
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

				AddTriangle(hexVertexIndex + i, hexVertexIndex + nbv, borderVertexIndex + i);
				AddTriangle(hexVertexIndex + nbv, borderVertexIndex + nbv, borderVertexIndex + i);

				#if DEBUG
					isoDebug.DrawTriangle(hexVertexIndex + i, hexVertexIndex + nbv, borderVertexIndex + i, Color.red);
					isoDebug.DrawTriangle(hexVertexIndex + nbv, borderVertexIndex + nbv, borderVertexIndex + i, Color.blue);
				#endif
			}
		}

		float SafeGetNeighbourChunkHeight(int x, int z, float defaultValue, Vector3 chunkDirection, Sampler2D currentTerrain, Sampler2D terrain)
		{
			//Corner:
			if (x >= currentChunkSize && z < 0)
				return terrain[x - currentChunkSize, z + currentChunkSize];
			else if (x < 0 && z >= currentChunkSize)
				return terrain[x + currentChunkSize, z - currentChunkSize];

			//Edges:
			if (x >= currentChunkSize && chunkDirection.x > 0 && z >= 0 && z < currentChunkSize)
				return terrain[x - currentChunkSize, z];
			else if (x < 0 && chunkDirection.x < 0 && z >= 0 && z < currentChunkSize)
				return terrain[x + currentChunkSize, z];
			else if (z >= currentChunkSize && chunkDirection.z > 0 && x >= 0 && x < currentChunkSize)
				return terrain[x, z - currentChunkSize];
			else if (z < 0 && chunkDirection.z < 0 && x >= 0 && x < currentChunkSize)
				return terrain[x, z + currentChunkSize];
			
			return SafeGetHeight(x, z, defaultValue, currentTerrain);
		}

		float GetNeighbourChunkHeight(int x, int z, int index, float defaultValue, Vector3 chunkDirection, Sampler2D currentTerrain, Sampler2D chunkTerrain)
		{
			//Yeah i know, it seems to be black magic, but trust me it works !
			int i1 = (-index + 6) % 6;
			int i2 = (-index + 11) % 6;
			var neighbourCoord1 = (z % 2 == 0) ? evenHexNeighbourCoords[i1] : oddHexNeighbourCoords[i1];
			var neighbourCoord2 = (z % 2 == 0) ? evenHexNeighbourCoords[i2] : oddHexNeighbourCoords[i2];
			float neighbourHeight1 = SafeGetNeighbourChunkHeight(x + (int)neighbourCoord1.x, z + (int)neighbourCoord1.y, defaultValue, chunkDirection, currentTerrain, chunkTerrain);
			float neighbourHeight2 = SafeGetNeighbourChunkHeight(x + (int)neighbourCoord2.x, z + (int)neighbourCoord2.y, defaultValue, chunkDirection, currentTerrain, chunkTerrain);
			
			#if DEBUG
				isoDebug.DrawLabel(GetPositionFromCoords(x + (int)neighbourCoord1.x, z + (int)neighbourCoord1.y, neighbourHeight1 * heightScale + .01f) * currentChunkSize, "nheight1: " + neighbourHeight1);
				isoDebug.DrawLabel(GetPositionFromCoords(x + (int)neighbourCoord2.x, z + (int)neighbourCoord2.y, neighbourHeight2 * heightScale - .01f) * currentChunkSize, "nheight2: " + neighbourHeight2);
			#endif

			return Mathf.Min(neighbourHeight1, neighbourHeight2);
		}

		void UpdateBorderForChunk(Vector3 chunkDirection, Sampler2D chunk, Mesh chunkMesh, Sampler2D neighbourChunk)
		{
			var vertices = chunkMesh.vertices;

			var borderPos = GetBorderPositions(chunkDirection).ToList();

			for (int b = 0; b < borderPos.Count; b++)
			{
				int x = (int)borderPos[b].x;
				int z = (int)borderPos[b].y;
				int borderVertexIndex = currentChunkSize * currentChunkSize * (6 + 1) + (x + z * currentChunkSize) * 6;
				float h = chunk[x, z];

				#if DEBUG
					isoDebug.BeginFrame("Chunk border update");
				#endif

				for (int i = 0; i < 6; i++)
				{
					float neighbourHeight = GetNeighbourChunkHeight(x, z, i, h, chunkDirection, chunk, neighbourChunk);
	
					float newHeight = neighbourHeight * heightScale;

					if (newHeight >= vertices[borderVertexIndex + i].y)
						continue ;
					
					vertices[borderVertexIndex + i].y = newHeight;
				}
			}

			chunkMesh.vertices = vertices;

			chunkMesh.RecalculateBounds();
		}

		public void UpdateMeshBorder(TerrainStorage terrainStorage, Vector3 position, Vector3 neighbourPosition)
		{
			var chunk = terrainStorage[position];
			var neighbourChunk = terrainStorage[neighbourPosition];

			Sampler2D terrain = chunk.terrainData.terrain as Sampler2D;
			Sampler2D neighbourTerrain = neighbourChunk.terrainData.terrain as Sampler2D;

			Mesh terrainMesh = (chunk.userData as GameObject).GetComponent< MeshFilter >().sharedMesh;
			Mesh neighbourMesh = (neighbourChunk.userData as GameObject).GetComponent< MeshFilter >().sharedMesh;
			
			Vector3 chunkDirection = neighbourPosition - position;

			UpdateBorderForChunk(chunkDirection, terrain, terrainMesh, neighbourTerrain);
			UpdateBorderForChunk(-chunkDirection, neighbourTerrain, neighbourMesh, terrain);
		}

		#endregion

		#region Utils

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
		
		IEnumerable< Vector2 > GetBorderPositions(Vector3 chunkDirection)
		{
			if (chunkDirection.x > 0)
				for (int i = 0; i < currentChunkSize; i++)
					yield return new Vector2(currentChunkSize - 1, i);
			if (chunkDirection.x < 0)
				for (int i = 0; i < currentChunkSize; i++)
					yield return new Vector2(0, i);
			if (chunkDirection.z < 0)
				for (int i = 0; i < currentChunkSize; i++)
					yield return new Vector2(i, 0);
			if (chunkDirection.z > 0)
				for (int i = 0; i < currentChunkSize; i++)
					yield return new Vector2(i, currentChunkSize - 1);
		}

		Vector3 GetPositionFromCoords(int x, int z, float height = 0)
		{
			float hexMinRadius = Mathf.Cos(Mathf.Deg2Rad * 30);
			float hexDecal = hexMinRadius * hexMinRadius;
			float f = 1f / currentChunkSize * hexMinRadius;

			float zPos = ((float)z * hexDecal / currentChunkSize);
			float xPos = ((float)x * hexMinRadius / currentChunkSize) - ((Mathf.Abs(z) % 2 == 1) ? f / 2 : 0);

			return new Vector3(xPos, height, zPos);
		}

		#endregion

    }
}