using UnityEditor;
using UnityEngine;
using PW.Core;
using PW.Noises;

namespace PW.Node
{
	public class PWNodePerlinNoise2D : PWNode
	{
		public float		persistance;
		public int			octaves;
		public int			additionalSeed;

		public float		persistanceMin;
		public float		persistanceMax;
		public float		scale = 1f;

		[PWOutput]
		public Sampler2D	output;

		PerlinNoise2D		perlin2D;

		public override void OnNodeCreation()
		{
			name = "Perlin noise 2D";
		}

		public override void OnNodeEnable()
		{
			output = new Sampler2D(chunkSize, step);
			perlin2D = new PerlinNoise2D();
		}

		public override void OnNodeProcess()
		{
			//recalcul perlin noise values with new seed / position.
			output.ResizeIfNeeded(chunkSize, step);

			perlin2D.ComputeSampler(output, scale, seed + additionalSeed);
		}
	}
}
