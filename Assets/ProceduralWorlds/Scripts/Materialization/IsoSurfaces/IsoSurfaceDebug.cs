using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.IsoSurfaces
{
	public class IsoSurfaceDebug : VisualDebug
	{
		readonly Dictionary< int, Vector3 > vertices = new Dictionary< int, Vector3 >();

		public void DrawVertex(Vector3 pos, int index, int chunkSize)
		{
			vertices[index] = pos * chunkSize;
			DrawPoint(pos * chunkSize, .1f);
		}
		
		public void DrawTriangle(int i1, int i2, int i3, Color color)
		{
			DrawTriangle(vertices[i1], vertices[i2], vertices[i3], color);
		}
		
		public void DrawTriangle(int i1, int i2, int i3)
		{
			DrawTriangle(vertices[i1], vertices[i2], vertices[i3]);
		}
	}
}
	