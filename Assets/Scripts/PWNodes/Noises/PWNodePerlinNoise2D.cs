using UnityEditor;

namespace PW
{
	public class PWNodePerlinNoise2D : PWNode {
		
		public float		persistance;
		public int			octaves;

		[PWOutput("OUT")]
		public Sampler2D	output;

		public override void OnNodeCreate()
		{
			name = "Perlin noise 2D";
		}

		public override void OnNodeGUI()
		{
			persistance = EditorGUILayout.Slider("Persistance", persistance, 0, 1);
			octaves = EditorGUILayout.IntSlider("Octaves", octaves, 0, 32);
		}

		public override void OnNodeProcess()
		{
			if (seedHasChanged || positionHasChanged)
			{
				//recalcul perlin noise values with new seed / position.
			}
		}
	}
}
