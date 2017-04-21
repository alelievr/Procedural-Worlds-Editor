using UnityEditor;
using UnityEngine;

namespace PW
{
	public class PWNodeTopDown2DTerrain : PWNode {
	
		[PWInput("TEX")]
		public Sampler2D		texture;

		[PWOutput("MAP")]
		public TopDown2DData	terrainOutput;

		private Texture2D		samplerTexture;

		public override void OnNodeCreate()
		{
			name = "2D TopDown terrain";
			samplerTexture = new Texture2D(chunkSize, chunkSize, TextureFormat.ARGB32, false, false);
			texture = new Sampler2D(chunkSize);
			terrainOutput = new TopDown2DData();
		}

		public override void OnNodeGUI()
		{
			EditorGUILayout.LabelField("MAP:");

			GUILayout.Label(samplerTexture, GUILayout.Width(100), GUILayout.Height(100));
		}

		public override void OnNodeAnchorLink(string propName, int index)
		{
			if (propName == "texture")
				samplerTexture = new Texture2D(texture.size, texture.size, TextureFormat.ARGB32, false, false);
		}

		public override void OnNodeProcess()
		{
			if (chunkSizeHasChanged)
				samplerTexture = new Texture2D(chunkSize, chunkSize, TextureFormat.ARGB32, false, false);
			if (needUpdate)
			{
				texture.Foreach((x, y, val) => {samplerTexture.SetPixel(x, y, new Color(0, 0, val));});
				samplerTexture.Apply();
			}

			terrainOutput.size = chunkSize;
			terrainOutput.texture = samplerTexture;
		}
	}
}