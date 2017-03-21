using UnityEditor;

namespace PW
{
	public class PWNode2DSideViewTerrain : PWNode {
	
		[PWInput("TEX")]
		public Sampler2D	texture;

		public override void OnNodeCreate()
		{
			name = "2D SideView terrain";
		}

		public override void OnNodeGUI()
		{
			EditorGUILayout.LabelField("display map here");
		}

		public override void OnNodeProcess()
		{

		}

	}
}