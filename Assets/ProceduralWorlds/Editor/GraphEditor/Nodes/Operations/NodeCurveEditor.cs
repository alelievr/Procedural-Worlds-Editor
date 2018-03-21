using UnityEditor;
using UnityEngine;
using ProceduralWorlds.Node;
using ProceduralWorlds.Editor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodeCurve))]
	public class NodeCurveEditor : BaseNodeEditor
	{
		NodeCurve		node;
		
		const string notifyKey = "curveModify";
		
		public override void OnNodeEnable()
		{
			node = target as NodeCurve;
			
			delayedChanges.BindCallback(notifyKey, (unused) => {
					NotifyReload();
					node.CurveTerrain();
					node.sCurve.SetAnimationCurve(node.curve);
				});
		}

		public override void OnNodeGUI()
		{
			PWGUI.SpaceSkipAnchors();

			EditorGUI.BeginChangeCheck();
			{
				Rect pos = EditorGUILayout.GetControlRect(false, 100);
				node.curve = EditorGUI.CurveField(pos, node.curve);
			}
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(notifyKey);

			PWGUI.SamplerPreview(node.outputTerrain);
		}
	}
}