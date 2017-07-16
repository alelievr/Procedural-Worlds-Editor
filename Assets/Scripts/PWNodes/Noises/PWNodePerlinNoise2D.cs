using UnityEditor;
using UnityEngine;
using PW.Core;

namespace PW.Node
{
	public class PWNodePerlinNoise2D : PWNode {
		
		public float		persistance;
		public int			octaves;

		[PWOutput]
		public Sampler2D	output;

		[SerializeField]
		float				persistanceMin;
		[SerializeField]
		float				persistanceMax;

		public override void OnNodeCreate()
		{
			name = "Perlin noise 2D";
			output = new Sampler2D(chunkSize, step);
		}

		public override void OnNodeGUI()
		{
			EditorGUI.BeginChangeCheck();
			{
				PWGUI.Slider("Persistance: ", ref persistance, ref persistanceMin, ref persistanceMax);
				PWGUI.IntSlider("Octaves: ", ref octaves, 0, 16);
			}
			if (EditorGUI.EndChangeCheck())
				notifyDataChanged = true;

			PWGUI.Sampler2DPreview(output, needUpdate);
		}

		public override void OnNodeProcess()
		{
			if (chunkSizeHasChanged)
				output.Resize(chunkSize);

			//recalcul perlin noise values with new seed / position.
			if (needUpdate)
			{
				float scale = 20f;
				output.Foreach((x, y) => {
					float nx = (float)x * step + chunkPosition.x;
					float ny = (float)y * step + chunkPosition.z;
					float val = Mathf.PerlinNoise(nx / scale + (float)seed, ny / scale + (float)seed);
					for (int i = 0; i < octaves; i++)
						val *= 1.2f;
					return val;
				});
			}
		}
	}
}
