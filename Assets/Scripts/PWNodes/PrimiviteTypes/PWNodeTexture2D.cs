using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeTexture2D : PWNode {

		[PWOutput("tex2D")]
		public Texture2D		outputTexture;

		[PWOutput("mat")]
		public Material			outputMaterial;

		[SerializeField]
		bool					isMaterialOutput = false;
		[SerializeField]
		Vector2					tiling = Vector2.one;
		[SerializeField]
		Vector2					offset;
		[SerializeField]
		bool					showSceneHiddenObjects = false;
		[SerializeField]
		bool					preview = true;

		PWGUIMaterialPreview	matPreview;

		public override void OnNodeCreate()
		{
			externalName = "Texture 2D";
			matPreview = new PWGUIMaterialPreview();
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);
			outputTexture = EditorGUILayout.ObjectField(outputTexture, typeof(Texture2D), false) as Texture2D;
			EditorGUI.BeginChangeCheck();
			{
				if ((isMaterialOutput = EditorGUILayout.Toggle("material output", isMaterialOutput)))
				{
					if (outputMaterial == null)
						CreateNewMaterial();
					tiling = EditorGUILayout.Vector2Field("tiling", tiling);
					offset = EditorGUILayout.Vector2Field("offset", offset);
	
					UpdateMaterialProperties();
					
					EditorGUI.BeginChangeCheck();
					showSceneHiddenObjects = EditorGUILayout.Toggle("Show scene hidden objects", showSceneHiddenObjects);
					if (EditorGUI.EndChangeCheck())
						matPreview.UpdateShowSceneHiddenObjects(showSceneHiddenObjects);
					
					if ((preview = EditorGUILayout.Foldout(preview, "preview")))
						matPreview.Render();
				}
				else if (outputTexture != null)
					if ((preview = EditorGUILayout.Foldout(preview, "preview")))
						PWGUI.TexturePreview(outputTexture);
			}
			if (EditorGUI.EndChangeCheck())
				UpdateProps();
		}

		void UpdateProps()
		{
			if (isMaterialOutput)
			{
				UpdatePropVisibility("outputTexture", PWVisibility.Gone);
				UpdatePropVisibility("outputMaterial", PWVisibility.Visible);
				RequestRemoveLink("outputTexture");
			}
			else
			{
				UpdatePropVisibility("outputTexture", PWVisibility.Visible);
				UpdatePropVisibility("outputMaterial", PWVisibility.Gone);
				RequestRemoveLink("outputMaterial");
			}
		}

		void CreateNewMaterial()
		{
			outputMaterial = new Material(Shader.Find("Unlit/Texture"));
			if (outputTexture != null)
				outputMaterial.SetTexture("_MainTex", outputTexture);
			UpdateMaterialProperties();
		}

		void UpdateMaterialProperties()
		{
			outputMaterial.SetTextureOffset("_MainTex", offset);
			outputMaterial.SetTextureScale("_MainTex", tiling);
			if (outputTexture != null)
				outputMaterial.SetTexture("_MainTex", outputTexture);
			matPreview.SetMaterial(outputMaterial);
		}

		public override void OnNodeProcess()
		{
			if (outputMaterial == null && isMaterialOutput)
				CreateNewMaterial();
		}

		public override void OnNodeDisable()
		{
			matPreview.Cleanup();
		}
	}
}
