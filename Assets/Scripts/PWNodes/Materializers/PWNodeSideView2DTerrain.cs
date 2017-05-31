using UnityEditor;
using UnityEngine;
using PW.Core;

namespace PW.Node
{
	public class PWNodeSideView2DTerrain : PWNode {
	
		[PWInput]
		public Sampler2D		texture;

		[PWOutput]
		public SideView2DData	terrainOutput;

		public override void OnNodeCreate()
		{
			name = "2D SideView terrain";
			texture = new Sampler2D(chunkSize, step);
		}

		public override void OnNodeGUI()
		{
			EditorGUILayout.LabelField("MAP:");
			
			if (chunkSizeHasChanged)
				texture = new Sampler2D(chunkSize, step);
			
			PWGUI.Sampler2DPreview("perlinControlName", texture, needUpdate);
		}

		public override void OnNodeProcess()
		{
			terrainOutput.size = chunkSize;
			// terrainOutput.texture = samplerTexture;
		}

	}
}
