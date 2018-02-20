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
		[SerializeField]
		float				scale = 1f;

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
				scale = PWGUI.Slider("Scale: ", scale, 0.01f, 10);
				additionalSeed = EditorGUILayout.IntField("Seed", additionalSeed);
			}
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(noiseSettingsChangedKey);

			PWGUI.Sampler2DPreview(output);
		}

		public override void OnNodeProcess()
		{
			//recalcul perlin noise values with new seed / position.
			output.ResizeIfNeeded(chunkSize, step);

			perlin2D.ComputeSampler(output, scale, seed + additionalSeed);
		}
	}
}
