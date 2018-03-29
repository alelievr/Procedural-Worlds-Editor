using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.IsoSurfaces
{
	public enum NormalGenerationMode
	{
		Flat,
		Smooth,
	}

	public abstract class IsoSurface
	{
		public bool					generateUvs = true;
		public bool					generateSharedVertices = true;

        protected Vector2[]			uvs;
        protected Vector3[]			vertices;
        protected Vector3[]			normals;
        protected int[] 			triangles;

		protected int				triangleIndex;
		
		[System.NonSerialized]
		int							oldVertexCount = -1;

		public NormalGenerationMode	normalGenerationMode;

		public abstract Mesh Generate(int chunkSize);

		protected void UpdateVerticesSize(int vertexCount, int faceCount)
		{
			if (vertexCount != oldVertexCount)
			{
				vertices = new Vector3[vertexCount];
				normals = new Vector3[vertexCount];
				uvs = new Vector2[vertexCount];
				triangles = new int[faceCount * 3];

				oldVertexCount = vertexCount;
			}
		}

		protected Mesh GenerateMesh(bool recalculateNormals = false)
		{
			Mesh mesh = new Mesh();

			mesh.vertices = vertices;
			mesh.triangles = triangles;
			mesh.uv = uvs;
			mesh.normals = normals;

			if (recalculateNormals)
				mesh.RecalculateNormals();
			
			mesh.RecalculateBounds();

			triangleIndex = 0;

			return mesh;
		}

		protected void AddTriangle(int v1, int v2, int v3)
		{
			triangles[triangleIndex++] = v1;
			triangles[triangleIndex++] = v2;
			triangles[triangleIndex++] = v3;
		}
	}
}