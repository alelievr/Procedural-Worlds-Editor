using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWNodeCircleNoiseMask : PWNode {
	
		[PWInput("MAP")]
		public Sampler2D	samp;

		[PWOutput("OUT")]
		public Sampler2D	output;

		[SerializeField]
		float	blur = .5f;
		[SerializeField]
		float	radius = .5f;

		Sampler2D			mask;
		Texture2D			maskTexture;

		public override void OnNodeCreate()
		{
			name = "2D noise mask";
			CreateNoiseMask();

			GUILayout.Label(maskTexture);
		}

		void	CreateNoiseMask()
		{
			Vector2		center = new Vector2(samp.size / 2, samp.size / 2);
			float		maxDist = samp.size * radius; //simplified max dist to get better noiseMask.

			mask = new Sampler2D(chunkSize);
			maskTexture = new Texture2D(chunkSize, chunkSize, TextureFormat.Alpha8, false, false);
			mask.Foreach((x, y) => {
				float val = Vector2.Distance(new Vector2(x, y), center) / maxDist;
				maskTexture.SetPixel(x, y, Color.white * val);
				return val;
			});
			maskTexture.Apply();
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 70;
			blur = EditorGUILayout.Slider("blur", blur, 0, 1);
			radius = EditorGUILayout.Slider("blur", radius, 0, 1);
		}

		public override void OnNodeProcess()
		{
			if (chunkSizeHasChanged)
				CreateNoiseMask();
			samp.Foreach((x, y, val) => {return val * mask[x, y];});
			output = samp;
		}
	}
}