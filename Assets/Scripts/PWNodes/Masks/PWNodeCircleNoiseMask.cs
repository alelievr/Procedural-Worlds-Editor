using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeCircleNoiseMask : PWNode {
	
		[PWInput]
		public Sampler2D	samp;

		[PWOutput]
		public Sampler2D	output;

		[SerializeField]
		float	blur = .5f;
		[SerializeField]
		float	radius = .5f;

		Sampler2D			mask;
		Texture2D			maskTexture;

		public override void OnNodeCreateOnce()
		{
			name = "2D noise mask";
		}

		public override	void OnNodeCreate()
		{
			mask = new Sampler2D(chunkSize, step);
		}

		void	CreateNoiseMask()
		{
			Vector2		center = new Vector2(samp.size / 2, samp.size / 2);
			float		maxDist = samp.size * radius; //simplified max dist to get better noiseMask.

			mask.Resize(samp.size);
			maskTexture = new Texture2D(chunkSize, chunkSize, TextureFormat.RGBA32, false, false);
			mask.Foreach((x, y) => {
				float val = 1 - (Vector2.Distance(new Vector2(x, y), center) / maxDist);
				maskTexture.SetPixel(x, y, new Color(val, val, val));
				return val;
			});
			maskTexture.Apply();
		}

		public override	bool OnNodeAnchorLink(string propName, int index)
		{
			if (propName == "samp")
				CreateNoiseMask();
			return true;
		}

		/*public override void OnNodeAnchorUnLink(string propName, int index)
		{

		}*/

		public override void OnNodeGUI()
		{
			//TODO: delete this protection !
			if (samp == null)
			{
				EditorGUILayout.LabelField("please connect the input noise (Sampler2D)");
				return ;
			}

			EditorGUIUtility.labelWidth = 70;
			EditorGUI.BeginChangeCheck();
			{
				blur = EditorGUILayout.Slider("blur", blur, 0, 1);
				radius = EditorGUILayout.Slider("radius", radius, 0, 1);
			}
			if (EditorGUI.EndChangeCheck())
			{
				CreateNoiseMask();
				notifyDataChanged = true;
			}
			
			GUILayout.Label(maskTexture);
		}

		public override void OnNodeProcess()
		{
			if (needUpdate)
			{
				CreateNoiseMask();
				samp.Foreach((x, y, val) => {return val * (mask[x, y]);});
			}
			output = samp;
		}
	}
}
