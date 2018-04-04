using UnityEngine;
using ProceduralWorlds.Core;
using ProceduralWorlds.Noises;

namespace ProceduralWorlds.Nodes
{
	public class NodePerlinNoise2D : BaseNode
	{
		public float			persistence = 1f;
		public float			lacunarity = 1.5f;
		public int				octaves = 4;
		public int				additionalSeed;

		public float			persistenceMin = 0.1f;
		public float			persistenceMax = 4f;
		public float			scale = 1f;

		[Output]
		public Sampler2D		output;

		public PerlinNoise2D	perlin2D;

		public override void OnNodeCreation()
		{
			name = "Perlin noise 2D";
		}

		public override void OnNodeEnable()
		{
			output = new Sampler2D(chunkSize, step);
			perlin2D = new PerlinNoise2D(seed);
		}

		public int GetSeed()
		{
			return (worldGraphRef != null) ? worldGraphRef.seed + additionalSeed : additionalSeed;
		}

		public override void OnNodeProcess()
		{
			//recalcul perlin noise values with new seed / position.
			output.ResizeIfNeeded(chunkSize, step);
			
			perlin2D.UpdateParams(GetSeed(), scale, octaves, persistence, lacunarity);

			perlin2D.ComputeSampler2D(output, chunkPosition);
		}
	}
}
