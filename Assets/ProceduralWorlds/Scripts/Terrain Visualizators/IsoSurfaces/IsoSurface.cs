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

		protected bool				useDynamicTriangleCount = false;

        protected Vector2[]			uvs;
        protected Vector3[]			vertices;
        protected Vector3[]			normals;
        protected int[] 			triangles;
		protected List< int >		traingleList = new List< int >();

		protected int				triangleIndex;

		public IsoSurfaceDebug		isoDebug = new IsoSurfaceDebug();
		
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
			
			triangleIndex = 0;
			traingleList.Clear();
		}

		protected Mesh GenerateMesh(bool recalculateNormals = false)
		{
			Mesh mesh = new Mesh();

			triangleIndex = 0;

			mesh.vertices = vertices;
			if (useDynamicTriangleCount)
				mesh.SetTriangles(traingleList, 0);
			else
				mesh.triangles = triangles;
			mesh.uv = uvs;
			mesh.normals = normals;

			if (recalculateNormals)
				mesh.RecalculateNormals();
			
			mesh.RecalculateBounds();

			return mesh;
		}

		protected void AddTriangle(int v1, int v2, int v3)
		{
			if (useDynamicTriangleCount)
			{
				traingleList.Add(v1);
				traingleList.Add(v2);
				traingleList.Add(v3);
			}
			else
			{
				triangles[triangleIndex++] = v1;
				triangles[triangleIndex++] = v2;
				triangles[triangleIndex++] = v3;
			}
		}
	}
}