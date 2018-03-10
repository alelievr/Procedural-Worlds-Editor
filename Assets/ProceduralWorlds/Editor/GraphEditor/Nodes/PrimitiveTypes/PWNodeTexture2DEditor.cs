using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Node;
using UnityEditor;

namespace PW.Editor
{
	[CustomEditor(typeof(PWNodeTexture2D))]
	public class PWNodeTexture2DEditor : PWNodeEditor
	{
		public PWNodeTexture2D	node;
		
		PWGUIMaterialPreview	matPreview;

		public override void OnNodeEnable()
		{
			node = target as PWNodeTexture2D;

			matPreview = new PWGUIMaterialPreview();
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight * 2 + 4);
			node.outputTexture = EditorGUILayout.ObjectField(node.outputTexture, typeof(Texture2D), false) as Texture2D;
			EditorGUI.BeginChangeCheck();
			{
				node.isMaterialOutput = EditorGUILayout.Toggle("material output", node.isMaterialOutput);
				
				if (node.isMaterialOutput)
				{
					if (node.outputMaterial == null)
						node.CreateNewMaterial();
					node.tiling = EditorGUILayout.Vector2Field("tiling", node.tiling);
					node.offset = EditorGUILayout.Vector2Field("offset", node.offset);
	
					node.UpdateMaterialProperties();
					
					if ((node.preview = EditorGUILayout.Foldout(node.preview, "preview")))
						matPreview.Render(node.outputMaterial);
				}
				else if (node.outputTexture != null)
					if ((node.preview = EditorGUILayout.Foldout(node.preview, "preview")))
						PWGUI.TexturePreview(node.outputTexture);
			}
			if (EditorGUI.EndChangeCheck())
				node.UpdateProps();
		}

		public override void OnNodeDisable()
		{
			matPreview.Cleanup();
		}
	}
}