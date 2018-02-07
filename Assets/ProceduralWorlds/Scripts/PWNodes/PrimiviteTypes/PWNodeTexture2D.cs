using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeTexture2D : PWNode {

		[PWOutput("Texture 2D")]
		public Texture2D		outputTexture;

		[PWOutput("Material")]
		public Material			outputMaterial;

		[SerializeField]
		bool					isMaterialOutput = false;
		[SerializeField]
		Vector2					tiling = Vector2.one;
		[SerializeField]
		Vector2					offset;
		[SerializeField]
		bool					preview = true;

		PWGUIMaterialPreview	matPreview;

		public override void OnNodeCreation()
		{
			name = "Texture 2D";
		}

		public override void OnNodeEnable()
		{
			matPreview = new PWGUIMaterialPreview();
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight * 2 + 4);
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
					
					if ((preview = EditorGUILayout.Foldout(preview, "preview")))
						matPreview.Render(outputMaterial);
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
				SetAnchorVisibility("outputTexture", PWVisibility.Gone);
				SetAnchorVisibility("outputMaterial", PWVisibility.Visible);
				RemoveAllLinksFromAnchor("outputTexture");
			}
			else
			{
				SetAnchorVisibility("outputTexture", PWVisibility.Visible);
				SetAnchorVisibility("outputMaterial", PWVisibility.Gone);
				RemoveAllLinksFromAnchor("outputMaterial");
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
