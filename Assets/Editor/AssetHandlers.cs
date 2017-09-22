using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using PW.Core;

public class NewBehaviourScript {

    [OnOpenAssetAttribute(1)]
	public static bool OnOpenAssetAttribute(int instanceId, int line)
	{
		Object instance = EditorUtility.InstanceIDToObject(instanceId);

		if (instance.GetType() == typeof(PWMainGraph))
		{
			//open PWNodeGraph window:
			PWMainGraphEditor window = (PWMainGraphEditor)EditorWindow.GetWindow(typeof(PWMainGraphEditor));
			window.graph = instance as PWGraph;
		}
		if (instance.GetType() == typeof(PWBiomeGraph))
		{
			//TODO: PWBiomeGraph editor
			// PWBiomeGraphEditor
		}
		return false;
	}
}
