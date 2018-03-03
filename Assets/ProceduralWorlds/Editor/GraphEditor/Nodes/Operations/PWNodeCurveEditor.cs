using UnityEditor;
using UnityEngine;
using PW.Node;
using PW.Editor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeCurve))]
	public class PWNodeCurveEditor : PWNodeEditor
	{
		PWNodeCurve		node;
		
		const string notifyKey = "curveModify";
		
		public override void OnNodeEnable()
		{
			node = target as PWNodeCurve;
			
			delayedChanges.BindCallback(notifyKey, (unused) => {
					NotifyReload();
					node.CurveTerrain();
					node.sCurve.SetAnimationCurve(node.curve);
				});
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight * 1.2f);
			EditorGUI.BeginChangeCheck();
			Rect pos = EditorGUILayout.GetControlRect(false, 100);
			node.curve = EditorGUI.CurveField(pos, node.curve);
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(notifyKey);

			PWGUI.SamplerPreview(node.outputTerrain);
		}
	}
}