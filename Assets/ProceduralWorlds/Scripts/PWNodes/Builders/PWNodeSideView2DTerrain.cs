using UnityEditor;
using UnityEngine;
using PW.Core;

namespace PW.Node
{
	public class PWNodeSideView2DTerrain : PWNode {
	
		[PWInput]
		public Sampler2D		terrain;

		[PWOutput]
		public SideView2DData	terrainOutput;

		public override void OnNodeCreation()
		{
			name = "2D SideView terrain";
		}

		public override void OnNodeGUI()
		{
			if (terrain == null)
			{
				EditorGUILayout.LabelField("Please connect the input to a terrain !");
			}
			EditorGUILayout.LabelField("MAP:");
			
			PWGUI.Sampler2DPreview("perlinControlName", terrain);
		}

		public override void OnNodeProcess()
		{
			terrainOutput.size = chunkSize;
		}

	}
}
