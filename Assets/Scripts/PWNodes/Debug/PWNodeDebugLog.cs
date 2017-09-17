using UnityEditor;
using UnityEngine;
using System;
using PW.Core;

namespace PW.Node
{
	public class PWNodeDebugLog : PWNode {
	
		[PWInput]
		public object		obj;

		public override void OnNodeCreation()
		{
			name = "Debug log node";
			renamable = true;
			obj = "null";
		}

		public override void OnNodeGUI()
		{
			if (obj != null)
			{
				Type	objType = obj.GetType();
				EditorGUILayout.LabelField(obj.ToString());
				if (objType == typeof(PWValues))
				{
					var pwv = obj as PWValues;

					for (int i = 0; i < pwv.Count; i++)
					{
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("[" + i + "] " + pwv.NameAt(i) + ": " + pwv.At(i), GUILayout.Width(300));
						EditorGUILayout.EndHorizontal();
					}
				}
				else if (objType == typeof(Vector2))
					EditorGUILayout.Vector2Field("vec2", (Vector2)obj);
				else if (objType == typeof(Vector3))
					EditorGUILayout.Vector2Field("vec3", (Vector3)obj);
				else if (objType == typeof(Vector4))
					EditorGUILayout.Vector2Field("vec4", (Vector4)obj);
				else if (objType == typeof(Sampler2D))
				{
					//TODO
				}
				else if (objType == typeof(Sampler3D))
				{
					//TODO
				}
				else if (objType == typeof(BiomeData))
				{
					BiomeUtils.DrawBiomeInfos(obj as BiomeData);
				}
			}
			else
				EditorGUILayout.LabelField("null");
		}

		//no process needed, no output.
	}
}
