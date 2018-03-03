using UnityEditor;
using UnityEngine;
using PW.Node;
using PW.Editor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodePerlinNoise2D))]
	public class PWNodePerlinNoise2DEditor : PWNodeEditor
	{
		PWNodePerlinNoise2D		node;
		
		const string noiseSettingsChangedKey = "PerlinNoiseSettings";

		public override void OnNodeEnable()
		{
			node = target as PWNodePerlinNoise2D;
			delayedChanges.BindCallback(noiseSettingsChangedKey, (unused) => NotifyReload());
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 40;
			EditorGUI.BeginChangeCheck();
			{
				node.persistance = PWGUI.Slider("Persistance: ", node.persistance, ref node.persistanceMin, ref node.persistanceMax);
				node.octaves = PWGUI.IntSlider("Octaves: ", node.octaves, 0, 16);
				node.scale = PWGUI.Slider("Scale: ", node.scale, 0.01f, 10);
				node.additionalSeed = EditorGUILayout.IntField("Seed", node.additionalSeed);
			}
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(noiseSettingsChangedKey);

			PWGUI.Sampler2DPreview(node.output);
		}
	}
}