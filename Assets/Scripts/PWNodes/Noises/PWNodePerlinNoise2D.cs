using UnityEditor;
using UnityEngine;

namespace PW
{
	public class PWNodePerlinNoise2D : PWNode {
		
		public float		persistance;
		public int			octaves;

		[PWOutput]
		public Sampler2D	output;

		public override void OnNodeCreate()
		{
			name = "Perlin noise 2D";
			output = new Sampler2D(chunkSize);
		}

		Color c = Color.white;
		float min, max;
		public override void OnNodeGUI()
		{
			EditorGUI.BeginChangeCheck();
			{
				PWGUI.Slider("Persistance: ", ref persistance, ref min, ref max);
				PWGUI.IntSlider("Octaves: ", ref octaves, 0, 16);
				PWGUI.ColorPicker(ref c);
			}
			if (EditorGUI.EndChangeCheck())
				notifyDataChanged = true;

			//TODO: shader preview here

			PWGUI.Sampler2DPreview(output, needUpdate);
		}

		public override void OnNodeProcess()
		{
			if (chunkSizeHasChanged)
				output.Resize(chunkSize);

			//recalcul perlin noise values with new seed / position.
			if (needUpdate)
			{
				output.Foreach((x, y) => {
					float val = Mathf.PerlinNoise((float)x / 20f + seed, (float)y / 20f + seed);
					for (int i = 0; i < octaves; i++)
						val *= 1.2f;
					return val;
				});
			}
		}
	}
}
