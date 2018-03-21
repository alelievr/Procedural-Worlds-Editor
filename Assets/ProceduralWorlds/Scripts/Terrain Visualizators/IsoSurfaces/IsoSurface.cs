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

		public NormalGenerationMode	normalGenerationMode;

		public abstract Mesh Generate(int chunkSize);
	}
}