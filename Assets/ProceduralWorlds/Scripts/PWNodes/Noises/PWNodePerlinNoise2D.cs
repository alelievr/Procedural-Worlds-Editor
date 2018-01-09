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

		[PWOutput]
		public Sampler2D	output;

		[SerializeField]
		float				persistanceMin;
		[SerializeField]
		float				persistanceMax;

		const string noiseSettingsChangedKey = "PerlinNoiseSettings";

		public override void OnNodeCreation()
		{
			name = "Perlin noise 2D";
		}

		public override void OnNodeEnable()
		{
			output = new Sampler2D(chunkSize, step);
			graphRef.OnChunkSizeChanged += ChunkSizeChanged;
			delayedChanges.BindCallback(noiseSettingsChangedKey, (unused) => NotifyReload());
		}

		public override void OnNodeDisable()
		{
			graphRef.OnChunkPositionChanged -= ChunkSizeChanged;
		}

		void ChunkSizeChanged()
		{
			output.Resize(chunkSize);
		}

		public override void OnNodeGUI()
		{
			EditorGUI.BeginChangeCheck();
			{
				persistance = PWGUI.Slider("Persistance: ", persistance, ref persistanceMin, ref persistanceMax);
				octaves = PWGUI.IntSlider("Octaves: ", octaves, 0, 16);
			}
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(noiseSettingsChangedKey);

			PWGUI.Sampler2DPreview(output);
		}

		public override void OnNodeProcess()
		{
			Debug.Log("Process !");

			//recalcul perlin noise values with new seed / position.
			float scale = 40f;
			output.Foreach((x, y) => {
				float nx = (float)x * step + chunkPosition.x;
				float ny = (float)y * step + chunkPosition.z;
				float val = PerlinNoise2D.GenerateNoise(nx / scale, ny / scale, 2, 2, 1, 1, seed);
				for (int i = 0; i < octaves; i++)
					val *= 1.2f;
				return val;
			});
		}
	}
}
