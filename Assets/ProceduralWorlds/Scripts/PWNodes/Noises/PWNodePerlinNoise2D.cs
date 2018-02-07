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

		[PWOutput]
		public Sampler2D	output;

		[SerializeField]
		float				persistanceMin;
		[SerializeField]
		float				persistanceMax;

		PerlinNoise2D		perlin2D;

		const string noiseSettingsChangedKey = "PerlinNoiseSettings";

		public override void OnNodeCreation()
		{
			name = "Perlin noise 2D";
		}

		public override void OnNodeEnable()
		{
			output = new Sampler2D(chunkSize, step);
			perlin2D = new PerlinNoise2D();
			delayedChanges.BindCallback(noiseSettingsChangedKey, (unused) => NotifyReload());
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 40;
			EditorGUI.BeginChangeCheck();
			{
				persistance = PWGUI.Slider("Persistance: ", persistance, ref persistanceMin, ref persistanceMax);
				octaves = PWGUI.IntSlider("Octaves: ", octaves, 0, 16);
				additionalSeed = EditorGUILayout.IntField("Seed", additionalSeed);
			}
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(noiseSettingsChangedKey);

			PWGUI.Sampler2DPreview(output);
		}

		public override void OnNodeProcess()
		{
			//recalcul perlin noise values with new seed / position.
			output.ResizeIfNeeded(graphRef.chunkSize, graphRef.step);

			float scale = 40f;
			perlin2D.ComputeSampler(output, seed + additionalSeed);
			// output.Foreach((x, y) => {
			// 	float nx = (float)x * step + chunkPosition.x;
			// 	float ny = (float)y * step + chunkPosition.z;
			// 	float val = PerlinNoise2D.GenerateNoise(nx / scale, ny / scale, 2, 2, 1, 1, seed + additionalSeed);
			// 	for (int i = 0; i < octaves; i++)
			// 		val *= 1.2f;
			// 	return val;
			// });
		}
	}
}
