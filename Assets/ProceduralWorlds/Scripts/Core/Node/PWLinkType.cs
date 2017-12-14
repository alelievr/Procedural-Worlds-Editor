using UnityEngine;

namespace PW.Core
{
	[System.SerializableAttribute]
	public enum PWLinkType
	{
		BasicData,
		TwoChannel,
		ThreeChannel,
		FourChannel,
		Sampler2D,
		Sampler3D,
		ChunkData,
	}
}
