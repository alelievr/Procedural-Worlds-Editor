using UnityEditor;
using UnityEngine;
using ProceduralWorlds.Node;
using ProceduralWorlds.Editor;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(NodePerlinNoise2D))]
	public class NodePerlinNoise2DEditor : BaseNodeEditor
	{
		NodePerlinNoise2D		node;
		
		const string noiseSettingsChangedKey = "PerlinNoiseSettings";

		public override void OnNodeEnable()
		{
			node = target as NodePerlinNoise2D;
			delayedChanges.BindCallback(noiseSettingsChangedKey, (unused) => NotifyReload());
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 40;
			EditorGUI.BeginChangeCheck();
			{
				node.persistence = PWGUI.Slider("persistence: ", node.persistence, ref node.persistenceMin, ref node.persistenceMax);
				node.lacunarity = PWGUI.Slider("Lacunarity: ", node.lacunarity, 0.1f, 5);
				node.octaves = PWGUI.IntSlider("Octaves: ", node.octaves, 0, 16);
				node.scale = PWGUI.Slider("Scale: ", node.scale, 0.01f, 10);
				node.additionalSeed = EditorGUILayout.IntField("Seed", node.additionalSeed);
			}
			if (EditorGUI.EndChangeCheck())
			{
				node.perlin2D.UpdateParams(node.GetSeed(), node.scale, node.octaves, node.persistence, node.lacunarity);
				delayedChanges.UpdateValue(noiseSettingsChangedKey);
			}

			PWGUI.Sampler2DPreview(node.output);
		}
	}
}