using UnityEngine;

namespace PW
{
	public enum PWChunkLoadMode
	{
		CUBIC,
		// PRIORITY_CUBIC,
		// PRIORITY_CIRCLE,
	}

	public interface PWTerrainInterface<T>
	{
		T RequestChunk(Vector3 pos, int seed);
	}
}