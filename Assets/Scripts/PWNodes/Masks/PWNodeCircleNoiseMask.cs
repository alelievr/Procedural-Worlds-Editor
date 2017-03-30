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

		public override void OnNodeCreateOnce()
		{
			name = "2D noise mask";
		}

		public override	void OnNodeCreate()
		{
			mask = new Sampler2D(chunkSize);
		}

		//do not use inputs here, their values may be null.
		void	CreateNoiseMask()
		{
			Debug.Log("not null + " + Event.current.type);
			Vector2		center = new Vector2(samp.size / 2, samp.size / 2);
			float		maxDist = samp.size * radius; //simplified max dist to get better noiseMask.

			mask = new Sampler2D(chunkSize);
			maskTexture = new Texture2D(chunkSize, chunkSize, TextureFormat.RGBA32, false, false);
			mask.Foreach((x, y) => {
				float val = 1 - (Vector2.Distance(new Vector2(x, y), center) / maxDist);
				maskTexture.SetPixel(x, y, new Color(val, val, val));
				return val;
			});
			maskTexture.Apply();
		}

		public override	void OnNodeAnchorLink(string propName, int index)
		{
			if (propName == "samp")
			{
				Debug.Log("attached samp !");
				CreateNoiseMask();
			}
		}

		/*public override void OnNodeAnchorUnLink(string propName, int index)
		{

		}*/

		public override void OnNodeGUI()
		{
			//TODO: delete this protection !
			if (samp == null)
			{
				Debug.Log("null + " + Event.current.type);
				EditorGUILayout.LabelField("please connect the input");
				return ;
			}

			EditorGUIUtility.labelWidth = 70;
			blur = EditorGUILayout.Slider("blur", blur, 0, 1);
			EditorGUI.BeginChangeCheck();
			radius = EditorGUILayout.Slider("radius", radius, 0, 1);
			if (EditorGUI.EndChangeCheck())
				CreateNoiseMask();
			
			GUILayout.Label(maskTexture);
		}

		public override void OnNodeProcess()
		{
			if (chunkSizeHasChanged)
				CreateNoiseMask();
			//TODO: delete this protection !
			if (samp == null || mask == null)
				return ;
			samp.Foreach((x, y, val) => {return val * (mask[x, y]);});
			output = samp;
		}
	}
}