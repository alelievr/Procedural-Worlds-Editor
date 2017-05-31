using UnityEditor;
using UnityEngine;
using PW.Core;

namespace PW.Node
{
	public class PWNodeTopDown2DTerrain : PWNode {
	
		[PWInput]
		public Sampler2D		texture;

		[PWOutput]
		public TopDown2DData	terrainOutput;

		private Texture2D		samplerTexture;

		public override void OnNodeCreate()
		{
			name = "2D TopDown terrain";
			samplerTexture = new Texture2D(chunkSize, chunkSize, TextureFormat.ARGB32, false, false);
			texture = new Sampler2D(chunkSize, step);
			terrainOutput = new TopDown2DData();
		}

		public override void OnNodeGUI()
		{
			EditorGUILayout.LabelField("MAP:");

			PWGUI.TexturePreview(samplerTexture);
		}

		public override bool OnNodeAnchorLink(string propName, int index)
		{
			if (propName == "texture")
				samplerTexture = new Texture2D(texture.size, texture.size, TextureFormat.ARGB32, false, false);
			
			return true;
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
